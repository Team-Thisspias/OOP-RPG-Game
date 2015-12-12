namespace BattleNinja.Characters
{
    using System;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Graphics;

    using BattleNinja.Animations;
    using BattleNinja.Common;
    using BattleNinja.Enums;

    public class Enemy
    {
        private Level level;
        private Vector2 position;
        private Rectangle localBounds;

        //Animations
        private Animation runAnimation;
        private Animation idleAnimation;
        private Animation dieAnimation;
        private AnimationPlayer sprite;

        //Sounds
        private SoundEffect killedSound;

        //The direction this enemy is facing and moving alon the X axis;
        private FaceDirection direction = FaceDirection.Left;
        //How long this enemy has been waiting before turning around;
        private float waitTime;


        public Enemy(Level level, Vector2 position, string spriteSet)
        {
            this.level = level;
            this.position = position;
            this.IsAlive = true; //he is alive 

            this.LoadContent(spriteSet);
        }


        public Level Level
        {
            get { return level; }
        }

        public Vector2 Position
        {
            get { return position; }
        }

        public float deathTime = GlobalConstants.deathTimeMax;

        public bool IsAlive { get; private set; }

        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(this.Position.X - sprite.Origin.X) + this.localBounds.X;
                int top = (int)Math.Round(this.Position.Y - sprite.Origin.Y) + this.localBounds.Y;

                return new Rectangle(left, top, this.localBounds.Width, this.localBounds.Height);
            }
        }

        public void LoadContent(string spriteSet)
        {
            //Load animations.
            spriteSet = "Sprites/" + spriteSet + "/";
            this.runAnimation = new Animation(this.Level.Content.Load<Texture2D>(spriteSet + "Run monster A"), 0.1f, true);
            this.idleAnimation = new Animation(this.Level.Content.Load<Texture2D>(spriteSet + "Idle monster A"), 0.15f, true);
            this.dieAnimation = new Animation(this.Level.Content.Load<Texture2D>(spriteSet + "Die monster A"), 0.07f, false);
            
            //Load sounds.
            this.killedSound = this.Level.Content.Load<SoundEffect>("Sounds/Horse grunt 1");

            //Calculate bounds within texture size.
            int width = (int)(this.idleAnimation.FrameWidth * 0.35);
            int left = (this.idleAnimation.FrameWidth - width) / 2;
            int height = (int)(this.idleAnimation.FrameWidth * 0.7);
            int top = this.idleAnimation.FrameHeight - height;
            this.localBounds = new Rectangle(left, top, width, height);
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Calculate tile position based on the side we are walking towards.
            float posX = this.Position.X + this.localBounds.Width / 2 * (int)this.direction;
            int tileX = (int)Math.Floor(posX / GlobalConstants.TileWidth) - (int)this.direction;
            int tileY = (int)Math.Floor(this.Position.Y / GlobalConstants.TileHeight);

            if (this.waitTime > 0)
            {
                //Wait for some amount of time.
                this.waitTime = Math.Max(0.0f, this.waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (this.waitTime <= 0.0f)
                {
                    //Then turn around.
                    this.direction = (FaceDirection)(-(int)this.direction);
                }
            }
            else
            {
                //If we are about to run into a wall or off a cliff  start waiting.
                if (this.Level.GetCollision(tileX + (int)this.direction, tileY - 1) == TileCollision.Impassable ||
                    this.Level.GetCollision(tileX + (int)this.direction, tileY) == TileCollision.Passable)
                {
                    this.waitTime = GlobalConstants.EnemyMaxWaitTime;
                }
                else
                {
                    //Move in the current directin.
                    Vector2 velocity = new Vector2((int)this.direction * GlobalConstants.EnemyMoveSpeed * elapsed, 0.0f);
                    this.position = this.position + velocity;
                }
                if (!this.IsAlive)
                {
                    this.deathTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
            }
        }

        public void OnKilled(Player killedBy)
        {
            this.IsAlive = false;
            this.killedSound.Play(); // once player is dead, play the killed sound for enemy
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //Play deatch animation if we're dead, stop running
            //When the game is paused or before turning around.
            if (this.deathTime < GlobalConstants.deathTimeMax)
            {
                this.sprite.PlayAnimation(this.dieAnimation);
            }
            else if(!this.Level.Player.IsAlive ||
                this.Level.TimeRemaining == TimeSpan.Zero ||
                this.waitTime > 0)
            {
                this.sprite.PlayAnimation(this.idleAnimation);
            }
            else
            {
                this.sprite.PlayAnimation(this.runAnimation);
            }

            SpriteEffects flip = this.direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            this.sprite.Draw(gameTime, spriteBatch, this.Position, flip);
        }
    }
}
