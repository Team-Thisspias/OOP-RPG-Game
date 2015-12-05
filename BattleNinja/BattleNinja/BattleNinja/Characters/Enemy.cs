namespace BattleNinja.Characters
{
    using Microsoft.Xna.Framework;
using System;

    public class Enemy
    {
        private Level level;
        private Vector2 position;
        private Rectangle localBounds;

        //private AnimationPlayer player;

        public Level Level
        {
            get { return level; }
        }

        public Vector2 Position
        {
            get { return position; }
        }

        //public Rectangle BoundingRectangle
        //{
        //    get
        //    {
        //        int left = (int)Math.Round(this.Position.X - sprite);
        //    }
        //}
    }
}
