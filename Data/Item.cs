using ProjectM;

namespace V.Data;

public class Item
{
    public PrefabGUID PrefabGUID { get; }

    public string FormalPrefabName { get; }

    public string OverrideName { get; private set; }

    public Item(PrefabGUID prefabGUID, string formalPrefabName, string overrideName = "")
    {
        PrefabGUID = prefabGUID;
        FormalPrefabName = formalPrefabName;
        OverrideName = overrideName;
    }

    public string GetName()
    {
        return string.IsNullOrEmpty(OverrideName) ? FormalPrefabName : OverrideName;
    }
}