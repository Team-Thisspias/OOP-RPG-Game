using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BattleNinja
{
    public struct AccelerometerState
    {
        /// Gets the accelerometer's current value in G-force.
        public Vector3 Acceleration { get; private set; }

        /// Gets whether or not the accelerometer is active and running.
        public bool IsActive { get; private set; }

        /// Initializes a new AccelerometerState.
        public AccelerometerState(Vector3 acceleration, bool isActive)
            : this()
        {
            Acceleration = acceleration;
            IsActive = isActive;
        }

        public override string ToString()
        {
            return string.Format("Acceleration: {0}, IsActive: {1}", Acceleration, IsActive);
        }
        
    }
}
