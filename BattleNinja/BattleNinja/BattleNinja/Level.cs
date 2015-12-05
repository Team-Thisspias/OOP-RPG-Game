namespace BattleNinja
{
    using System;
    using System.IO;
    using BattleNinja.Characters;
    using BattleNinja.Common;
    using BattleNinja.Items;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    public class Level
    {
        // Physical structure of the level.
        private Tile[,] tiles;
        private Layer[] layers;
        private TimeSpan timeRemaining;
        private ContentManager content;
        private SoundEffect exitReachedSound;

        private float cameraPosition;

        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");

            timeRemaining = TimeSpan.FromMinutes(2.0);

            //TODO LoadTiles()

            // Load background layer textures. For now, all levels must
            // use the same backgrounds and only use the left-most part of them.
            layers = new Layer[3];
            layers[0] = new Layer(Content, "Backgrounds/Layer0", 0.2f);   // speends at which the layers scroll. foreground scrolls the fastest
            layers[1] = new Layer(Content, "Backgrounds/Layer1", 0.5f);   // this allows me to have 3 layers of background instead of the standard 1
            layers[2] = new Layer(Content, "Backgrounds/Layer2", 0.8f);

            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/Space laser sounds 2");

            // once the exit point has been reached, it will play this sound
        }

        public TimeSpan TimeRemaining
        {
            get { return timeRemaining; }
        }
        // Level content.        
        public ContentManager Content
        {
            get { return content; }
        }

        /// Width of level measured in tiles.
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        // Height of the level measured in tiles.
        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        /// Draw everything in the level from background to foreground.
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            for (int i = 0; i <= GlobalConstants.EntityLayer; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition, 0.0f, 0.0f);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default,
                              RasterizerState.CullCounterClockwise, null, cameraTransform);

            DrawTiles(spriteBatch);

            //TODO gem,player,enemies

            spriteBatch.End();

            spriteBatch.Begin();
            for (int i = GlobalConstants.EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();

        }

        // this moves the world backwards, and keeps the camera inthe same position


        private void ScrollCamera(Viewport viewport)
        {
            // Calculate the edges of the screen.
            float marginWidth = viewport.Width * GlobalConstants.ViewMargin;
            float marginLeft = cameraPosition + marginWidth;
            float marginRight = cameraPosition + viewport.Width - marginWidth;

            // Calculate how far to scroll when the player is near the edges of the screen.
            //TODO
            float cameraMovement = 0.0f;
            
            // Update the camera position, but prevent scrolling off the ends of the level.
            float maxCameraPosition = GlobalConstants.TileWidth * Width - viewport.Width;
            cameraPosition = MathHelper.Clamp(cameraPosition + cameraMovement, 0.0f, maxCameraPosition);
        }

        /// Draws each tile in the level.
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // Calculate the visible range of tiles.
            int left = (int)Math.Floor(cameraPosition / GlobalConstants.TileWidth);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / GlobalConstants.TileWidth;
            right = Math.Min(right, Width - 1);
            // For each tile position

            for (int y = 0; y < Height; ++y)
            {
                for (int x = left; x <= right; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }



    }
}
