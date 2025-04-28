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

            public static readonly float MoveSpeed = .024f * 1.5f;
            public static readonly int MaxHealth = 100;

            public static readonly int Size = 112;
        }

        // Graphics settings
        public static class Graphics
        {
            public static readonly Color BackgroundColor = new Color(20, 20, 30);

            public static readonly int RoomSize = 768;
            public static readonly int WallThickness = 32;
            public static readonly int Padding = 0;
        }

        // Dungeon settings
        public static class Dungeon
        {
            public static readonly int Width = 64;
            public static readonly int Height = 64;

            public static readonly int MinRooms = 12;
            public static readonly int MaxRooms = 64;
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

        // Camera bounding box settings
        public static class Camera
        {
            public const float BoundingBoxWidthInPlayers = 3f;
            public const float BoundingBoxHeightInPlayers = 2f; // Мб сразу тут считать в размене игрока и возвращать уже значения??

            // Максимальная площадь бокса относительно площади экрана
            public const float MaxScreenWidthFraction = 0.5f;
            public const float MaxScreenHeightFraction = 1f / 3f; // Аналогично???
        }

        // Debug settings
        public static class Debug
        {
            public static bool IsDebugModeEnabled = true; // !!! ВКЛ по умолчанию. не забыть выключить при релизе
            public const Microsoft.Xna.Framework.Input.Keys DebugToggleKey = Microsoft.Xna.Framework.Input.Keys.P;
            
            public const int CenterLineThickness = 2;
            public static readonly Color CenterLineColor = Color.Cyan;

            public const int BoundingBoxBorderThickness = 4;
            public static readonly Color BoundingBoxColor = Color.Gold;
        }
    }
} 