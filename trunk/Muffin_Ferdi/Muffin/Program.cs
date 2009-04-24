using System;

namespace Muffin
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (MuffinGame game = new MuffinGame())
            {
                game.Run();
            }
        }
    }
}

