#region Assembly AdminCommands, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// C:\Users\mitch\Downloads\AdminCommands.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System.Collections.Generic;
using ProjectM;

namespace WorldBuild.Data;

public static class LegendaryData
{
    public static readonly Dictionary<string, PrefabGUID> weaponToPrefabDictionary = new Dictionary<string, PrefabGUID>
    {
        {
            "slashers",
            Prefabs.Item_Weapon_Slashers_Legendary_T08
        },
        {
            "spear",
            Prefabs.Item_Weapon_Spear_Legendary_T08
        },
        {
            "axe",
            Prefabs.Item_Weapon_Axe_Legendary_T08
        },
        {
            "greatsword",
            Prefabs.Item_Weapon_GreatSword_Legendary_T08
        },
        {
            "crossbow",
            Prefabs.Item_Weapon_Crossbow_Legendary_T08
        },
        {
            "pistols",
            Prefabs.Item_Weapon_Pistols_Legendary_T08
        },
        {
            "reaper",
            Prefabs.Item_Weapon_Reaper_Legendary_T08
        },
        {
            "sword",
            Prefabs.Item_Weapon_Sword_Legendary_T08
        },
        {
            "mace",
            Prefabs.Item_Weapon_Mace_Legendary_T08
        }
    };

    public static readonly Dictionary<string, PrefabGUID> infusionToPrefabDictionary = new Dictionary<string, PrefabGUID>
    {
        {
            "blood",
            Prefabs.SpellMod_Weapon_BloodInfused
        },
        {
            "chaos",
            Prefabs.SpellMod_Weapon_ChaosInfused
        },
        {
            "frost",
            Prefabs.SpellMod_Weapon_FrostInfused
        },
        {
            "illusion",
            Prefabs.SpellMod_Weapon_IllusionInfused
        },
        {
            "static",
            Prefabs.SpellMod_Weapon_StormInfused
        },
        {
            "unholy",
            Prefabs.SpellMod_Weapon_UndeadInfused
        }
    };

    public static List<PrefabGUID> statMods = new List<PrefabGUID>
    {
        Prefabs.StatMod_AttackSpeed,
        Prefabs.StatMod_CriticalStrikePhysical,
        Prefabs.StatMod_CriticalStrikePhysicalPower,
        Prefabs.StatMod_SpellPower,
        Prefabs.StatMod_PhysicalResistance,
        Prefabs.StatMod_MovementSpeed,
        Prefabs.StatMod_CriticalStrikeSpells,
        Prefabs.StatMod_CriticalStrikeSpellPower,
        Prefabs.StatMod_SpellLeech,
        Prefabs.StatMod_ResourceYield,
        Prefabs.StatMod_MaxHealth,
        Prefabs.StatMod_HealthRegen,
        Prefabs.StatMod_PhysicalPower
    };

    public static List<string> statModDescriptions = new List<string>
    {
        "Attack Speed", "Physical Critical Strike Chance", "Physical Critical Strike Damage", "Spell Power", "Physical Damage Reduction", "Movement Speed", "Spell Critical Strike Chance", "Spell Critical Strike Damage", "Spell Life Leech", "Resource Yield",
        "Max Health", "Health Regen", "Physical Power"
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
