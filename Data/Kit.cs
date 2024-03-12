using ProjectM;

namespace VBuild.Data;

public static class Kit
{
    public static List<PrefabGUID> personalGear = new List<PrefabGUID>
    {
        Prefabs.Item_Boots_T08_Shadowmoon,
        Prefabs.Item_Chest_T08_Shadowmoon,
        Prefabs.Item_Gloves_T08_Shadowmoon,
        Prefabs.Item_Legs_T08_Shadowmoon,
        Prefabs.Item_MagicSource_General_T08_Delusion,
        Prefabs.Item_MagicSource_General_T08_Beast,
        Prefabs.Item_Cloak_Main_T03_Phantom
    };

    public static List<PrefabGUID> cosmetics = new List<PrefabGUID>
    {
        Prefabs.Item_Cloak_T03_Royal,
        Prefabs.Item_Headgear_Strawhat
    };

    public static List<PrefabGUID> personalAccessories = new List<PrefabGUID>
    {
        Prefabs.Item_Bag_Grand_Coins,
        Prefabs.Item_Bag_Grand_Coins,
        Prefabs.Item_Bag_Grand_Coins,
        Prefabs.Item_Bag_Grand_Herbs
    };

    public static List<PrefabGUID> basicWeapons = new List<PrefabGUID>
    {
        Prefabs.Item_Weapon_Slashers_T08_Sanguine,
        Prefabs.Item_Weapon_Spear_T08_Sanguine,
        Prefabs.Item_Weapon_Axe_T08_Sanguine,
        Prefabs.Item_Weapon_GreatSword_T08_Sanguine,
        Prefabs.Item_Weapon_Crossbow_T08_Sanguine,
        Prefabs.Item_Weapon_Pistols_T08_Sanguine,
        Prefabs.Item_Weapon_Reaper_T08_Sanguine
    };

    public static List<PrefabGUID> servantGear = new List<PrefabGUID>
    {
        Prefabs.Item_Weapon_Spear_Legendary_T08,
        Prefabs.Item_Boots_T08_Shadowmoon,
        Prefabs.Item_Chest_T08_Shadowmoon,
        Prefabs.Item_Gloves_T08_Shadowmoon,
        Prefabs.Item_Legs_T08_Shadowmoon,
        Prefabs.Item_MagicSource_General_T08_Delusion
    };

    public static List<PrefabGUID> potions = new List<PrefabGUID>
    {
        Prefabs.Item_Consumable_GlassBottle_PhysicalBrew_T02,
        Prefabs.Item_Consumable_GlassBottle_SpellBrew_T02,
        Prefabs.Item_Consumable_GlassBottle_WranglersTea_T01,
        Prefabs.Item_Consumable_GlassBottle_HolyResistance_T03,
        Prefabs.Item_Consumable_Canteen_MinorFireResistanceBrew_T01,
        Prefabs.Item_Consumable_Canteen_MinorSunResistanceBrew_T01
    };

    public static List<PrefabGUID> heals = new List<PrefabGUID>
    {
        Prefabs.Item_Consumable_GlassBottle_BloodRosePotion_T02,
        Prefabs.Item_Consumable_Canteen_BloodRoseBrew_T01
    };

    public static Dictionary<PrefabGUID, int> heartUpgrade = new Dictionary<PrefabGUID, int>
    {
        {
            Prefabs.Item_Ingredient_Leather,
            12
        },
        {
            Prefabs.Item_Ingredient_Mineral_CopperIngot,
            12
        },
        {
            Prefabs.Item_Ingredient_Glass,
            24
        },
        {
            Prefabs.Item_Ingredient_ReinforcedPlank,
            8
        },
        {
            Prefabs.Item_BloodEssence_T02_Greater,
            1
        },
        {
            Prefabs.Item_Ingredient_RadiumAlloy,
            12
        },
        {
            Prefabs.Item_BloodEssence_T03_Primal,
            2
        },
        {
            Prefabs.Item_Ingredient_Mineral_DarkSilverBar,
            12
        },
        {
            Prefabs.Item_Ingredient_PowerCore,
            4
        }
    };

