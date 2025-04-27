using Microsoft.Xna.Framework;
using System;

namespace DungeonFlux.Model
{
    public class GameModel
    {
        public Room[,] Dungeon { get; private set; }
        public Vector2 PlayerPosition { get; private set; }
        public bool IsGameOver { get; private set; }

        private readonly DungeonGenerator _dungeonGenerator;

        public GameModel()
        {
            _dungeonGenerator = new DungeonGenerator(
                GameSettings.Dungeon.Width, 
                GameSettings.Dungeon.Height, 
                GameSettings.Dungeon.MinRooms, 
                GameSettings.Dungeon.MaxRooms
            );
            GenerateNewDungeon();
        }

        public void GenerateNewDungeon()
        {
            Dungeon = _dungeonGenerator.Generate();
            Console.WriteLine($"Dungeon generated with size: {Dungeon.GetLength(0)}x{Dungeon.GetLength(1)}");

            bool startFound = false;
            for (int x = 0; x < GameSettings.Dungeon.Width; x++)
            {
                for (int y = 0; y < GameSettings.Dungeon.Height; y++)
                {
                    if (Dungeon[x, y]?.Type == RoomType.Start)
                    {
                        PlayerPosition = new Vector2(x, y);
                        startFound = true;
                        Console.WriteLine($"Start position found at: {x}, {y}");
                        break;
                    }
                }
                if (startFound) break;
            }

            if (!startFound)
            {
                Console.WriteLine("Warning: Start position not found!");
                PlayerPosition = new Vector2(0, 0);
            }

            IsGameOver = false;
        }

        public bool TryMovePlayer(int direction)
        {
            Point[] directions = new[]
            {
                    new Point(0, -1),  // Up
                    new Point(1, 0),   // Right
                    new Point(0, 1),   // Down
                    new Point(-1, 0)   // Left
            }; // TODO: вынести дирекшнс в гейм сеттингс

            Point currentRoom = new Point((int)PlayerPosition.X, (int)PlayerPosition.Y);
            Point newRoom = currentRoom + directions[direction];

            if (newRoom.X < 0 || newRoom.X >= GameSettings.Dungeon.Width || 
                newRoom.Y < 0 || newRoom.Y >= GameSettings.Dungeon.Height)
                return false;

            if (Dungeon[newRoom.X, newRoom.Y] == null || 
                !Dungeon[currentRoom.X, currentRoom.Y].HasConnection(direction))
                return false;

            PlayerPosition = new Vector2(newRoom.X, newRoom.Y);
            Console.WriteLine($"Player moved to: {newRoom.X}, {newRoom.Y}");

            if (Dungeon[newRoom.X, newRoom.Y].Type == RoomType.Exit)
            {
                IsGameOver = true;
            }

            return true;
        }
    }
}