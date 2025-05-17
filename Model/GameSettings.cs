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

            public static readonly float MoveSpeed = .24f * 4;
            public static readonly int MaxHealth = 100;

            public static readonly int Size = 112;
        }

        // Weapon settings
        public static class Weapon
        {
            public static class Default
            {
                public const float Cooldown = 0.5f;  // 0.5 секунды между атаками
                public const int Damage = 10;        // 10 единиц урона
                public const float Range = 480f;     // дальности атаки
            }

            public static class AttackEffect
            {
                public const float Duration = 0.15f; // Длительность эффекта в секундах
                public const float Range = Default.Range;
                public static readonly Color Color = Color.Yellow;
                public const int Thickness = 4; // Толщина линии
            }
        }

        // Graphics settings
        public static class Graphics
        {
            public static readonly Color BackgroundColor = new Color(20, 20, 30);

            public static readonly int RoomSize = 1536;
            public static readonly int WallThickness = 32;
            public static readonly int Padding = 0;
            public static readonly float Scale = 1f;

            public static class Corridor
            {
                public const float WidthRatio = 0.3f; // Ширина коридора 30% от размера комнаты
                public const float CenterOffsetRatio = 0.5f; // Смещение центра коридора
            }

            public static class WallGeneration
            {
                public const float ShortWallLengthRatio = 0.35f; // Длина короткой стены относительно размера комнаты
                public const float LongWallLengthRatio = 0.67f; // Длина длинной стены относительно размера комнаты
                public const float ExternalWallLengthRatio = 0.65f; // Длина внешней стены относительно размера комнаты
                public const float ExternalOffsetRatio = 0.28f; // Смещение внешней стены относительно размера комнаты
            }

            public static class RoomColors
            {
                public static readonly Color Default = Color.Gray;

                public static readonly Color StartRoom = Color.Green;
                public static readonly Color ExitRoom = Color.Red;
                public static readonly Color BossRoom = Color.Purple;
                public static readonly Color Corridor = Color.Gray;
                public static readonly Color DeadEnd = Default; // Color.Orange;
            }

            public static class WallColors
            {
                public static readonly Color Corridor = Color.DarkGray;
                public static readonly Color Room = Color.DarkGray;
            }
        }

        // Dungeon settings
        public static class Dungeon
        {
            public static class Size
            {
                public static readonly int Width = 64;
                public static readonly int Height = 64;
            }

            public static class RoomCount
            {
                public static readonly int Min = 12;
                public static readonly int Max = 64;
            }

            public static class RoomSize
            {
                public static readonly Point Default = new Point(1, 1);
            }

            /// <summary>
            /// Вероятности появления разных типов комнат
            /// </summary>
            public static class RoomProbabilities
            {
                public const double Empty = 0.4;    // 40%
                public const double Enemy = 0.2;    // 20%
                public const double Treasure = 0.15; // 15%
                public const double Shop = 0.1;     // 10%
                public const double Boss = 0.15;    // 15%
            }
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

        // Camera settings
        public static class Camera
        {
            public static class BoundingBox
            {
                public const float WidthInPlayers = 3f;
                public const float HeightInPlayers = 2f; // Мб сразу тут считать в размене игрока и возвращать уже значения??
            }

            public static class ScreenLimits
            {
                // Максимальная площадь бокса относительно площади экрана
                public const float MaxWidthFraction = 0.5f;
                public const float MaxHeightFraction = 1f / 3f; // Аналогично???
            }
        }

        // Debug settings
        public static class Debug
        {
            public static bool IsDebugModeEnabled = false; // !!! ВКЛ по умолчанию. не забыть выключить при релизе
            public const Microsoft.Xna.Framework.Input.Keys DebugToggleKey = Microsoft.Xna.Framework.Input.Keys.F3;

            public static class BoundingBox
            {
                public const int BorderThickness = 2;
                public static readonly Color Color = Color.Yellow;
            }

            public static class CenterLine
            {
                public const int Thickness = 2;
                public static readonly Color Color = Color.Cyan;
            }

            public static class CorridorBorder
            {
                public const int Thickness = 2;
                public static readonly Color Color = Color.Red;
            }
        }
    }
} 