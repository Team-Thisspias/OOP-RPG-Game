namespace BattleNinja.Items
{
    using System;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Audio;

    using BattleNinja.Common;

    public class Gem
    {
        private Texture2D texture;
        private Vector2 origin;
        private SoundEffect collectedSound;
        private Level level;

        public readonly Color Color = Color.Blue; //color of the sprite

        //The gem is animated from a base position along the Y axis;
        private Vector2 basePosition;
        private float bounce; // the icon will bounce

        public Gem(Level level, Vector2 position)
        {
            this.Level = level;
            this.basePosition = position;

            LoadContent();
        }

        public Level Level 
        { 
            get
            {
                return level;
            }
            set
            {
                this.level = value;
            }
        }

        //Gets the current position of this gem in world space.
        public Vector2 Position 
        { 
            get
            {
                return this.basePosition + new Vector2(0.0f, bounce);
            }
        }

        //Gets a circle which bounds this gem in world space.
        public Circle BoundingCircle
        {
            get
            {
                return new Circle(this.Position, GlobalConstants.TileWidth / 3.0f);
            }
        }


        private void LoadContent()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Bounces up and down in the air to entice players to collect them.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Bounce along a sine curve over time.
            // Include the X coordinate so that neighboring gems bounce in a nice wave pattern.            
            double t = gameTime.TotalGameTime.TotalSeconds * GlobalConstants.GemBounceRate + Position.X * GlobalConstants.GemBounceSync;
            bounce = (float)Math.Sin(t) * GlobalConstants.GemBounceHeight * texture.Height;
        }
    }
}
