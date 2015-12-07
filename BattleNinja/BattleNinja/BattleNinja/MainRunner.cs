using System;

namespace BattleNinja
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (BattleNinjaGame game = new BattleNinjaGame())
            {
                game.Run();
            }
        }
    }
}

