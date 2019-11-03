using Newtonsoft.Json;
using System.Collections.Generic;
using BattleTech;

namespace BiggerDrops {
  public class Settings {
    public static readonly int MAX_ADDITINAL_MECH_SLOTS = 4;
    public static readonly int DEFAULT_MECH_SLOTS = 4;
    public static readonly string EMPLOYER_LANCE_GUID = "ecc8d4f2-74b4-465d-adf6-84445e5dfc230";
    public static readonly string ADDITIONAL_MECH_STAT = "BiggerDrops_AdditionalMechSlots";
    public static readonly string ADDITIONAL_PLAYER_MECH_STAT = "BiggerDrops_AdditionalPlayerMechSlots";
    public static readonly string MAX_TONNAGE_STAT = "BiggerDrops_MaxTonnage";
    public bool debugLog { get; set; }
    public int skirmishMax { get; set; }
    public string additionalLanceName { get; set; }
    public bool allowUpgrades { get; set; }
    public bool showArgoUpgrades { get; set; }
    public int additinalMechSlots {
      get {
        if (allowUpgrades && companyStats != null)
        {
            int val = companyStats.GetValue<int>(ADDITIONAL_MECH_STAT);
            FadditinalMechSlots = val > MAX_ADDITINAL_MECH_SLOTS ? MAX_ADDITINAL_MECH_SLOTS : val;
        }
        if (FadditinalMechSlots < 0) { FadditinalMechSlots = 0; }       
        return FadditinalMechSlots;
      }
      set {
        FadditinalMechSlots = value > MAX_ADDITINAL_MECH_SLOTS ? MAX_ADDITINAL_MECH_SLOTS : value;
        if (FadditinalPlayerMechSlots == -1) { FadditinalPlayerMechSlots = 0; } else
          if (FadditinalPlayerMechSlots > FadditinalMechSlots) { FadditinalPlayerMechSlots = FadditinalMechSlots; };
      }
    }
    public int additinalPlayerMechSlots {
      get {
        if (allowUpgrades && companyStats != null)
        {
            int val = companyStats.GetValue<int>(ADDITIONAL_PLAYER_MECH_STAT);
            FadditinalPlayerMechSlots = val > MAX_ADDITINAL_MECH_SLOTS ? MAX_ADDITINAL_MECH_SLOTS : val;
        }
        if (FadditinalPlayerMechSlots < 0) { FadditinalPlayerMechSlots = 0; }
        return FadditinalPlayerMechSlots;
      }
      set {
        FadditinalPlayerMechSlots = value > MAX_ADDITINAL_MECH_SLOTS ? MAX_ADDITINAL_MECH_SLOTS : value;
        if (FadditinalMechSlots == -1) { FadditinalMechSlots = FadditinalPlayerMechSlots; } else
          if (FadditinalMechSlots < FadditinalPlayerMechSlots) { FadditinalPlayerMechSlots = FadditinalMechSlots; };
      }
    }
    public int defaultMaxTonnage {
            get {
                if (allowUpgrades && companyStats != null)
                {
                    FmaxTonnage = companyStats.GetValue<int>(MAX_TONNAGE_STAT);
                }
                return FmaxTonnage;
            }
            set {
                FmaxTonnage = value;
            }
        }
    [JsonIgnore]
    private int FadditinalMechSlots;
    [JsonIgnore]
    private int FadditinalPlayerMechSlots;
    [JsonIgnore]
    private int FmaxTonnage;
    [JsonIgnore]
    private StatCollection companyStats;
    public Settings() {
      debugLog = false;
      FadditinalMechSlots = -1;
      FadditinalPlayerMechSlots = -1;
      additionalLanceName = "AI LANCE";
      FmaxTonnage = 500;
      allowUpgrades = false;
      showArgoUpgrades = true;
    }
    
    public void setCompanyStats(StatCollection stats) {
        companyStats = stats;
            if (allowUpgrades) {
                if (!companyStats.ContainsStatistic(ADDITIONAL_MECH_STAT)) { companyStats.AddStatistic(ADDITIONAL_MECH_STAT, FadditinalMechSlots); };
                if (!companyStats.ContainsStatistic(ADDITIONAL_PLAYER_MECH_STAT)) { companyStats.AddStatistic(ADDITIONAL_PLAYER_MECH_STAT, FadditinalPlayerMechSlots); };
                if (!companyStats.ContainsStatistic(MAX_TONNAGE_STAT)) { companyStats.AddStatistic(MAX_TONNAGE_STAT, FmaxTonnage); };
            }
     }
  }

  public class Fields {
    public static List<string> callsigns = new List<string>();
  }
}