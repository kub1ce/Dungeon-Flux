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
                GameSettings.Dungeon.Size.Width, 
                GameSettings.Dungeon.Size.Height, 
                GameSettings.Dungeon.RoomCount.Min, 
                GameSettings.Dungeon.RoomCount.Max
            );
            GenerateNewDungeon();
        }

        public void GenerateNewDungeon()
        {
            Dungeon = _dungeonGenerator.Generate();
            Logger.Log($"Dungeon generated with size: {Dungeon.GetLength(0)}x{Dungeon.GetLength(1)}");

            SetPlayerStartPosition();
            IsGameOver = false;
        }

        private void SetPlayerStartPosition()
        {
            var startPosition = FindStartRoom();
            if (startPosition.HasValue)
            {
                PlayerPosition = new Vector2(startPosition.Value.X, startPosition.Value.Y);
                Logger.Log($"Start position found at: {startPosition.Value.X}, {startPosition.Value.Y}");
            }
            else
            {
                Logger.LogWarning("Start position not found! Setting default position (0,0)");
                PlayerPosition = Vector2.Zero;
            }
        }

        private Point? FindStartRoom()
        {
            for (int x = 0; x < GameSettings.Dungeon.Size.Width; x++)
            {
                for (int y = 0; y < GameSettings.Dungeon.Size.Height; y++)
                {
                    if (Dungeon[x, y]?.Type == RoomType.Start)
                    {
                        return new Point(x, y);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Пытается переместить игрока в указанном направлении
        /// </summary>
        /// <param name="direction">Направление движения (0 - вверх, 1 - вправо, 2 - вниз, 3 - влево)</param>
        /// <returns>true, если перемещение успешно, иначе false</returns>
        public bool TryMovePlayer(int direction)
        {
            if (direction < 0 || direction >= GameSettings.Directions.All.Length)
            {
                Logger.LogWarning($"Invalid direction: {direction}");
                return false;
            }

            Point currentRoom = new Point((int)PlayerPosition.X, (int)PlayerPosition.Y);
            Point newRoom = currentRoom + GameSettings.Directions.All[direction];

            if (!IsValidRoomPosition(newRoom))
                return false;

            if (!CanMoveToRoom(currentRoom, newRoom, direction))
                return false;

            PlayerPosition = new Vector2(newRoom.X, newRoom.Y);
            // Logger.Log($"Player moved to: {newRoom.X}, {newRoom.Y}");

            CheckForExitRoom(newRoom);

            return true;
        }

        private bool IsValidRoomPosition(Point position)
        {
            return position.X >= 0 && position.X < GameSettings.Dungeon.Size.Width &&
                   position.Y >= 0 && position.Y < GameSettings.Dungeon.Size.Height;
        }

        private bool CanMoveToRoom(Point currentRoom, Point newRoom, int direction)
        {
            return Dungeon[newRoom.X, newRoom.Y] != null && 
                   Dungeon[currentRoom.X, currentRoom.Y].HasConnection(direction);
        }

        private void CheckForExitRoom(Point roomPosition)
        {
            if (Dungeon[roomPosition.X, roomPosition.Y].Type == RoomType.Exit)
            {
                IsGameOver = true;
                Logger.Log("Player reached the exit! Game over.");
            }
        }
    }
}