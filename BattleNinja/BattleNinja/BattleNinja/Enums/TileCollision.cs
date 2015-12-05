using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNinja.Enums
{
    //Controls the collision detection and response behavior of a tile.
    public enum TileCollision
    {
        //A passable tile is one which does not hinder player motion at all.
        Passable = 0,

        //An impassable tile is one which does not allow the player to move through
        Impassable = 1,

        //A platform tile is one which behaves like a passable tile except when the player is above it.
        Platform = 2,
    }
}
