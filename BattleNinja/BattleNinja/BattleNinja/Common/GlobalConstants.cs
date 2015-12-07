using Microsoft.Xna.Framework.Input;

namespace BattleNinja.Common
{
    public class GlobalConstants
    {
        public const int TileWidth = 40;
        public const int TileHeight = 32;

        public const int GemPointValue = 50; //Points given to the player for collecting a icon

        public const float deathTimeMax = 1.0f;

        //How long to wait before turning around;
        public const float EnemyMaxWaitTime = 0.5f;
        //The speed at which this enemy moves along the X axis;
        public const float EnemyMoveSpeed = 64.0f;

        public const float ViewMargin = 0.35f;

        public const int EntityLayer = 2; // The layer which entities are drawn on top of.

        // Constants for controling horizontal movement
        public const float MoveAcceleration = 13000.0f;
        public const float MaxMoveSpeed = 1750.0f;
        public const float GroundDragFactor = 0.48f;
        public const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        public const float MaxJumpTime = 0.35f;
        public const float JumpLaunchVelocity = -3500.0f;
        public const float GravityAcceleration = 3400.0f;
        public const float MaxFallSpeed = 550.0f;
        public const float JumpControlPower = 0.14f;

        // Input configuration
        public const float MoveStickScale = 1.0f;
        public const float AccelerometerScale = 1.5f;
        public const Buttons JumpButton = Buttons.A;

        public const float MaxAttackTime = 0.33f;
       

    }
}
