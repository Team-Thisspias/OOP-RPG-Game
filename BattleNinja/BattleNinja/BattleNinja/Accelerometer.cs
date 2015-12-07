using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BattleNinja
{
    public static class Accelerometer
    {
        // we want to prevent the Accelerometer from being initialized twice.
        private static bool isInitialized = false;

        // whether or not the accelerometer is active
        private static bool isActive = false;

        /// Initializes the Accelerometer for the current game. This method can only be called once per game.
        public static void Initialize()
        {
            // make sure we don't initialize the Accelerometer twice
            if (isInitialized)
            {
                throw new InvalidOperationException("Initialize can only be called once");
            }
            // remember that we are initialized
            isInitialized = true;
        }
        /// Gets the current state of the accelerometer.
        public static AccelerometerState GetState()
        {
            // make sure we've initialized the Accelerometer before we try to get the state
            if (!isInitialized)
            {
                throw new InvalidOperationException("You must Initialize before you can call GetState");
            }

            // create a new value for our state
            Vector3 stateValue = new Vector3();
            return new AccelerometerState(stateValue, isActive);
        }
    }
}
