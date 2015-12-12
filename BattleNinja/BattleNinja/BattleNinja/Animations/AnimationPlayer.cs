namespace BattleNinja.Animations
{
    using System;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public struct AnimationPlayer
    {
        private Animation animation;
        //The amount of time in seconds that the current frame has been shown for.
        private float time;
        private int frameIndex;

        //Gets the animation which is currently playing.
        public Animation Animation 
        { 
            get
            {
                return this.animation;
            }
        }

        //Gets the index of the current frame in the animation.
        public int FrameIndex
        {
            get
            {
                return this.frameIndex;
            }
        }

        //Gets a texture origin at the bottom center of each frame.
        public Vector2 Origin 
        { 
            get
            {
                return new Vector2(this.Animation.FrameWidth / 2.0f, this.Animation.FrameHeight);
            }
        }

        public void PlayAnimation(Animation animation)
        {
            //If this animation is already running, do not restart it.
            if (this.Animation == animation)
            {
                return;
            }

            //Start the new animation.
            this.animation = animation;
            this.frameIndex = 0;
            this.time = 0.0f;
        }

        //Advances the time position and draws the current frame of the animation. 
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects)
        {
            if (this.Animation == null)
            {
                throw new NotSupportedException("No animation is currently playing.");
            }

            // Process passing time.
            this.time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (this.time > this.Animation.FrameTime)
            {
                this.time -= this.Animation.FrameTime;

                //Advanced the frame index; looping or clamping as appropiate.
                if (this.Animation.IsLooping)
                {
                    this.frameIndex = (this.FrameIndex + 1) % this.Animation.FrameCount;
                }
                else
                {
                    this.frameIndex = Math.Min(this.FrameIndex + 1, this.Animation.FrameCount - 1);
                }
            }

            //Calculate the source rectangle of the current frame.
            Rectangle source = new Rectangle(
                this.FrameIndex * this.Animation.Texture.Height, 
                0, 
                this.Animation.Texture.Height, 
                this.Animation.Texture.Height);

            //Draw the current frame.
            spriteBatch.Draw(this.Animation.Texture, position, source, Color.White, 0.0f, this.Origin, 1.0f, spriteEffects, 0.0f);
        }
    }
}
