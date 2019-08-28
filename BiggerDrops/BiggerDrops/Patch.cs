using BattleTech;
using BattleTech.Save;
using BattleTech.Save.SaveGameStructure;
using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BiggerDrops {

    [HarmonyPatch(typeof(GameInstanceSave), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(GameInstance), typeof(SaveReason) })]
    public static class GameInstanceSave_Constructor_Patch {
        static void Postfix(GameInstanceSave __instance) {
            Helper.SaveState(__instance.InstanceGUID, __instance.SaveTime);
        }
    }

    [HarmonyPatch(typeof(GameInstance), "Load")]
    public static class GameInstance_Load_Patch {
        static void Prefix(GameInstanceSave save) {
            Helper.LoadState(save.InstanceGUID, save.SaveTime);
        }
    }

    [HarmonyPatch(typeof(SGCmdCenterLanceConfigBG), "OnAddedToHierarchy")]
    public static class SGCmdCenterLanceConfigBG_OnAddedToHierarchy {
        static void Postfix(SGCmdCenterLanceConfigBG __instance) {
            try {
                GameObject primelayout = __instance.LC.transform.FindRecursive("uixPrfPanel_LC_LanceSlots-Widget-MANAGED").gameObject;
                GameObject newLayout = GameObject.Instantiate(primelayout);
                newLayout.transform.parent = primelayout.transform.parent;
                newLayout.name = "AlliedSlots";
                GameObject slot1 = newLayout.transform.FindRecursive("lanceSlot1").gameObject;
                GameObject slot2 = newLayout.transform.FindRecursive("lanceSlot2").gameObject;
                GameObject slot3 = newLayout.transform.FindRecursive("lanceSlot3").gameObject;
                GameObject slot4 = newLayout.transform.FindRecursive("lanceSlot4").gameObject;
                primelayout.transform.FindRecursive("simbg").gameObject.active = false;
                newLayout.transform.FindRecursive("simbg").gameObject.active = false;
                newLayout.transform.FindRecursive("layout-lanceRating").gameObject.active = false;
                newLayout.transform.FindRecursive("lanceSlotHeader-Campaign").gameObject.active = true;
                newLayout.transform.FindRecursive("txt-unreadyLanceError").gameObject.active = false;
                TextMeshProUGUI aiText = newLayout.transform.FindRecursive("label-readyLanceHeading").gameObject.GetComponent<TextMeshProUGUI>();
                aiText.text = "AI LANCE";
                primelayout.transform.position = new Vector3(650, 315, primelayout.transform.position.z);
                primelayout.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                newLayout.transform.position = new Vector3(650, 83, primelayout.transform.position.z);
                newLayout.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

                GameObject deployButton = __instance.LC.transform.FindRecursive("DeployBttn-layout").gameObject;
                deployButton.transform.position = new Vector3(1675, 175, deployButton.transform.position.z);
                
                LanceLoadoutSlot[] loadoutSlots = (LanceLoadoutSlot[]) AccessTools.Field(typeof(LanceConfiguratorPanel), "loadoutSlots").GetValue(__instance.LC);
                List<LanceLoadoutSlot> list = loadoutSlots.ToList();
                list.Add(slot1.GetComponent<LanceLoadoutSlot>());
                list.Add(slot2.GetComponent<LanceLoadoutSlot>());
                list.Add(slot3.GetComponent<LanceLoadoutSlot>());
                list.Add(slot4.GetComponent<LanceLoadoutSlot>());
                loadoutSlots = list.ToArray<LanceLoadoutSlot>();
                AccessTools.Field(typeof(LanceConfiguratorPanel), "loadoutSlots").SetValue(__instance.LC, loadoutSlots);

                float[] slotMaxTonnages = (float[])AccessTools.Field(typeof(LanceConfiguratorPanel), "slotMaxTonnages").GetValue(__instance.LC);
                float[] slotMinTonnages = (float[])AccessTools.Field(typeof(LanceConfiguratorPanel), "slotMinTonnages").GetValue(__instance.LC);
                List<float> listMaxTonnages = slotMaxTonnages.ToList();
                List<float> listMinTonnages = slotMinTonnages.ToList();
                listMaxTonnages.Add(-1);
                listMaxTonnages.Add(-1);
                listMaxTonnages.Add(-1);
                listMaxTonnages.Add(-1);
                listMinTonnages.Add(-1);
                listMinTonnages.Add(-1);
                listMinTonnages.Add(-1);
                listMinTonnages.Add(-1);
                slotMaxTonnages = listMaxTonnages.ToArray<float>();
                slotMinTonnages = listMinTonnages.ToArray<float>();
                AccessTools.Field(typeof(LanceConfiguratorPanel), "slotMaxTonnages").SetValue(__instance.LC, slotMaxTonnages);
                AccessTools.Field(typeof(LanceConfiguratorPanel), "slotMinTonnages").SetValue(__instance.LC, slotMinTonnages);

            }
            catch (Exception e) {
                Logger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(LanceConfiguratorPanel), "SetData")]
    public static class LanceConfiguratorPanel_SetData {
        static void Prefix(LanceConfiguratorPanel __instance, ref int maxUnits) {
            try {
                maxUnits = 8;
            }
            catch (Exception e) {
                Logger.LogError(e);
            }
        }

        static void Postfix(LanceConfiguratorPanel __instance, Contract contract) {
            try {
                if(contract.Override.lanceMaxTonnage == -1) {
                    __instance.lanceMaxTonnage = 500;
                }
            }
            catch (Exception e) {
                Logger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(Contract), "CompleteContract")]
    public static class Contract_CompleteContract {
        static void Prefix(Contract __instance) {
            try {
                CombatGameState combat = __instance.BattleTechGame.Combat;
                List<Mech> allMechs = combat.AllMechs;
                foreach(Mech mech in allMechs) {
                    if(Fields.callsigns.Contains(mech.pilot.Callsign)) {
                        AccessTools.Field(typeof(Mech), "_teamId").SetValue(mech, combat.LocalPlayerTeam.GUID);
                    }
                }

            }
            catch (Exception e) {
                Logger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(AAR_UnitsResult_Screen), "InitializeData")]
    public static class AAR_UnitsResult_Screen_InitializeData {
        static bool Prefix(AAR_UnitsResult_Screen __instance, MissionResults mission, SimGameState sim, Contract contract) {
            try {
                List<AAR_UnitStatusWidget> UnitWidgets = (List<AAR_UnitStatusWidget>)AccessTools.Field(typeof(AAR_UnitsResult_Screen), "UnitWidgets").GetValue(__instance);
                GameObject nextButton = __instance.transform.FindRecursive("buttonPanel").gameObject;
                nextButton.transform.localPosition = new Vector3(150, 400, 0);

                Transform parent = UnitWidgets[0].transform.parent;
                parent.localPosition = new Vector3(0, 115, 0);
                foreach (AAR_UnitStatusWidget oldWidget in UnitWidgets) {
                    oldWidget.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                }
                GameObject newparent = GameObject.Instantiate(parent.gameObject);
                newparent.transform.parent = parent.parent;
                newparent.name = "newparent";
                newparent.transform.localPosition = new Vector3(0, -325, 0);
                foreach (Transform t in newparent.transform) {
                    UnitWidgets.Add(t.gameObject.GetComponent<AAR_UnitStatusWidget>());
                }
                AccessTools.Field(typeof(AAR_UnitsResult_Screen), "UnitWidgets").SetValue(__instance, UnitWidgets);

                List<UnitResult> UnitResults = new List<UnitResult>();
                for (int i = 0; i < 8; i++) {
                    if (i < contract.PlayerUnitResults.Count) {
                        UnitResults.Add(contract.PlayerUnitResults[i]);
                    }
                    else {
                        UnitResults.Add(null);
                    }
                }
                AccessTools.Field(typeof(AAR_UnitsResult_Screen), "simState").SetValue(__instance, sim);
                AccessTools.Field(typeof(AAR_UnitsResult_Screen), "missionResultParent").SetValue(__instance, mission);
                AccessTools.Field(typeof(AAR_UnitsResult_Screen), "theContract").SetValue(__instance, contract);
                AccessTools.Field(typeof(AAR_UnitsResult_Screen), "numUnits").SetValue(__instance, contract.PlayerUnitResults.Count);
                AccessTools.Field(typeof(AAR_UnitsResult_Screen), "UnitResults").SetValue(__instance, UnitResults);
                __instance.Visible = false;
                __instance.InitializeWidgets();
                return false;
            }
            catch (Exception e) {
                Logger.LogError(e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(AAR_UnitsResult_Screen), "FillInData")]
    public static class AAR_UnitsResult_Screen_FillInData {
        static bool Prefix(AAR_UnitsResult_Screen __instance) {
            try {
                Contract theContract = (Contract)AccessTools.Field(typeof(AAR_UnitsResult_Screen), "theContract").GetValue(__instance);
                List<AAR_UnitStatusWidget> UnitWidgets = (List<AAR_UnitStatusWidget>)AccessTools.Field(typeof(AAR_UnitsResult_Screen), "UnitWidgets").GetValue(__instance);
                List<UnitResult> UnitResults = (List<UnitResult>)AccessTools.Field(typeof(AAR_UnitsResult_Screen), "UnitResults").GetValue(__instance);
                int experienceEarned = theContract.ExperienceEarned;
                for (int i = 0; i < 8; i++) {
                    UnitWidgets[i].SetMechIconValueTextActive(false);
                    if (UnitResults[i] != null) {
                        UnitWidgets[i].SetNoUnitDeployedOverlayActive(false);
                        UnitWidgets[i].FillInData(experienceEarned);
                    }
                    else {
                        UnitWidgets[i].SetNoUnitDeployedOverlayActive(true);
                    }
                }
                AccessTools.Field(typeof(AAR_UnitsResult_Screen), "UnitWidgets").SetValue(__instance, UnitWidgets);
                return false;
            }
            catch (Exception e) {
                Logger.LogError(e);
                return true;
            }
        }
    }
    

    [HarmonyPatch(typeof(LanceConfiguratorPanel), "CreateLanceConfiguration")]
    public static class LanceConfiguratorPanel_CreateLanceConfiguration {
        static bool Prefix(LanceConfiguratorPanel __instance, ref LanceConfiguration __result) {
            try {
                return false;
            }
            catch (Exception e) {
                Logger.LogError(e);
                return false;
            }
        }

        static void Postfix(LanceConfiguratorPanel __instance, ref LanceConfiguration __result, LanceLoadoutSlot[] ___loadoutSlots) {
            try {
                Fields.callsigns.Clear();
                LanceConfiguration lanceConfiguration = new LanceConfiguration();
                for (int i = 0; i < ___loadoutSlots.Length; i++) {
                    LanceLoadoutSlot lanceLoadoutSlot = ___loadoutSlots[i];
                    MechDef mechDef = null;
                    PilotDef pilotDef = null;
                    if (lanceLoadoutSlot.SelectedMech != null) {
                        mechDef = lanceLoadoutSlot.SelectedMech.MechDef;
                    }
                    if (lanceLoadoutSlot.SelectedPilot != null) {
                        pilotDef = lanceLoadoutSlot.SelectedPilot.Pilot.pilotDef;
                    }
                    if (mechDef != null && pilotDef != null) {
                        if (i < 4) {
                            lanceConfiguration.AddUnit(__instance.playerGUID, mechDef, pilotDef);
                        } else {
                            Settings settings = Helper.LoadSettings();
                            if (!settings.experimentalPlayerLance) {
                                Fields.callsigns.Add(pilotDef.Description.Callsign);
                                //EMPLOYER ID
                                lanceConfiguration.AddUnit("ecc8d4f2-74b4-465d-adf6-84445e5dfc230", mechDef, pilotDef);
                            } else {
                                lanceConfiguration.AddUnit(__instance.playerGUID, mechDef, pilotDef);
                            }
                        }
                    }
                }
                __result = lanceConfiguration;
            }
            catch (Exception e) {
                Logger.LogError(e);
            }
        }
    }

   /* [HarmonyPatch(typeof(PlayerLanceSpawnerGameLogic), "ContractInitialize")]
    public static class PlayerLanceSpawnerGameLogic_ContractInitialize {
        static void Prefix(PlayerLanceSpawnerGameLogic __instance) {
            try {
                UnitSpawnPointGameLogic[] unitSpawnPointGameLogicList = __instance.unitSpawnPointGameLogicList;
                SpawnableUnit[] lanceUnits = __instance.Combat.ActiveContract.Lances.GetLanceUnits("ecc8d4f2-74b4-465d-adf6-84445e5dfc230");
                int num = 1;
                while (num < lanceUnits.Length && num < unitSpawnPointGameLogicList.Length) {
                    unitSpawnPointGameLogicList[num].OverrideSpawn(lanceUnits[num]);
                    num++;
                }
            }
            catch (Exception e) {
                Logger.LogError(e);
            }
        }
    }*/
}