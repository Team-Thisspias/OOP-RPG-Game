namespace BattleNinja
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    
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
        //Entities in the level.
        private Player player;

        private List<Gem> gems;
        private List<Enemy> enemies;

        //Key locations in the level.
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        //Level game state.
        private Random random = new Random(354668); // Arbitrary, but constant seed

        private float cameraPosition;
        private int score;
        private bool reachedExit;


        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");

            timeRemaining = TimeSpan.FromMinutes(2.0);

            this.LoadTiles(fileStream);

            // Load background layer textures. For now, all levels must
            // use the same backgrounds and only use the left-most part of them.
            layers = new Layer[3];
            layers[0] = new Layer(Content, "Backgrounds/Layer0", 0.2f);   // speends at which the layers scroll. foreground scrolls the fastest
            layers[1] = new Layer(Content, "Backgrounds/Layer1", 0.5f);   // this allows me to have 3 layers of background instead of the standard 1
            layers[2] = new Layer(Content, "Backgrounds/Layer2", 0.8f);

            this.Gems = new List<Gem>();
            this.Enemies = new List<Enemy>();

            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/Space laser sounds 2");

            // once the exit point has been reached, it will play this sound
        }

        public int Score 
        { 
            get
            {
                return this.score;
            }
        }

        public bool ReachedExit 
        { 
            get
            {
                return this.reachedExit;
            }
        }

        public Player Player 
        { 
            get
            {
                return this.player;
            }
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

        public List<Gem> Gems 
        { 
            get
            {
                return this.gems;
            }
            set
            {
                this.gems = value;
            }
        }

        public List<Enemy> Enemies
        {
            get
            {
                return this.enemies;
            }
            set
            {
                this.enemies = value;
            }
        }

        private void LoadTiles(Stream fileStream)
        {
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                    {
                        throw new Exception(string.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    }

                    line = reader.ReadLine();
                }
            }

            //Allocate the tile grid.
            this.tiles = new Tile[width, lines.Count];

            //Loop over every tile position
            for (int y = 0; y < this.Height; ++y)
            {
                for (int x = 0; x < this.Width; ++x)
                {
                    //to load each tile.
                    char tileType = lines[y][x];
                    tiles[x, y] = this.LoadTiles(tileType, x, y);
                }
            }

            //Verify that level has a beginning and an end.
            if (this.Player == null)
            {
                throw new NotSupportedException("A level must have a starting point.");
            }
            if (this.exit == InvalidPosition)
            {
                throw new NotSupportedException("A level must have an exit.");
            }
        }

        private Tile LoadTiles(char tileType, int x, int y)
        {
            throw new NotImplementedException();
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
