namespace BattleNinja
{
    using System;
    
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework;

    using BattleNinja.Enums;
    using BattleNinja.Common;

    public struct Tile
    {
        public Texture2D Texture;
        public TileCollision Collision;

        public static readonly Vector2 Size = new Vector2(GlobalConstants.TileWidth, GlobalConstants.TileHeight);

        public Tile(Texture2D texture, TileCollision collision)
            : this()
        {
            this.Texture = texture;
            this.Collision = collision;
        }
    }
}
