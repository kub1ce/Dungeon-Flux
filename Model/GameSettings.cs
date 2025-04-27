using Microsoft.Xna.Framework;

namespace DungeonFlux.Model
{
    public static class GameSettings
    {
        // Player settings
        public static class Player
        {
            public static readonly Vector2 DefaultStartPosition = new Vector2(400, 300);
            public static readonly Point GameStartPosition = new Point(400, 300);

            public static readonly float MoveSpeed = .12f;
            public static readonly int MaxHealth = 100;

            public static readonly int Size = 16;
        }

        // Graphics settings
        public static class Graphics
        {
            public static readonly Color BackgroundColor = new Color(20, 20, 30);

            public static readonly int RoomSize = 32;
            public static readonly int WallThickness = 4;
            public static readonly int Padding = 32;
        }

        // Dungeon settings
        public static class Dungeon
        {
            public static readonly int Width = 64;
            public static readonly int Height = 64;

            public static readonly int MinRooms = 1024;
            public static readonly int MaxRooms = 1024;
        }

        // Room settings
        public static class Room
        {
            public static readonly int ConnectionCount = 4;
        }

        public static class Directions
        {
            public static readonly Point[] All = new[]
            {
                new Point(0, -1),  // Up
                new Point(1, 0),   // Right
                new Point(0, 1),   // Down
                new Point(-1, 0)   // Left
            };

            public const int Up = 0;
            public const int Right = 1;
            public const int Down = 2;
            public const int Left = 3;
        }
    }
} 