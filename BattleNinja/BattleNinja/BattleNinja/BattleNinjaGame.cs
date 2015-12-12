using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BattleNinja
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class BattleNinjaGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //Global content
        private SpriteFont hudFont;

        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;

        //Meta-level game state.
        private int levelIndex = -1;
        private Level level;
        private bool wasContinuePressed;

        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        // Store input states so that we only poll once per frame, 
        // then we use the same input state wherever needed
        private GamePadState gamePadState;
        private KeyboardState keyboardState;
        private AccelerometerState accelerometerState;
        private const int numberOfLevels = 3;
        
        public BattleNinjaGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Accelerometer.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load fonts
            hudFont = Content.Load<SpriteFont>("Fonts/Hud");

            // Load overlay textures
            winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = Content.Load<Texture2D>("Overlays/you_died");

            try
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(Content.Load<Song>("Sounds/war plains"));
            }
            catch { }

            LoadNextLevel();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Handle polling for our input and handling high-level input
            HandleInput();

            // update our level, passing down the GameTime along with all of our input states
            level.Update(gameTime, keyboardState, gamePadState,accelerometerState, Window.CurrentOrientation);

            base.Update(gameTime);
        }

        private void HandleInput()
        {
            // get all of our input states
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);
            accelerometerState = Accelerometer.GetState();

            // Exit the game when back is pressed.
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                Exit();

            bool continuePressed =
                keyboardState.IsKeyDown(Keys.Space) ||
                gamePadState.IsButtonDown(Buttons.A);
        

            // Perform the appropriate action to advance the game and
            // to get the player back to playing.
            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive)
                {
                    level.StartNewLife();
                }
                else if (level.TimeRemaining == TimeSpan.Zero)
                {
                    if (level.ReachedExit)
                        LoadNextLevel();
                    else
                        ReloadCurrentLevel();
                }
            }

            wasContinuePressed = continuePressed;
        }

        private void LoadNextLevel()
        {
            // move to the next level
            levelIndex = (levelIndex + 1) % numberOfLevels;

            // Unloads the content for the current level before loading the next one.
            if (level != null)
                level.Dispose();


            //CHECK THIS!!!
            using (FileStream stream = File.OpenRead(@"C:/Users/Aleksandar/Desktop/Levels/"+ levelIndex +".txt"))
            using (FileStream writeStream = File.OpenWrite(@"Content/Levels/" + levelIndex + ".txt"))
            {
                BinaryReader reader = new BinaryReader(stream);
                BinaryWriter writer = new BinaryWriter(writeStream);
                writer.Write(reader.ReadBytes((int)stream.Length));
            }

            // Load the level.
            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(Services, fileStream, levelIndex);
        }

        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            level.Draw(gameTime, spriteBatch);

            DrawHud();

            base.Draw(gameTime);
        }

        private void DrawHud()
        {
            spriteBatch.Begin();

            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            // Draw time remaining. Uses modulo division to cause blinking when the
            // player is running out of time.
            string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.Blue;
            }
            else
            {
                timeColor = Color.Red;
            }
            DrawShadowedString(hudFont, timeString, hudLocation, timeColor);

            // Draw score
            float timeHeight = hudFont.MeasureString(timeString).Y;
            DrawShadowedString(hudFont, "SCORE: " + level.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Blue);

            // Determine the status overlay message to show.
            Texture2D status = null;
            if (level.TimeRemaining == TimeSpan.Zero)
            {
                if (level.ReachedExit)
                {
                    status = winOverlay;
                }
                else
                {
                    status = loseOverlay;
                }
            }
            else if (!level.Player.IsAlive)
            {
                status = diedOverlay;
            }

            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }

            spriteBatch.End();
        }
        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }
    }
}
