using System;

namespace _3D_Renderer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Renderer game = new Renderer())
            {
                game.Run();
            }
        }
    }
}

