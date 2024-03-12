using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using VampireCommandFramework;
using VCreate.Systems;
using VCreate.Core.Toolbox;
using VCreate.Data;
using static VCreate.Core.Services.PlayerService;
using static VCreate.Systems.Enablers;

namespace VCreate.Core.Commands
{
    public class CoreCommands
    {
        [Command(name: "equipUnarmedSkills", shortHand: "equip", adminOnly: true, usage: ".equip", description: "Toggles extra skills when switching to unarmed.")]
        public static void ToggleSkillEquip(ChatCommandContext ctx)
        {
            User setter = ctx.Event.User;
            Entity userEntity = ctx.Event.SenderUserEntity;
            User user = VWorld.Server.EntityManager.GetComponentData<User>(userEntity);
            if (DataStructures.PlayerSettings.TryGetValue(user.PlatformId, out Omnitool data))
            {
                bool skills = !data.EquipSkills;

                DataStructures.Save();
                string enabledColor = FontColors.Green("enabled");
                string disabledColor = FontColors.Red("disabled");
                ctx.Reply($" {(skills ? disabledColor : enabledColor)}");
            }
            else
            {
                ctx.Reply("Couldn't find omnitool data.");
            }
        }

        [Command(name: "toggleEmotes", shortHand: "emotes", adminOnly: true, usage: ".emotes", description: "Toggles using the emote wheel to change action on Q when extra skills for unarmed are equipped.")]
        public static void ToggleEmoteActions(ChatCommandContext ctx)
        {
            User setter = ctx.Event.User;
            Entity userEntity = ctx.Event.SenderUserEntity;
            User user = VWorld.Server.EntityManager.GetComponentData<User>(userEntity);
            if (DataStructures.PlayerSettings.TryGetValue(user.PlatformId, out Omnitool data))
            {
                // Toggle the CanEditTiles value
                bool emotes = !data.Emotes;

                DataStructures.Save();
                string enabledColor = FontColors.Green("enabled");
                string disabledColor = FontColors.Red("disabled");
                ctx.Reply($"EmoteToggles: {(emotes ? disabledColor : enabledColor)}");
            }
            else
            {
                ctx.Reply("Couldn't find omnitool data.");
            }
        }

        [Command(name: "MoveDismantlePermissions", shortHand: "perms", adminOnly: true, usage: ".perms [Name]", description: "Toggles tile permissions for a player (allows moving or dismantling objects they don't own if it is something that otherwise could be moved or dismantled by the player).")]
        public static void TogglePlayerPermissions(ChatCommandContext ctx, string name)
        {
            User setter = ctx.Event.User;
            TryGetUserFromName(name, out Entity userEntity);
            User user = VWorld.Server.EntityManager.GetComponentData<User>(userEntity);
            if (DataStructures.PlayerSettings.TryGetValue(user.PlatformId, out Omnitool data))
            {
                // Toggle the CanEditTiles value
                bool currentCanEditTiles = !data.Permissions;

                DataStructures.Save();
                string enabledColor = FontColors.Green("enabled");
                string disabledColor = FontColors.Red("disabled");
                ctx.Reply($"Edit tiles outside of territories: {(currentCanEditTiles ? disabledColor : enabledColor)}");
            }
            else
            {
                ctx.Reply("Couldn't find omnitool data.");
            }
        }

        [Command(name: "toggleSnapping", shortHand: "snap", adminOnly: true, usage: ".snap", description: "Toggles snapping.")]
        public static void SnapToggleCommand(ChatCommandContext ctx)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            if (DataStructures.PlayerSettings.TryGetValue(SteamID, out Omnitool data))
            {
                data.SetMode("SnappingToggle", !data.GetMode("SnappingToggle"));
                DataStructures.Save();
                string enabledColor = FontColors.Green("enabled");
                string disabledColor = FontColors.Red("disabled");
                ctx.Reply($"Snapping: {(data.GetMode("SnappingToggle") ? enabledColor : disabledColor)}");


            }
            else
            {
                ctx.Reply("Couldn't find omnitool data.");
            }
        }

