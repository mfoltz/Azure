**RPGAddOns**

RPGAddOns interfaces with existing RPGMod systems to provide a way for players to reset their level for additional stats and rewards. Prestige will be similar but the points will be earned from VBloods. Will add more once level resetting and prestiging are in a decent spot. Configuration options currently exist for the extra stats given to players when resetting, max number of resets, item rewards on/off, item prefab, item quantity, buff rewards for resetting on/off, buff rewards for prestiging on/off, buff prefab list for resets, and buff prefab list for prestiges. Example config below.

.rpg resetlevel

## Settings file was created by plugin RPGAddOns v1.0.0
## Plugin GUID: RPGAddOns

[Config]

## Extra health on reset
Setting type: Int32
# Default value: 50
ExtraHealth = 50

## Extra physical power awarded on reset
# Setting type: Int32
# Default value: 5
ExtraPhysicalPower = 5

## Extra spell power awarded on reset
# Setting type: Int32
# Default value: 5
ExtraSpellPower = 5

## Extra physical resistance awarded on reset
# Setting type: Int32
# Default value: 0
ExtraPhysicalResistance = 0

## Extra spell resistance awarded on reset
# Setting type: Int32
# Default value: 0
ExtraSpellResistance = 0

## Maximum number of times players can reset their level.
# Setting type: Int32
# Default value: 5
MaxResetCount = 5

## Gives specified item/quantity to players when resetting if enabled.
# Setting type: Boolean
# Default value: false
ItemRewards = true

## Item prefab to give players when resetting. Onyx tears default
# Setting type: Int32
# Default value: -651878258
ItemPrefab = -651878258

## Item quantity to give players when resetting.
# Setting type: Int32
# Default value: 3
ItemQuantity = 3

## Grants permanent buff to players when resetting if enabled.
# Setting type: Boolean
# Default value: false
BuffRewardsReset = true

## Grants permanent buff to players when prestiging if enabled.
# Setting type: Boolean
# Default value: false
BuffRewardsPrestige = false

## Buff prefabs to give players when resetting. Granted in order, want # buffs == # levels [Buff1, Buff2, etc] to skip buff for a level set it to be 'placeholder'
# Setting type: String
# Default value: []
BuffPrefabsReset = [476186894,-1591883586,-1591827622,2099221856,-706770454]

## Buff prefabs to give players when prestiging. Granted in order, want # buffs == # prestige (5) if enabled to skip buff for a level set it to be 'placeholder'
# Setting type: String
# Default value: []
BuffPrefabsPrestige = []



