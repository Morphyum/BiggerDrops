using Harmony;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace BiggerDrops {
  public static class CustomUnitsAPI {
    private static bool CustomUnits_detected = false;
    private static MethodInfo m_setLancesCount = null;
    private static MethodInfo m_setLanceData = null;
    private static MethodInfo m_setOverallDeployCount = null;
    private static MethodInfo m_playerControl = null;
    private static MethodInfo m_setMechBayCount = null;
    public static bool Detected() { return CustomUnits_detected; }
    public static void setLancesCount(int count) { if (m_setLancesCount != null) { m_setLancesCount.Invoke(null, new object[] { count }); }; }
    public static void setLanceData(int lanceid, int size, int allow, bool is_vehicle) { if (m_setLanceData != null) { m_setLanceData.Invoke(null, new object[] { lanceid, size, allow, is_vehicle }); }; }
    public static void setOverallDeployCount(int count) { if (m_setOverallDeployCount != null) { m_setOverallDeployCount.Invoke(null, new object[] { count }); }; }
    public static void playerControl(int mechs, int vehicles) { if (m_playerControl != null) { m_playerControl.Invoke(null, new object[] { mechs, vehicles }); }; }
    public static void setMechBayCount(int count) { if (m_setMechBayCount != null) { m_setMechBayCount.Invoke(null, new object[] { count }); }; }
    public static void CustomUnitsDetected() {
      Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
      Logger.M.TWL(0, "CustomUnitsAPI.CustomUnitsDetected");
      foreach (Assembly assembly in assemblies) {
        //Logger.M.WL(1, assembly.FullName);
        if (assembly.FullName.StartsWith("CustomUnits")) {
          Logger.M.WL(1, assembly.FullName);
          Type helperType = assembly.GetType("CustomUnits.CustomLanceHelper");
          if(helperType != null) {
            m_setLancesCount = helperType.GetMethod("setLancesCount", BindingFlags.Static | BindingFlags.Public);
            if (m_setLancesCount == null) { Logger.M.WL(2, "setLancesCount not found"); } else { Logger.M.WL(2, "setLancesCount found"); }
            m_setLanceData = helperType.GetMethod("setLanceData", BindingFlags.Static | BindingFlags.Public);
            if (m_setLanceData == null) { Logger.M.WL(2, "setLanceData not found"); } else { Logger.M.WL(2, "setLanceData found"); }
            m_setOverallDeployCount = helperType.GetMethod("setOverallDeployCount", BindingFlags.Static | BindingFlags.Public);
            if (m_setOverallDeployCount == null) { Logger.M.WL(2, "setOverallDeployCount not found"); } else { Logger.M.WL(2, "setOverallDeployCount found"); }
            m_playerControl = helperType.GetMethod("playerControl", BindingFlags.Static | BindingFlags.Public);
            if (m_playerControl == null) { Logger.M.WL(2, "playerControl not found"); } else { Logger.M.WL(2, "playerControl found"); }
            m_setMechBayCount = helperType.GetMethod("BaysCount", BindingFlags.Static | BindingFlags.Public);
            if (m_setMechBayCount == null) { Logger.M.WL(2, "BaysCount not found"); } else { Logger.M.WL(2, "BaysCount found"); }
            CustomUnits_detected = true;
            break;
          }
        }
      }
    }

  }
  public class BiggerDrops {
    internal static string ModDirectory;
    public static Settings settings;
    public static int baysAlreadyAdded = 0;
    public static void FinishedLoading(List<string> loadOrder) {
      Logger.M.TWL(0, "FinishedLoading", true);
      try {
        foreach (string name in loadOrder) { if (name == "CustomUnits") { CustomUnitsAPI.CustomUnitsDetected(); break; }; }
      } catch (Exception e) {
        Logger.M.TWL(0, e.ToString(), true);
      }
    }

    public static void Init(string directory, string settingsJSON) {
      BiggerDrops.ModDirectory = directory;
      Logger.BaseDirectory = directory;
      try {
        settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path.Combine(directory,"settings.json")));
        Logger.InitLog();
        Logger.M.TWL(0,"BiggerDrop log inited...",true);
      } catch (Exception e) {
        settings = new Settings();
        Logger.InitLog();
        Logger.M.TWL(0, "BiggerDrop log init exception "+e.ToString(), true);
      }
      try {
        var harmony = HarmonyInstance.Create("de.morphyum.BiggerDrops");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        ModDirectory = directory;
      } catch (Exception e) {
        Logger.M.TWL(0, e.ToString());
      }
    }
  }
}
