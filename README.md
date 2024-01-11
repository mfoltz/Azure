WIP
# RPGAddOns

RPGAddOns interfaces with existing RPGMod systems to enhance the player experience in PvE environments. It provides functionality for players to reset their level for additional stats and rewards. The plugin is also designed to work with PvE Rank points accrued from killing VBloods.

For an example configuration, see [RPGAddOns.cfg](https://github.com/mfoltz/BlueBuilds/blob/master/RPGAddOns.cfg).

---

## Commands

**Command**: `.rpg resetlevel` or `.rpg rl`  
**Admin**: false  
**Usage**: Use this command to reset your level to 1 after reaching max level to receive configured extras.  
**Description**: This command allows you to reset your level for rewards once you've reached the maximum level.

---

**Command**: `.rpg getresets` or `.rpg gr`  
**Admin**: false  
**Usage**: Check your current reset count.  
**Description**: Displays the number of times you have reset your level.

---

**Command**: `.rpg getbuffs` or `.rpg gb`  
**Admin**: false  
**Usage**: Check your current permanent buffs.  
**Description**: Displays the buffs you have received from resets.

---

**Command**: `.rpg wiperesets` or `.rpg wr`  
**Admin**: true  
**Usage**: Resets the specified user's reset count and buffs to the initial state.  
**Description**: This command resets a player's resets, including their reset count and buffs. It does not remove the buffs from their player character as that can be done with other commands but will add later.

---

**Command**: `.rpg getresetdata` or `.rpg grd`  
**Admin**: true  
**Usage**: Retrieves the reset count and buffs for a specified player.  
**Description**: Use this command to view the reset count and buffs of a specific player.

---

For more information or to report issues, visit the [GitHub repository](https://github.com/mfoltz/BlueBuilds/tree/NagaAlpha1).