    public static Dictionary<PrefabGUID, int> baseBuilding = new Dictionary<PrefabGUID, int>
    {
        {
            Prefabs.Item_Ingredient_Plank,
            800
        },
        {
            Prefabs.Item_Ingredient_StoneBrick,
            800
        },
        {
            Prefabs.Item_Ingredient_Stone,
            2000
        },
        {
            Prefabs.Item_Ingredient_Wood_Standard,
            2000
        },
        {
            Prefabs.Item_Ingredient_Mineral_CopperIngot,
            200
        },
        {
            Prefabs.Item_Ingredient_Mineral_CopperOre,
            200
        },
        {
            Prefabs.Item_BloodEssence_T01,
            1000
        },
        {
            Prefabs.Item_BloodEssence_T02_Greater,
            40
        },
        {
            Prefabs.Item_BloodEssence_T03_Primal,
            10
        },
        {
            Prefabs.Item_Ingredient_Mineral_IronBar,
            200
        },
        {
            Prefabs.Item_Ingredient_ReinforcedPlank,
            200
        },
        {
            Prefabs.Item_Ingredient_Mineral_GoldBar,
            40
        },
        {
            Prefabs.Item_Ingredient_PowerCore,
            10
        },
        {
            Prefabs.Item_Ingredient_Research_Schematic,
            50
        },
        {
            Prefabs.Item_Ingredient_Glass,
            200
        },
        {
            Prefabs.Item_Ingredient_Cloth,
            200
        },
        {
            Prefabs.Item_Ingredient_Mineral_Sulfur,
            200
        },
        {
            Prefabs.Item_Ingredient_Gemdust,
            250
        },
        {
            Prefabs.Item_Ingredient_Gravedust,
            50
        },
        {
            Prefabs.Item_Ingredient_RuggedHide,
            1000
        },
        {
            Prefabs.Item_Ingredient_Plant_PlantFiber,
            7000
        },
        {
            Prefabs.Item_Ingredient_Leather,
            100
        },
        {
            Prefabs.Item_Ingredient_CottonYarn,
            100
        },
        {
            Prefabs.Item_Ingredient_Spectraldust,
            100
        },
        {
            Prefabs.Item_Ingredient_RadiumAlloy,
            200
        },
        {
            Prefabs.Item_Ingredient_Scourgestone,
            40
        },
        {
            Prefabs.Item_Ingredient_Whetstone,
            50
        },
        {
            Prefabs.Item_Ingredient_Thread_Wool,
            50
        },
        {
            Prefabs.Item_Ingredient_TechScrap,
            200
        },
        {
            Prefabs.Item_Ingredient_Gem_Amethyst_T01,
            8
        },
        {
            Prefabs.Item_Ingredient_Bone,
            1000
        }
    };
}

