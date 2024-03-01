using System.Collections.Generic;
using ProjectM;

namespace VBuild.Data;

public static class ServantData
{
    public static List<string> ServantTypes = new List<string>
    {
        "Cleric", "Lightweaver", "Nun", "Rifleman", "Priest", "Knight 2H", "Paladin", "Longbowman", "Devoted", "Brawler",
        "Pyro", "TractorBeamer", "Bellringer", "Thief"
    };

    public static List<KeyValuePair<PrefabGUID, PrefabGUID>> UnitToServantList = new List<KeyValuePair<PrefabGUID, PrefabGUID>>
    {
        new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_ChurchOfLight_Cleric, Prefabs.CHAR_ChurchOfLight_Cleric_Servant),
        new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_ChurchOfLight_Lightweaver, Prefabs.CHAR_ChurchOfLight_Lightweaver_Servant),
        new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Militia_Nun, Prefabs.CHAR_Farmlands_Nun_Servant),
        new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_ChurchOfLight_Rifleman, Prefabs.CHAR_ChurchOfLight_Rifleman_Servant),
        new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_ChurchOfLight_Priest, Prefabs.CHAR_ChurchOfLight_Priest_Servant),
        new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_ChurchOfLight_Knight_2H, Prefabs.CHAR_ChurchOfLight_Knight_2H_Servant),
        new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_ChurchOfLight_Paladin, Prefabs.CHAR_ChurchOfLight_Paladin_Servant),
        new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Militia_Longbowman, Prefabs.CHAR_Militia_Longbowman_Servant),
        new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Militia_Devoted, Prefabs.CHAR_Militia_Devoted_Servant),
        new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Militia_Heavy, Prefabs.CHAR_Militia_Heavy_Servant),
        new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Gloomrot_Pyro, Prefabs.CHAR_Gloomrot_Pyro_Servant),
        new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Gloomrot_TractorBeamer, Prefabs.CHAR_Gloomrot_TractorBeamer_Servant),
        new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Militia_BellRinger, Prefabs.CHAR_Militia_BellRinger_Servant),
        new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Bandit_Thief, Prefabs.CHAR_Bandit_Thief_Servant)
    };
}