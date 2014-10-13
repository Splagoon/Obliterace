using System;

namespace ObliteRace
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (ObliteRaceGame game = new ObliteRaceGame())
            {
                game.Run();
            }
        }
    }
}