        [Command(name: "setTileRotation", shortHand: "rot", adminOnly: true, usage: ".rot [0/90/180/270]", description: "Sets rotation for spawned tiles.")]
        public static void SetTileRotationCommand(ChatCommandContext ctx, int rotation)
        {
            if (rotation != 0 && rotation != 90 && rotation != 180 && rotation != 270)
            {
                ctx.Reply("Invalid rotation. Use 0, 90, 180, or 270 degrees.");
                return;
            }

            User user = ctx.Event.User;
            if (DataStructures.PlayerSettings.TryGetValue(user.PlatformId, out Omnitool settings))
            {
                settings.SetData("Rotation", rotation);
                DataStructures.Save();
                ctx.Reply($"Tile rotation set to: {rotation} degrees.");
            }
        }

        [Command(name: "setSnapLevel", shortHand: "sl", adminOnly: true, usage: ".sl [1/2/3]", description: "Sets snap level for spawned tiles.")]
        public static void SetSnappingLevelCommand(ChatCommandContext ctx, int level)
        {
            if (level != 1 && level != 2 && level != 3)
            {
                ctx.Reply("Options are 1 for 2.5u, 2 for 5u, and 3 for 7.5u.");
                return;
            }

            User user = ctx.Event.User;
            if (DataStructures.PlayerSettings.TryGetValue(user.PlatformId, out Omnitool settings))
            {
                settings.SetData("GridSize", level);
                DataStructures.Save();
                ctx.Reply($"Tile snapping set to: {level}u");
            }
        }

