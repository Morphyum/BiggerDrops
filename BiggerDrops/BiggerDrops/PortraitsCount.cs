using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Reflection;
using UnityEngine;

namespace BiggerDrops {
  /*[HarmonyPatch(typeof(PathNodeGrid))]
  [HarmonyPatch("ResetPathGrid")]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(float), typeof(PathingCapabilitiesDef), typeof(float), typeof(MoveType) })]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDMechwarriorTray_ResetPathGrid {
    public static bool Prefix(PathNodeGrid __instance,ref CombatGameState ___combat) {
      Logger.M.TWL(0, "PathNodeGrid.ResetPathGrid");
      if (___combat == null) {
        Logger.M.WL(1, "combat is null ... skipping");
        return false;
      } else {
        Logger.M.WL(1, "combat is not null");
      }
      return true;
    }
  }*/
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("SetTrayState")]
  [HarmonyPatch(MethodType.Normal)]
  public static class Init {
    public static bool Prefix(CombatHUDMechwarriorTray __instance) {
      Logger.M.TWL(0, "CombatHUDMechwarriorTray.SetTrayState");
      try {
        /*for (int index = 0; index < __instance.PortraitHolders.Length; ++index) {
          Vector3[] corners = new Vector3[4];
          RectTransform prectt = __instance.PortraitHolders[index].GetComponent<RectTransform>();
          prectt.GetLocalCorners(corners);
          Logger.M.WL(1, "portrait "+ prectt.name+ ":" + __instance.PortraitHolders[index].GetInstanceID() + ". index:" + index + " pos:" +prectt.localPosition+" corners 0:" + corners[0] + " 1:" + corners[1] + " 2:" + corners[2] + " 3:" + corners[3]);
        }*/
        if (__instance.PortraitHolders.Length <= 4) { return true; }
        CombatHUDMoraleBar combatHUDMoraleBar = (CombatHUDMoraleBar)typeof(CombatHUDMechwarriorTray).GetProperty("moraleDisplay", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance, new object[0] { });
        RectTransform rtr = combatHUDMoraleBar.gameObject.GetComponent<RectTransform>();
        RectTransform prt = __instance.PortraitHolders[0].GetComponent<RectTransform>();
        Vector3[] pcorners = new Vector3[4];
        prt.GetLocalCorners(pcorners);
        Logger.M.WL(0, "portrait corners: bl:" + pcorners[0] + " tl:" + pcorners[1] + " tr:" + pcorners[2] + " br:" + pcorners[3]);
        Logger.M.WL(0, "moraleDisplay local pos:" + rtr.localPosition);
        Vector3 pos = rtr.localPosition;
        pos.x = pcorners[0].x + prt.localPosition.x - 10f;
        rtr.localPosition = pos;
        Logger.M.WL(0, "moraleDisplay local pos:" + rtr.localPosition);
        Logger.M.WL(0, "CombatHUDMechwarriorTray.SetTrayState");
        combatHUDMoraleBar.gameObject.SetActive(true);
      } catch(Exception e) {
        Logger.M.TWL(0, e.ToString());
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("RefreshTeam")]
  [HarmonyPatch(new Type[] { typeof(Team) })]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDMechwarriorTray_RefreshTeam {
    public static bool Prefix(CombatHUDMechwarriorTray __instance, Team team, CombatGameState ___Combat) {
      try {
        //__instance.displayedTeam = team;
        Logger.M.TWL(0, "CombatHUDMechwarriorTray.RefreshTeam team:"+team.DisplayName+ " unitCount:" + team.unitCount);
        foreach(var lance in ___Combat.ActiveContract.Lances.Lances) {
          Logger.M.WL(1, "lance:"+lance.Key+" "+lance.Value.Count);
          foreach(var unit in lance.Value) {
            Logger.M.WL(2, "pilot:"+unit.Pilot.Description.Id+" unit:"+(unit.Unit!=null?unit.Unit.Description.Id:"not a mech")+" team:"+unit.TeamDefinitionGuid);
          }
        }
        typeof(CombatHUDMechwarriorTray).GetField("displayedTeam", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, team);
        CombatHUDPortrait[] Portraits = (CombatHUDPortrait[])typeof(CombatHUDMechwarriorTray).GetField("Portraits", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        for (int index = 0; index < Portraits.Length; ++index) {
          Logger.M.WL(1, "set portrait "+index+" unit:" + (index < team.unitCount? team.units[index].DisplayName:"null"));
          if (index < team.unitCount) {
            Portraits[index].DisplayedActor = team.units[index];
          } else {
            Portraits[index].DisplayedActor = null;
          }
        }
        return false;
      } catch (Exception e) {
        Logger.LogLine(e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD) })]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDMechwarriorTray_Init {
    public static bool Prefix(CombatHUDMechwarriorTray __instance, CombatGameState Combat, CombatHUD HUD) {
      Logger.M.TWL(0,"CombatHUDMechwarriorTray.Init prefix");
      if (BiggerDrops.settings.additinalPlayerMechSlots == 0) {
        Logger.M.WL(1, "no additional portraits needed");
        return true;
      }
      try {
        //int portraitsCount = 8;
        int portraitsCount = Combat.LocalPlayerTeam.unitCount > (Settings.DEFAULT_MECH_SLOTS + Settings.MAX_ADDITINAL_MECH_SLOTS) ?
                                                    Settings.DEFAULT_MECH_SLOTS + Settings.MAX_ADDITINAL_MECH_SLOTS : Combat.LocalPlayerTeam.unitCount;
        if (__instance.PortraitHolders.Length >= portraitsCount) {
          Logger.M.WL(1, "no additional portraits needed");
          return true;
        }
        GameObject[] portraitHolders = new GameObject[portraitsCount];
        HBSDOTweenToggle[] portraitTweens = new HBSDOTweenToggle[portraitsCount];
        GameObject layout = __instance.PortraitHolders[0].transform.parent.gameObject;
        //layout.transform.localScale 
        //GameObject elayout = GameObject.Instantiate(layout);
        //elayout.transform.localPosition += Vector3.up * (-30.0f);
        for (int index = 0; index < portraitHolders.Length; ++index) {
          if (index < __instance.PortraitHolders.Length) {
            portraitHolders[index] = __instance.PortraitHolders[index];
            continue;
          }
          GameObject srcPortraitHolder = portraitHolders[index%(__instance.PortraitHolders.Length)];
          GameObject newPortraitHolder = GameObject.Instantiate(srcPortraitHolder, srcPortraitHolder.transform.parent);
          Vector3[] corners = new Vector3[4];
          srcPortraitHolder.GetComponent<RectTransform>().GetWorldCorners(corners);
          float height = corners[2].z - corners[0].z;
          //Logger.M.WL(1, "portrait. index:" + index + " corners 0:" + corners[0] + " 1:" + corners[1] + " 2:" + corners[2] + " 3:" + corners[3]);
          foreach (Component component in srcPortraitHolder.GetComponentsInChildren<Component>()) {
            Logger.M.WL(2, component.name+":"+component.GetType()+":"+component.GetInstanceID());
          }
          newPortraitHolder.SetActive(true);
          portraitHolders[index] = newPortraitHolder;
          newPortraitHolder.GetComponent<RectTransform>().GetWorldCorners(corners);
        }
        __instance.PortraitHolders = portraitHolders;
        float spacing = 117.2f;
        if(__instance.portraitTweens.Length > 1) {
          Vector3[] corners0 = new Vector3[4];
          __instance.portraitTweens[0].gameObject.GetComponent<RectTransform>().GetWorldCorners(corners0);
          Vector3[] corners1 = new Vector3[4];
          __instance.portraitTweens[1].gameObject.GetComponent<RectTransform>().GetWorldCorners(corners1);
          spacing = corners1[0].x - corners0[0].x;
        }
        float diff = 0f;
        Vector3[] cornersl = new Vector3[4];
        __instance.portraitTweens[__instance.portraitTweens.Length - 1].gameObject.GetComponent<RectTransform>().GetWorldCorners(cornersl);
        Vector3[] cornersf = new Vector3[4];
        __instance.portraitTweens[0].gameObject.GetComponent<RectTransform>().GetWorldCorners(cornersf);
        diff = cornersl[0].x - cornersf[0].x + spacing;
        for (int index = 0; index < portraitTweens.Length; ++index) {
          if (index < __instance.portraitTweens.Length) {
            portraitTweens[index] = __instance.portraitTweens[index];
            continue;
          }
          HBSDOTweenToggle srcPortraitTween = portraitTweens[index % (__instance.portraitTweens.Length)];
          GameObject newPortraitTweenGO = GameObject.Instantiate(srcPortraitTween.gameObject, srcPortraitTween.gameObject.transform.parent);
          HBSDOTweenToggle newPortraitTween = newPortraitTweenGO.GetComponent<HBSDOTweenToggle>();
          newPortraitTweenGO.transform.localPosition += Vector3.right * diff;
          newPortraitTween.TweenObjects[0] = portraitHolders[index];
          //Logger.M.WL(1, "tween. index:" + index + " corners 0:" + corners[0] + " 1:" + corners[1] + " 2:" + corners[2] + " 3:" + corners[3]);
          portraitTweens[index] = newPortraitTween;
          //newPortraitTween.GetComponent<RectTransform>().GetWorldCorners(corners);
          //Logger.M.WL(1, "tween. index:" + (__instance.portraitTweens.Length + index) + " corners 0:" + corners[0] + " 1:" + corners[1] + " 2:" + corners[2] + " 3:" + corners[3]);
        }
        __instance.portraitTweens = portraitTweens;
        return true;
      } catch (Exception e) {
        Logger.M.TWL(0,e.ToString(),true);
        return true;
      }
    }
    public static void Postfix(CombatHUDMechwarriorTray __instance, CombatGameState Combat, CombatHUD HUD) {
      Logger.M.TWL(0, "CombatHUDMechwarriorTray.Init postfix");
      try {
        for (int index = 0; index < __instance.PortraitHolders.Length; ++index) {
          Vector3[] corners = new Vector3[4];
          __instance.PortraitHolders[index].GetComponent<RectTransform>().GetWorldCorners(corners);
          Logger.M.WL(1, "portrait "+ __instance.PortraitHolders[index].GetInstanceID()+ ". index:" + index + " corners 0:" + corners[0] + " 1:" + corners[1] + " 2:" + corners[2] + " 3:" + corners[3]);
          foreach (Component component in __instance.PortraitHolders[index].GetComponentsInChildren<Component>()) {
            Logger.M.WL(3, component.name + ":" + component.GetType() + ":" + component.GetInstanceID());
          }
        }
        for (int index = 0; index < __instance.portraitTweens.Length; ++index) {
          Vector3[] corners = new Vector3[4];
          __instance.portraitTweens[index].gameObject.GetComponent<RectTransform>().GetWorldCorners(corners);
          Logger.M.WL(1, "tween " + __instance.portraitTweens[index].gameObject.GetInstanceID() + ". index:" + index + " corners 0:" + corners[0] + " 1:" + corners[1] + " 2:" + corners[2] + " 3:" + corners[3]);
          foreach (Component component in __instance.portraitTweens[index].gameObject.GetComponentsInChildren<Component>()) {
            Logger.M.WL(3, component.name + ":" + component.GetType() + ":" + component.GetInstanceID());
          }
        }
        Logger.M.WL(0, "Moralebar diactivate");
        CombatHUDMoraleBar combatHUDMoraleBar = (CombatHUDMoraleBar)typeof(CombatHUDMechwarriorTray).GetProperty("moraleDisplay", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance, new object[0] { });
        combatHUDMoraleBar.gameObject.SetActive(false);
      } catch (Exception e) {
        Logger.M.TWL(0, e.ToString(), true);
      }
    }
  }
}