using Microsoft.Xna.Framework.Graphics;
namespace BattleNinja.Animations
{
    public class Animation
    {
        private Texture2D texture;
        private float frameTime;
        private bool isLooping;

        public Animation(Texture2D texture, float frameTime, bool isLooping)
        {
            this.texture = texture;
            this.frameTime = frameTime;
            this.isLooping = isLooping;
        }

        //All grames in the animation arranged horizontally.
        public Texture2D Texture
        {
            get
            {
                return this.texture;
            }
        }

        //Duration of time to show each frame.
        public float FrameTime
        {
            get
            {
                return this.frameTime;
            }
        }

        //When the end of the animation i reached, should it continue playing from the beginning.
        public bool IsLooping
        {
            get
            {
                return this.isLooping;
            }
        }

        //Gets the number of frames in the animation.
        public int FrameCount 
        { 
            get
            {
                return this.Texture.Width / this.FrameWidth;
            }
        }

        //Gets the width of a frame in the animation.
        public int FrameWidth 
        {
            // Assume square frames.
            get
            {
                return this.Texture.Height;
            }
        }

        //Gets the height of a frame in the animation.
        public int FrameHeight
        {
            get
            {
                return this.Texture.Height;
            }
        }
    }
}
