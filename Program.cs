using System;
using System.IO;

namespace DungeonFlux
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting game...");
                using var game = new DungeonFluxGame();
                game.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                throw;
            }
        }
    }
}