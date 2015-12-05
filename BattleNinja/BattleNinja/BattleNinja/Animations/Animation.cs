﻿using Microsoft.Xna.Framework.Graphics;
namespace BattleNinja.Animations
{
    public class Animation
    {
        private Texture2D texture;
        private float frameTime;
        private bool isLooping;

        public Animation(Texture2D texture, float frameTime, bool isLooping)
        {
            this.Texture = texture;
            this.FrameTime = frameTime;
            this.IsLooping = isLooping;
        }

        //All grames in the animation arranged horizontally.
        public Texture2D Texture
        {
            get
            {
                return this.texture;
            }
            set
            {
                this.texture = value;
            }
        }

        //Duration of time to show each frame.
        public float FrameTime
        {
            get
            {
                return this.frameTime;
            }
            set
            {
                this.frameTime = value;
            }
        }

        //When the end of the animation i reached, should it continue playing from the beginning.
        public bool IsLooping
        {
            get
            {
                return this.isLooping;
            }
            set
            {
                this.isLooping = value;
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
                return this.Texture.Width;
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
