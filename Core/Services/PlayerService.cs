#region Assembly AdminCommands, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// C:\Users\mitch\Downloads\AdminCommands.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using V.Core.Tools;

namespace V.Core.Services;
public record FoundPlayer(PlayerService.Player Value);
public static class PlayerService
{
    public struct Player
    {
        public string Name { get; set; }

        public ulong SteamID { get; set; }

        public bool IsOnline { get; set; }

        public bool IsAdmin { get; set; }

        public Entity User { get; set; }

        public Entity Character { get; set; }

        public Player(Entity userEntity = default, Entity charEntity = default)
        {
            User = userEntity;
            User user = User.Read<User>();
            Character = user.LocalCharacter._Entity;
            Name = user.CharacterName.ToString();
            IsOnline = user.IsConnected;
            IsAdmin = user.IsAdmin;
            SteamID = user.PlatformId;
        }

        public static implicit operator Player(FoundPlayer v)
        {
            throw new NotImplementedException();
        }
    }

    public static bool TryGetPlayerFromString(string input, out Player player)
    {
        NativeArray<Entity>.Enumerator enumerator = Helper.GetEntitiesByComponentTypes<User>(includeDisabled: true).GetEnumerator();
        while (enumerator.MoveNext())
        {
            Entity current = enumerator.Current;
            User user = current.Read<User>();
            if (user.CharacterName.ToString().ToLower() == input.ToLower())
            {
                player = new Player(current);
                return true;
            }

            if (ulong.TryParse(input, out var result) && user.PlatformId == result)
            {
                player = new Player(current);
                return true;
            }
        }

        player = default;
        return false;
    }

    public static bool TryGetCharacterFromName(string input, out Entity Character)
    {
        if (TryGetPlayerFromString(input, out var player))
        {
            Character = player.Character;
            return true;
        }

        Character = default;
        return false;
    }

    public static bool TryGetUserFromName(string input, out Entity User)
    {
        if (TryGetPlayerFromString(input, out var player))
        {
            User = player.User;
            return true;
        }

        User = default;
        return false;
    }
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
