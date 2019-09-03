using Newtonsoft.Json;
using System.Collections.Generic;

namespace BiggerDrops {
  public class Settings {
    public static readonly int MAX_ADDITINAL_MECH_SLOTS = 4;
    public static readonly int DEFAULT_MECH_SLOTS = 4;
    public static readonly string EMPLOYER_LANCE_GUID = "ecc8d4f2-74b4-465d-adf6-84445e5dfc230";
    public bool debugLog { get; set; }
    public int defaultMaxTonnage { get; set; }
    public string additionalLanceName { get; set; }
    public int additinalMechSlots {
      get {
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
        if (FadditinalPlayerMechSlots < 0) { FadditinalPlayerMechSlots = 0; }
        return FadditinalPlayerMechSlots;
      }
      set {
        FadditinalPlayerMechSlots = value > MAX_ADDITINAL_MECH_SLOTS ? MAX_ADDITINAL_MECH_SLOTS : value;
        if (FadditinalMechSlots == -1) { FadditinalMechSlots = FadditinalPlayerMechSlots; } else
          if (FadditinalMechSlots < FadditinalPlayerMechSlots) { FadditinalPlayerMechSlots = FadditinalMechSlots; };
      }
    }
    [JsonIgnore]
    private int FadditinalMechSlots;
    [JsonIgnore]
    private int FadditinalPlayerMechSlots;
    public Settings() {
      debugLog = false;
      FadditinalMechSlots = -1;
      FadditinalPlayerMechSlots = -1;
      additionalLanceName = "AI LANCE";
      defaultMaxTonnage = 500;
    }
  }

  public class Fields {
    public static List<string> callsigns = new List<string>();
  }
}