        [Command(name: "setCharacterUnit", shortHand: "char", adminOnly: true, usage: ".char [PrefabGUID]", description: "Sets cloned unit prefab.")]
        public static void SetUnit(ChatCommandContext ctx, int choice)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            if (DataStructures.PlayerSettings.TryGetValue(SteamID, out Omnitool data))
            {
                // Assuming there's a similar check for map icons as there is for tile models
                if (Prefabs.FindPrefab.CheckForMatch(choice))
                {
                    PrefabGUID prefabGUID = new(choice);
                    if (prefabGUID.LookupName().ToLower().Contains("char"))
                    {
                        ctx.Reply($"Character unit set.");
                        data.SetData("Unit", choice);
                        DataStructures.Save();
                    }
                    else
                    {
                        ctx.Reply("Invalid character unit.");
                    }
                }
                else
                {
                    ctx.Reply("Couldn't find prefab.");
                }
            }
            else
            {
                ctx.Reply("Couldn't find omnitool data.");
            }
        }
        [Command(name: "setBuff", shortHand: "sb", adminOnly: true, usage: ".sb [PrefabGUID]", description: "Sets buff for buff mode.")]
        public static void SetBuff(ChatCommandContext ctx, int choice)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            if (DataStructures.PlayerSettings.TryGetValue(SteamID, out Omnitool data))
            {
                // Assuming there's a similar check for map icons as there is for tile models
                if (Prefabs.FindPrefab.CheckForMatch(choice))
                {
                    PrefabGUID prefabGUID = new PrefabGUID(choice);
                    if (prefabGUID.LookupName().ToLower().Contains("buff"))
                    {
                        ctx.Reply($"Buff set.");
                        data.SetData("Buff", choice);
                        DataStructures.Save();
                    }
                    else
                    {
                        ctx.Reply("Invalid buff.");
                    }
                }
                else
                {
                    ctx.Reply("Couldn't find prefab.");
                }
            }
            else
            {
                ctx.Reply("Couldn't find omnitool data.");
            }
        }

        [Command(name: "setMapIcon", shortHand: "map", adminOnly: true, usage: ".map [PrefabGUID]", description: "Sets map icon to prefab.")]
        public static void SetMapIcon(ChatCommandContext ctx, int choice)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            if (DataStructures.PlayerSettings.TryGetValue(SteamID, out Omnitool data))
            {
                // Assuming there's a similar check for map icons as there is for tile models
                if (Prefabs.FindPrefab.CheckForMatch(choice))
                {
                    PrefabGUID prefabGUID = new PrefabGUID(choice);
                    if (prefabGUID.LookupName().ToLower().Contains("map"))
                    {
                        ctx.Reply($"Map icon set.");
                        data.SetData("MapIcon", choice);
                        DataStructures.Save();
                    }
                    else
                    {
                        ctx.Reply("Invalid map icon.");
                    }
                }
                else
                {
                    ctx.Reply("Couldn't find prefab.");
                }
            }
            else
            {
                ctx.Reply("Couldn't find omnitool data.");
            }
        }

        [Command(name: "setTileModel", shortHand: "tm", adminOnly: true, usage: ".tm [PrefabGUID]", description: "Sets tile model to prefab.")]
        public static void SetTileByPrefab(ChatCommandContext ctx, int choice)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            if (DataStructures.PlayerSettings.TryGetValue(SteamID, out Omnitool data))
            {
                if (Prefabs.FindPrefab.CheckForMatch(choice))
                {
                    PrefabGUID prefabGUID = new PrefabGUID(choice);
                    if (prefabGUID.LookupName().ToLower().Contains("tile"))
                    {
                        ctx.Reply($"Tile model set.");
                        data.SetData("Tile", choice);
                        DataStructures.Save();
                    }
                    else
                    {
                        ctx.Reply("Invalid choice for tile model.");
                    }
                }
                else
                {
                    ctx.Reply("Invalid tile choice.");
                }
            }
            else
            {
                ctx.Reply("Couldn't find omnitool data.");
            }
        }
        

        [Command(name: "undolast", shortHand: "undo", adminOnly: true, usage: ".undo", description: "Destroys the last entity placed, up to 10.")]
        public static void UndoCommand(ChatCommandContext ctx)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            User user = ctx.Event.User;
            if (DataStructures.PlayerSettings.TryGetValue(user.PlatformId, out Omnitool data))
            {
                string lastTileRef = data.PopEntity();
                if (!string.IsNullOrEmpty(lastTileRef))
                {
                    string[] parts = lastTileRef.Split(", ");
                    if (parts.Length == 2 && int.TryParse(parts[0], out int index) && int.TryParse(parts[1], out int version))
                    {
                        Entity tileEntity = new Entity { Index = index, Version = version };
                        if (entityManager.Exists(tileEntity) && tileEntity.Version == version)
                        {
                            SystemPatchUtil.Destroy(tileEntity);
                            ctx.Reply($"Successfully destroyed last tile placed.");
                            DataStructures.Save();
                        }
                        else
                        {
                            ctx.Reply("The tile could not be found or has already been modified.");
                        }
                    }
                    else
                    {
                        ctx.Reply("Failed to parse the reference to the last tile placed.");
                    }
                }
                else
                {
                    ctx.Reply("You haven't placed any tiles yet or all undos have been used.");
                }
            }
            else
            {
                ctx.Reply("Couldn't find omnitool data.");
            }
        }

        [Command(name: "destroyResourceNodes", shortHand: "destroynodes", adminOnly: true, usage: ".destroynodes", description: "Destroys resources in player territories. Only use this after disabling worldbuild.")]
        public static void DestroyResourcesCommand(ChatCommandContext ctx)
        {
            ResourceFunctions.SearchAndDestroy();
            ctx.Reply("Resource nodes in player territories destroyed. Probably.");
        }

        [Command("destroyTileModels", shortHand: "dtm", adminOnly: true, description: "Destroys tiles in entered radius matching entered full tile model name (ex: TM_ArtisansWhatsit_T01).", usage: "Usage: .dtm [TM_Example_01] [radius]")]
        public static void DestroyTiles(ChatCommandContext ctx, string name, float radius = 5f)
        {
            // Check if a name is not provided or is empty
            if (string.IsNullOrEmpty(name))
            {
                ctx.Error("You need to specify a tile name!");
                return;
            }

            var tiles = Enablers.TileFunctions.ClosestTiles(ctx, radius, name);

            foreach (var tile in tiles)
            {
                SystemPatchUtil.Destroy(tile);
                ctx.Reply(name + " destroyed!");
            }

            if (tiles.Count < 1)
            {
                ctx.Error("Failed to destroy any tiles, are there any in range?");
            }
            else
            {
                ctx.Reply("Tiles have been destroyed!");
            }
        }

        
    }
}