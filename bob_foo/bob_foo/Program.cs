using System;

namespace bob_foo
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Engine game = new Engine())
            {
                game.Run();
            }
        }
    }
#endif
}