#if false // Decompilation log
'342' items in cache
------------------
Resolve: 'System.Runtime, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\mitch\.nuget\packages\microsoft.netcore.app.ref\6.0.27\ref\net6.0\System.Runtime.dll'
------------------
Resolve: 'System.Collections, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\mitch\.nuget\packages\microsoft.netcore.app.ref\6.0.27\ref\net6.0\System.Collections.dll'
------------------
Resolve: 'Unity.Entities, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Unity.Entities, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\Unity.Entities.dll'
------------------
Resolve: 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\UnityEngine.CoreModule.dll'
------------------
Resolve: 'ProjectM, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'ProjectM, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\ProjectM.dll'
------------------
Resolve: '0Harmony, Version=2.10.1.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: '0Harmony, Version=2.10.1.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\harmonyx\2.10.1\lib\netstandard2.0\0Harmony.dll'
------------------
Resolve: 'BepInEx.Unity.IL2CPP, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'BepInEx.Unity.IL2CPP, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\bepinex.unity.il2cpp\6.0.0-be.668\lib\net6.0\BepInEx.Unity.IL2CPP.dll'
------------------
Resolve: 'Il2CppInterop.Runtime, Version=1.4.5.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Il2CppInterop.Runtime, Version=1.4.5.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\Il2CppInterop.Runtime.dll'
------------------
Resolve: 'Stunlock.Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Stunlock.Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\Stunlock.Core.dll'
------------------
Resolve: 'ProjectM.Shared, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'ProjectM.Shared, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\ProjectM.Shared.dll'
------------------
Resolve: 'ProjectM.Gameplay.Systems, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'ProjectM.Gameplay.Systems, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\ProjectM.Gameplay.Systems.dll'
------------------
Resolve: 'ProjectM.Misc.Systems, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'ProjectM.Misc.Systems, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\ProjectM.Misc.Systems.dll'
------------------
Resolve: 'Unity.Collections, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Unity.Collections, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\Unity.Collections.dll'
------------------
Resolve: 'ProjectM.Roofs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'ProjectM.Roofs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\ProjectM.Roofs.dll'
------------------
Resolve: 'Unity.Mathematics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Unity.Mathematics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\Unity.Mathematics.dll'
------------------
Resolve: 'Il2Cppmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Il2Cppmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\Il2Cppmscorlib.dll'
------------------
Resolve: 'Unity.Transforms, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Unity.Transforms, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\Unity.Transforms.dll'
------------------
Resolve: 'BepInEx.Core, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'BepInEx.Core, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\bepinex.core\6.0.0-be.668\lib\netstandard2.0\BepInEx.Core.dll'
------------------
Resolve: 'Bloodstone, Version=0.1.6.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Bloodstone, Version=0.1.6.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.bloodstone\0.1.6\lib\net6.0\Bloodstone.dll'
------------------
Resolve: 'com.stunlock.network, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'com.stunlock.network, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\com.stunlock.network.dll'
------------------
Resolve: 'ProjectM.Terrain, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'ProjectM.Terrain, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\ProjectM.Terrain.dll'
------------------
Resolve: 'ProjectM.Gameplay.Scripting, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'ProjectM.Gameplay.Scripting, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\ProjectM.Gameplay.Scripting.dll'
------------------
Resolve: 'System.Text.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Text.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Users\mitch\.nuget\packages\microsoft.netcore.app.ref\6.0.27\ref\net6.0\System.Text.Json.dll'
------------------
Resolve: 'System.ObjectModel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ObjectModel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\mitch\.nuget\packages\microsoft.netcore.app.ref\6.0.27\ref\net6.0\System.ObjectModel.dll'
------------------
Resolve: 'VampireCommandFramework, Version=0.8.2.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'VampireCommandFramework, Version=0.8.2.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.vampirecommandframework\0.8.2\lib\net6.0\VampireCommandFramework.dll'
------------------
Resolve: 'System.Linq, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Linq, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\mitch\.nuget\packages\microsoft.netcore.app.ref\6.0.27\ref\net6.0\System.Linq.dll'
------------------
Resolve: 'System.Runtime.InteropServices, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.InteropServices, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\mitch\.nuget\packages\microsoft.netcore.app.ref\6.0.27\ref\net6.0\System.Runtime.InteropServices.dll'
------------------
Resolve: 'ProjectM.CodeGeneration, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'ProjectM.CodeGeneration, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\ProjectM.CodeGeneration.dll'
------------------
Resolve: 'System.Text.RegularExpressions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Text.RegularExpressions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\mitch\.nuget\packages\microsoft.netcore.app.ref\6.0.27\ref\net6.0\System.Text.RegularExpressions.dll'
------------------
Resolve: 'System.Runtime.CompilerServices.Unsafe, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'System.Runtime.CompilerServices.Unsafe, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\mitch\.nuget\packages\microsoft.netcore.app.ref\6.0.27\ref\net6.0\System.Runtime.CompilerServices.Unsafe.dll'
#endif