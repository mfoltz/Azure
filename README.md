**RPGAddOns**

RPGAddOns interfaces with existing RPGMod systems to provide a way for players to reset their level for additional stats and rewards. Prestige will be similar but the points will be earned from VBloods. Will add more once level resetting and prestiging are in a decent spot. Configuration options currently exist for the extra stats given to players when resetting, max number of resets, item rewards on/off, item prefab, item quantity, buff rewards for resetting on/off, buff rewards for prestiging on/off, buff prefab list for resets, and buff prefab list for prestiges.

Example config: https://github.com/mfoltz/BlueBuilds/blob/master/RPGAddOns.cfg

https://github.com/oscarpedrero/BloodyPoints is a great mod to start poking around in if you're new to this.

**Commands**
###
Command: .rpg resetlevel or .rpg rl

Admin: false

Usage: Use this command to reset your level to 1 after reaching max level to receive configured extras.

Description: This command allows you to reset your level for rewards once you've reached the maximum level.
###
Command: .rpg getresets or .rpg gr

Admin: false

Usage: Check your current reset count.

Description: Displays the number of times you have reset your level.
###
Command: .rpg getbuffs or .rpg gb

Admin: false

Usage: Check your current permanent buffs.

Description: Displays the buffs you have received from resets.
###
Command: .rpg wiperesets or .rpg wr <PlayerName>

Admin: true

Usage: Resets the specified user's reset count and buffs to the initial state.

Description: This command resets a player's resets, including their reset count and buffs. It does not remove the buffs from their player character as that can be done with other commands but will add later
###
Command: .rpg getresetdata or .rpg grd <PlayerName>

Admin: true

Usage: Retrieves the reset count and buffs for a specified player.

Description: Use this command to view the reset count and buffs of a specific player.
