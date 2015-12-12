namespace BattleNinja
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    
    using BattleNinja.Characters;
    using BattleNinja.Common;
    using BattleNinja.Items;
    using BattleNinja.Enums;
    
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public class Level : IDisposable
    {
        // Physical structure of the level.
        private Tile[,] tiles;
        private Layer[] layers;
        private TimeSpan timeRemaining;
        private ContentManager content;
        private SoundEffect exitReachedSound;
        //Entities in the level.
        private Player player;

        private List<Gem> gems = new List<Gem>();
        private List<Enemy> enemies = new List<Enemy>();

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

        // Draw everything in the level from background to foreground.
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            for (int i = 0; i <= GlobalConstants.EntityLayer; ++i)
            {
                layers[i].Draw(spriteBatch, cameraPosition);
            }

            spriteBatch.End();

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition, 0.0f, 0.0f);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default,
                              RasterizerState.CullCounterClockwise, null, cameraTransform);

            DrawTiles(spriteBatch);

            foreach (Gem gem in gems)
                gem.Draw(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            foreach (Enemy enemy in enemies)
            {
                if (enemy.IsAlive || enemy.deathTime > 0)
                {
                    enemy.Draw(gameTime, spriteBatch);
                }
            }

            spriteBatch.End();

            spriteBatch.Begin();
            
            for (int i = GlobalConstants.EntityLayer + 1; i < layers.Length; ++i)
            {
                layers[i].Draw(spriteBatch, cameraPosition);
            }
            
            spriteBatch.End();

        }

        //Gets the collision made of the tile at a particular location.
        //This method handles tiles outside of the levels boundries by making it 
        //impossible to escape past the left or right edges, but allowing things
        //to jump beyond the top of the level and fall off the bottom.
        public TileCollision GetCollision(int x, int y)
        {
            //Prevent escaping past the level ends.
            if (x < 0 || x >= this.Width)
            {
                return TileCollision.Impassable;
            }

            //Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= this.Height)
            {
                return TileCollision.Passable;
            }

            return this.tiles[x, y].Collision;
        }

        //Updates all objects in the world, performs collision between them,
        //and handles the time limit with scoring.
        public void Update(
            GameTime gameTime,
            KeyboardState keyboardState,
            GamePadState gamePadState,
            AccelerometerState accelState,
            DisplayOrientation orientation
            )
        {
            if (!this.Player.IsAlive || this.TimeRemaining == TimeSpan.Zero)
            {
                //Still want to perform physics on the player.
                this.Player.ApplyPhysics(gameTime);
            }
            else if(ReachedExit)
            {
                //Animate the time being converted into points.
                int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 100.0f);
                seconds = Math.Min(seconds, (int)Math.Ceiling(this.TimeRemaining.TotalSeconds));
                timeRemaining -= TimeSpan.FromSeconds(seconds);
                score += seconds * GlobalConstants.LevelPointsPerSecond;
            }
            else
            {
                timeRemaining -= gameTime.ElapsedGameTime;
                this.Player.Update(gameTime, keyboardState, gamePadState, accelState, orientation);
                this.UpdateGems(gameTime);

                //Falling off the bottom of the level kills the player.
                if (this.Player.BoundingRectangle.Top >= this.Height * GlobalConstants.TileHeight)
                {
                    this.OnPlayerKilled(null);
                }

                this.UpdateEnemies(gameTime);

                //The player has reached the exit if they are standing on the ground and
                //his bounding rectangle contains the center of the exit tile. They can only
                //exit when they have collected all of the gems.
                if (this.Player.IsAlive && this.Player.IsOnGround && this.Player.BoundingRectangle.Contains(exit))
                {
                    this.OnExitReached();
                }
            }

            if (this.timeRemaining < TimeSpan.Zero)
            {
                timeRemaining = TimeSpan.Zero;
            }
        }

        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * GlobalConstants.TileWidth, y * GlobalConstants.TileHeight, GlobalConstants.TileWidth, GlobalConstants.TileHeight);
        }

        public void StartNewLife()
        {
            this.Player.Reset(this.start);
        }

        // Unloads the level content.
        public void Dispose()
        {
            this.Content.Unload();
        }

        private void UpdateGems(GameTime gameTime)
        {
            for (int i = 0; i < this.gems.Count; i++)
            {
                Gem gem = this.gems[i];
                gem.Update(gameTime);

                if (gem.BoundingCircle.Intersects(this.Player.BoundingRectangle))
                {
                    this.gems.RemoveAt(i--);
                    OnGemCollected(gem, this.Player);
                }
            }
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in this.enemies)
            {
                enemy.Update(gameTime);

                //Touching an enemy instantly kills the player
                if (enemy.BoundingRectangle.Intersects(this.Player.BoundingRectangle))
                {
                    if (enemy.IsAlive)
                    {
                        this.OnPlayerKilled(enemy); //if the enemy is alive then he is not instantly killed
                    }

                    if (enemy.IsAlive && enemy.BoundingRectangle.Intersects(this.Player.MeleeRectangle))
                    {
                        if (this.Player.isAttacking)
                        {
                            this.OnEnemyKilled(enemy, this.Player); // if the player is prunching, and his rectangle
                                                                    // interacts with the enemy's then the enemy will die
                        }
                    }
                }
            }
        }

        private void OnGemCollected(Gem gem, Player collectedBy)
        {
            this.score += GlobalConstants.GemPointValue;

            gem.OnCollected(collectedBy);
        }

        private void OnEnemyKilled(Enemy enemy, Player killedBy)
        {
            enemy.OnKilled(killedBy);
        }

        private void OnPlayerKilled(Enemy killedBy)
        {
            this.Player.OnKilled(killedBy); //enemy will die
        }

        private void OnExitReached()
        {
            this.Player.OnReachedExit();
            this.exitReachedSound.Play();
            this.reachedExit = true;
        }

        // this moves the world backwards, and keeps the camera inthe same position
        private void ScrollCamera(Viewport viewport)
        {
            // Calculate the edges of the screen.
            float marginWidth = viewport.Width * GlobalConstants.ViewMargin;
            float marginLeft = cameraPosition + marginWidth;
            float marginRight = cameraPosition + viewport.Width - marginWidth;

            // Calculate how far to scroll when the player is near the edges of the screen.
            float cameraMovement = 0.0f;
            if (Player.Position.X < marginLeft)
            {
                cameraMovement = Player.Position.X - marginLeft;
            }
            else if (Player.Position.X > marginRight)
            {
                cameraMovement = Player.Position.X - marginRight;
            }
                        
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
                    tiles[x, y] = this.LoadTile(tileType, x, y);
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

        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);
                // Exit
                case 'X':
                    return this.LoadExitTile(x, y);
                // Gem
                case 'G':
                    return this.LoadGemTile(x, y);
                // Floating platform
                case '-':
                    return this.LoadTile("Platform", TileCollision.Platform);

                // Various enemis
                case 'A':
                    return this.LoadEnemyTile(x, y, "MonsterA");
                case 'B':
                    return this.LoadEnemyTile(x, y, "MonsterB");
                case 'C':
                    return this.LoadEnemyTile(x, y, "MonsterC");
                case 'D':
                    return this.LoadEnemyTile(x, y, "MonsterD");

                //Platform block
                case '~':
                    return this.LoadVarietyTile("BlockB", 2, TileCollision.Platform);
                //Passable block
                case ':':
                    return this.LoadVarietyTile("BlockB", 2, TileCollision.Passable);

                //Player 1 start point
                case '1':
                    return this.LoadStartTile(x, y);

                //Impassable block
                case '#':
                    return LoadVarietyTile("BlockA", 7, TileCollision.Impassable);

                default:
                    throw new NotSupportedException(string.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        private Tile LoadStartTile(int x, int y)
        {
            if (this.Player != null)
            {
                throw new NotSupportedException("level may only have one starting point.");
            }

            this.start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            //Set player
            this.player = new Player(this, start);

            return new Tile(null, TileCollision.Passable);
        }

        //Loads a tile with a random appearance.
        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision tileCollision)
        {
            int index = random.Next(variationCount);
            return this.LoadTile(baseName + index, tileCollision);
        }

        //Instantiates an enemy and puts him in the level.
        private Tile LoadEnemyTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            this.enemies.Add(new Enemy(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadGemTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;           
            this.gems.Add(new Gem(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadExitTile(int x, int y)
        {
            if (this.exit != InvalidPosition)
            {
                throw new NotSupportedException("A level may only have one exit.");
            }

            this.exit = this.GetBounds(x, y).Center;

            return this.LoadTile("Exit", TileCollision.Passable);
        }

        //Loads a tile with a random appearance.
        private Tile LoadTile(string name, TileCollision tileCollision)
        {
            return new Tile(this.Content.Load<Texture2D>("Tiles/" + name), tileCollision);
        }
    }
}
