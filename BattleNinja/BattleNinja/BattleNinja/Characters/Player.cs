using System;
using BattleNinja.Animations;
using BattleNinja.Common;
using BattleNinja.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BattleNinja.Characters
{
    public class Player
    {
        KeyboardState previousKeyboardState = Keyboard.GetState();
        // Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation celebrateAnimation;
        private Animation dieAnimation;
        private Animation attackAnimation; // declaring the attack animation
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;

        // Sounds
        private SoundEffect killedSound;
        private SoundEffect jumpSound;
        private SoundEffect fallSound;

        private Level level;
        private bool isAlive;
        private bool isOnGround;
        private Vector2 velocity;
        private Vector2 position;
        private float previousBottom;
        private int numberOfJumps = 0;

        // Current user movement input.
        private float movement;

        // Jumping state
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        // Attacking state
        public bool isAttacking;
        public float AttackTime;

        private Rectangle localBounds;

        // Constructors a new player.
        public Player(Level level, Vector2 position)
        {
            this.level = level;

            LoadContent();

            Reset(position);
        }

        // Gets a rectangle which bounds this player in world space.
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        //drawing the rectangle around the sprite sheet.
        //once the attack button is pressed, if part of that rectangle is touching the enemy, the enemy will die
        public Rectangle MeleeRectangle 
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;
                
                if (flip == SpriteEffects.FlipHorizontally)
                {
                    return new Rectangle(
                        (left + localBounds.Width),
                        top,
                        localBounds.Width,
                        localBounds.Height);
                }
                else
                {
                    return new Rectangle(
                        (left - localBounds.Width),
                        top,
                        localBounds.Width,
                        localBounds.Height);
                }
            }
        }

        public Level Level
        {
            get { return level; }
        }
        
        public bool IsAlive
        {
            get { return isAlive; }
        }

        // Physics state
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        
        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        // Gets whether or not the player's feet are on the ground.
        public bool IsOnGround
        {
            get { return isOnGround; }
        }


        /// Loads the player sprite sheet and sounds.
        public void LoadContent()
        {
            // Load animated textures.
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/mysprites/idle2"), 0.1f, true);
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/mysprites/walk2"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/mysprites/Jump2"), 0.1f, false);
            celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/mysprites/idle2"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/mysprites/damage2"), 0.1f, false);
            attackAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/mysprites/attack2"), 0.1f, false);

            // Calculate bounds within texture size.            
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Load sounds.            
            killedSound = Level.Content.Load<SoundEffect>("Sounds/Horse grunt 3");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/Grunt");
            fallSound = Level.Content.Load<SoundEffect>("Sounds/Grunt");
        }

        /// Resets the player to life.
        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
        }

        // Handles input, performs physics, and animates the player sprite.
        // Pass the game's orientation because when using the accelerometer,
        // we need to reverse our motion when the orientation is in the LandscapeRight orientation.
        public void Update(
            GameTime gameTime,
            KeyboardState keyboardState,
            GamePadState gamePadState,
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            GetInput(keyboardState, gamePadState, accelState, orientation);

            if (previousKeyboardState.IsKeyUp(Keys.F) && keyboardState.IsKeyDown(Keys.F))
            {
                if (AttackTime != GlobalConstants.MaxAttackTime)
                {
                    isAttacking = true;
                    AttackTime = GlobalConstants.MaxAttackTime;
                }
            }

            DoAttack(gameTime);

            ApplyPhysics(gameTime);

            if (IsAlive && IsOnGround)
            {
                if (isAttacking)
                {
                    sprite.PlayAnimation(attackAnimation);

                }
                else
                {
                    if (Math.Abs(Velocity.X) - 0.02f > 0)
                    {
                        sprite.PlayAnimation(runAnimation);
                    }
                    else
                    {
                        sprite.PlayAnimation(idleAnimation);
                    }
                }
            }

            //Basically saying if the player is alive, and is on the ground
            //then you can play the animation. and if he is in the air, then it applies velocity

            // Clear input.
            movement = 0.0f;
            isJumping = false;
            if (isOnGround)
            {
                numberOfJumps = 0;
            }
        }
        // Gets player horizontal movement and jump commands from input.
        private void GetInput(
            KeyboardState keyboardState,
            GamePadState gamePadState,
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            // Get analog horizontal movement.
            movement = gamePadState.ThumbSticks.Left.X * GlobalConstants.MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // Move the player with accelerometer
            if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
            {
                // set our movement speed
                movement = MathHelper.Clamp(-accelState.Acceleration.Y * GlobalConstants.AccelerometerScale, -1f, 1f);

                // if we're in the LandscapeLeft orientation, we must reverse our movement
                if (orientation == DisplayOrientation.LandscapeRight)
                    movement = -movement;
            }

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.0f;
            }
            else if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0f;
            }

            // Check if the player wants to jump.
            isJumping =
                gamePadState.IsButtonDown(GlobalConstants.JumpButton) ||
                keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.Up) ||
                keyboardState.IsKeyDown(Keys.W);
        }

        // Updates the player's velocity and position based on input, gravity, etc.
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * GlobalConstants.MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GlobalConstants.GravityAcceleration * elapsed, -GlobalConstants.MaxFallSpeed, GlobalConstants.MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GlobalConstants.GroundDragFactor;
            else
                velocity.X *= GlobalConstants.AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -GlobalConstants.MaxMoveSpeed, GlobalConstants.MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }

        private void DoAttack(GameTime gametime)
        {
            //if the player would like to do an attack
            if (isAttacking)
            {
                // Stars or continues the attack
                if (AttackTime > 0.0f)
                {
                    AttackTime -= (float)gametime.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    isAttacking = false;
                }
            }
            else
            {
                //Continues not attack or cancels an attack in progress
                AttackTime = 0.0f;
            }
        }

        // Calculates the Y velocity accounting for jumping and animates accordingly.
        // The jump velocity is controlled by the jumpTime field which measures time into the accent of the current jump.
        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    if (jumpTime == 0.0f)
                        jumpSound.Play();

                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the incline of the jump
                if (0.0f < jumpTime && jumpTime <= GlobalConstants.MaxJumpTime)
                {
                    //Overrides the vertical velocity with a power curve that
                    //gives players more control over the top of the jump
                    velocityY =
                        GlobalConstants.JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / GlobalConstants.MaxJumpTime, GlobalConstants.JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump and has double jumps
                    if (velocityY > -GlobalConstants.MaxFallSpeed * 0.5f && !wasJumping && numberOfJumps < 1)
                    {
                        velocityY =
                            GlobalConstants.JumpLaunchVelocity * (0.5f - (float)Math.Pow(jumpTime / GlobalConstants.MaxJumpTime, GlobalConstants.JumpControlPower));
                        jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        numberOfJumps++;
                    }
                    else
                    {
                        jumpTime = 0.0f;
                    }
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }

        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one axis to prevent overlapping.
        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / GlobalConstants.TileWidth);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / GlobalConstants.TileWidth)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / GlobalConstants.TileHeight);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / GlobalConstants.TileHeight)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }
            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }

        /// Called when the player has been killed.
        public void OnKilled(Enemy killedBy)
        {
            isAlive = false;

            if (killedBy != null)
            {
                killedSound.Play();
            }
            else
            {
                fallSound.Play();
            }
            sprite.PlayAnimation(dieAnimation);
        }

        public void OnReachedExit()
        {
            sprite.PlayAnimation(celebrateAnimation);
        }

        // Draws the animated player.
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Flip the sprite to face the way we are moving.
            if (Velocity.X > 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X < 0)
                flip = SpriteEffects.None;

            // Draw that sprite.
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }

    }
}
