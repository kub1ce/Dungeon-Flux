using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonFlux.Model
{
    public class GameModel
    {
        public Room[,] Dungeon { get; private set; }
        public Vector2 PlayerPosition { get; private set; }
        public bool IsGameOver { get; private set; }
        public List<Wall> Walls { get; private set; }
        private bool _allDoorsOpen = false;

        private readonly DungeonGenerator _dungeonGenerator;

        public GameModel()
        {
            _dungeonGenerator = new DungeonGenerator(
                GameSettings.Dungeon.Size.Width, 
                GameSettings.Dungeon.Size.Height, 
                GameSettings.Dungeon.RoomCount.Min, 
                GameSettings.Dungeon.RoomCount.Max
            );
            Walls = new List<Wall>();
            GenerateNewDungeon();
        }

        public void GenerateNewDungeon()
        {
            Dungeon = _dungeonGenerator.Generate();
            Logger.Log($"Dungeon generated with size: {Dungeon.GetLength(0)}x{Dungeon.GetLength(1)}");

            SetPlayerStartPosition();
            GenerateWalls();
            IsGameOver = false;
        }

        private void GenerateWalls()
        {
            Walls.Clear();
            int roomSize = GameSettings.Graphics.RoomSize;
            int wallThickness = GameSettings.Graphics.WallThickness;
            int corridorWidth = (int)(roomSize * GameSettings.Graphics.Corridor.WidthRatio);
            int centerOffset = (roomSize - corridorWidth) / 2;

            for (int x = 0; x < Dungeon.GetLength(0); x++)
            {
                for (int y = 0; y < Dungeon.GetLength(1); y++)
                {
                    var room = Dungeon[x, y];
                    if (room == null) continue;

                    Vector2 roomPos = new Vector2(x * roomSize, y * roomSize);

                    if (room.Type == RoomType.Corridor)
                    {
                        GenerateCorridorWalls(room, roomPos, roomSize, wallThickness, corridorWidth, centerOffset);
                    }
                    else
                    {
                        GenerateRoomWalls(room, roomPos, roomSize, wallThickness, corridorWidth, centerOffset);
                    }
                }
            }
        }

        private void GenerateCorridorWalls(Room room, Vector2 position, int size, int wallThickness, int corridorWidth, int centerOffset)
        {
            int x = (int)position.X;
            int y = (int)position.Y;
            
            // Count connections and store their directions
            List<int> connections = new List<int>();
            for (int dir = 0; dir < 4; dir++)
            {
                if (room.HasConnection(dir))
                {
                    connections.Add(dir);
                }
            }

            switch (connections.Count)
            {
                case 2:
                    HandleTwoConnections(room, x, y, size, wallThickness, corridorWidth, centerOffset, connections);
                    break;
                case 3:
                    HandleThreeConnections(room, x, y, size, wallThickness, corridorWidth, centerOffset, connections);
                    break;
                case 4:
                    HandleFourConnections(room, x, y, size, wallThickness, corridorWidth, centerOffset);
                    break;
            }
        }

        private void HandleTwoConnections(Room room, int x, int y, int size, int wallThickness, int corridorWidth, int centerOffset, List<int> connections)
        {
            // Check if connections are opposite
            bool areOpposite = (connections[0] + 2) % 4 == connections[1];

            if (areOpposite)
            {
                // Add long walls on sides without connections
                int[] sidesWithoutConnections = Enumerable.Range(0, 4)
                    .Where(dir => !connections.Contains(dir))
                    .ToArray();

                foreach (int dir in sidesWithoutConnections)
                {
                    switch (dir)
                    {
                        case GameSettings.Directions.Up:
                            Walls.Add(new Wall(
                                new Rectangle(x, y + centerOffset, size, wallThickness),
                                true, dir, false));
                            break;
                        case GameSettings.Directions.Right:
                            Walls.Add(new Wall(
                                new Rectangle(x + centerOffset + corridorWidth - wallThickness, y, wallThickness, size),
                                true, dir, false));
                            break;
                        case GameSettings.Directions.Down:
                            Walls.Add(new Wall(
                                new Rectangle(x, y + centerOffset + corridorWidth - wallThickness, size, wallThickness),
                                true, dir, false));
                            break;
                        case GameSettings.Directions.Left:
                            Walls.Add(new Wall(
                                new Rectangle(x + centerOffset, y, wallThickness, size),
                                true, dir, false));
                            break;
                    }
                }
            }
            else
            {
                float wallLength = size * GameSettings.Graphics.WallGeneration.ShortWallLengthRatio;
                float longWallLength = size * GameSettings.Graphics.WallGeneration.LongWallLengthRatio;
                float externalWallLength = size * GameSettings.Graphics.WallGeneration.ExternalWallLengthRatio;
                float externalOffset = size * GameSettings.Graphics.WallGeneration.ExternalOffsetRatio;

                foreach (int dir in connections)
                {
                    // Get adjacent sides (clockwise and counterclockwise)
                    int clockwiseAdjacent = (dir + 1) % 4;
                    int counterClockwiseAdjacent = (dir + 3) % 4;
                    int oppositeSide = (dir + 2) % 4;

                    // Check if adjacent sides have connections
                    bool hasClockwiseConnection = room.HasConnection(clockwiseAdjacent);
                    bool hasCounterClockwiseConnection = room.HasConnection(counterClockwiseAdjacent);

                    // Add walls based on adjacent connections
                    switch (dir)
                    {
                        case GameSettings.Directions.Up:
                            if (hasClockwiseConnection)
                            {
                                Walls.Add(new Wall(
                                    new Rectangle((int)(x + size - wallLength), y + centerOffset, (int)wallLength, wallThickness),
                                    true, dir, false));
                                Walls.Add(new Wall(
                                    new Rectangle((int)(x + size - externalWallLength), (int)(y + centerOffset + externalOffset), (int)externalWallLength, wallThickness),
                                    true, dir, false));
                            }
                            if (hasCounterClockwiseConnection)
                            {
                                Walls.Add(new Wall(
                                    new Rectangle(x, y + centerOffset, (int)wallLength, wallThickness),
                                    true, dir, false));
                                Walls.Add(new Wall(
                                    new Rectangle(x, (int)(y + centerOffset + externalOffset), (int)externalWallLength, wallThickness),
                                    true, dir, false));
                            }
                            break;

                        case GameSettings.Directions.Right:
                            if (hasClockwiseConnection)
                            {
                                Walls.Add(new Wall(
                                    new Rectangle(x + centerOffset + corridorWidth - wallThickness, (int)(y + size - wallLength), wallThickness, (int)wallLength),
                                    true, dir, false));
                                Walls.Add(new Wall(
                                    new Rectangle((int)(x + centerOffset + corridorWidth - wallThickness - externalOffset), (int)(y + size - externalWallLength), wallThickness, (int)externalWallLength),
                                    true, dir, false));
                            }
                            if (hasCounterClockwiseConnection)
                            {
                                Walls.Add(new Wall(
                                    new Rectangle(x + centerOffset + corridorWidth - wallThickness, y, wallThickness, (int)wallLength),
                                    true, dir, false));
                                Walls.Add(new Wall(
                                    new Rectangle((int)(x + centerOffset + corridorWidth - wallThickness - externalOffset), y, wallThickness, (int)externalWallLength),
                                    true, dir, false));
                            }
                            break;

                        case GameSettings.Directions.Down:
                            if (hasClockwiseConnection)
                            {
                                Walls.Add(new Wall(
                                    new Rectangle(x, y + centerOffset + corridorWidth - wallThickness, (int)wallLength, wallThickness),
                                    true, dir, false));
                                Walls.Add(new Wall(
                                    new Rectangle(x, (int)(y + centerOffset + corridorWidth - wallThickness - externalOffset), (int)externalWallLength, wallThickness),
                                    true, dir, false));
                            }
                            if (hasCounterClockwiseConnection)
                            {
                                Walls.Add(new Wall(
                                    new Rectangle((int)(x + size - wallLength), y + centerOffset + corridorWidth - wallThickness, (int)wallLength, wallThickness),
                                    true, dir, false));
                                Walls.Add(new Wall(
                                    new Rectangle((int)(x + size - externalWallLength), (int)(y + centerOffset + corridorWidth - wallThickness - externalOffset), (int)externalWallLength, wallThickness),
                                    true, dir, false));
                            }
                            break;

                        case GameSettings.Directions.Left:
                            if (hasClockwiseConnection)
                            {
                                Walls.Add(new Wall(
                                    new Rectangle(x + centerOffset, y, wallThickness, (int)wallLength),
                                    true, dir, false));
                                Walls.Add(new Wall(
                                    new Rectangle((int)(x + centerOffset + externalOffset), y, wallThickness, (int)externalWallLength),
                                    true, dir, false));
                            }
                            if (hasCounterClockwiseConnection)
                            {
                                Walls.Add(new Wall(
                                    new Rectangle(x + centerOffset, (int)(y + size - wallLength), wallThickness, (int)wallLength),
                                    true, dir, false));
                                Walls.Add(new Wall(
                                    new Rectangle((int)(x + centerOffset + externalOffset), (int)(y + size - externalWallLength), wallThickness, (int)externalWallLength),
                                    true, dir, false));
                            }
                            break;
                    }
                }
            }
        }

        private void HandleThreeConnections(Room room, int x, int y, int size, int wallThickness, int corridorWidth, int centerOffset, List<int> connections)
        {
            // Find the side without connection
            int sideWithoutConnection = Enumerable.Range(0, 4)
                .First(dir => !connections.Contains(dir));

            float perpendicularWallLength = size * GameSettings.Graphics.WallGeneration.ShortWallLengthRatio;

            // Add long wall on the side without connection
            switch (sideWithoutConnection)
            {
                case GameSettings.Directions.Up:
                    Walls.Add(new Wall(
                        new Rectangle(x, y + centerOffset, size, wallThickness),
                        true, sideWithoutConnection, false));
                    break;

                case GameSettings.Directions.Right:
                    Walls.Add(new Wall(
                        new Rectangle(x + centerOffset + corridorWidth - wallThickness, y, wallThickness, size),
                        true, sideWithoutConnection, false));
                    break;

                case GameSettings.Directions.Down:
                    Walls.Add(new Wall(
                        new Rectangle(x, y + centerOffset + corridorWidth - wallThickness, size, wallThickness),
                        true, sideWithoutConnection, false));
                    break;

                case GameSettings.Directions.Left:
                    Walls.Add(new Wall(
                        new Rectangle(x + centerOffset, y, wallThickness, size),
                        true, sideWithoutConnection, false));
                    break;
            }

            // Add walls on opposite side
            int oppositeSide = (sideWithoutConnection + 2) % 4;
            float shortWallLength = size * GameSettings.Graphics.WallGeneration.ShortWallLengthRatio;
            int gap = (int)((size - 2 * shortWallLength) / 2);

            switch (oppositeSide)
            {
                case GameSettings.Directions.Up:
                    Walls.Add(new Wall(
                        new Rectangle(x, y + centerOffset, (int)shortWallLength, wallThickness),
                        true, oppositeSide, false));
                    Walls.Add(new Wall(
                        new Rectangle(x + size - (int)shortWallLength, y + centerOffset, (int)shortWallLength, wallThickness),
                        true, oppositeSide, false));
                        
                    Walls.Add(new Wall(
                        new Rectangle(x + centerOffset, y, wallThickness, (int)shortWallLength),
                        true, GameSettings.Directions.Left, false));
                    Walls.Add(new Wall(
                        new Rectangle(x + centerOffset + corridorWidth - wallThickness, y, wallThickness, (int)shortWallLength),
                        true, GameSettings.Directions.Right, false));
                    break;

                case GameSettings.Directions.Right:
                    Walls.Add(new Wall(
                        new Rectangle(x + centerOffset + corridorWidth - wallThickness, y, wallThickness, (int)shortWallLength),
                        true, oppositeSide, false));
                    Walls.Add(new Wall(
                        new Rectangle(x + centerOffset + corridorWidth - wallThickness, y + size - (int)shortWallLength, wallThickness, (int)shortWallLength),
                        true, oppositeSide, false));
                        
                    Walls.Add(new Wall(
                        new Rectangle(x + centerOffset + corridorWidth, y + centerOffset, (int)shortWallLength, wallThickness),
                        true, GameSettings.Directions.Up, false));
                    Walls.Add(new Wall(
                        new Rectangle(x + centerOffset + corridorWidth, y + centerOffset + corridorWidth - wallThickness, (int)shortWallLength, wallThickness),
                        true, GameSettings.Directions.Down, false));
                    break;

                case GameSettings.Directions.Down:
                    Walls.Add(new Wall(
                        new Rectangle(x, y + centerOffset + corridorWidth - wallThickness, (int)shortWallLength, wallThickness),
                        true, oppositeSide, false));
                    Walls.Add(new Wall(
                        new Rectangle(x + size - (int)shortWallLength, y + centerOffset + corridorWidth - wallThickness, (int)shortWallLength, wallThickness),
                        true, oppositeSide, false));
                        
                    Walls.Add(new Wall(
                        new Rectangle(x + centerOffset, y + centerOffset + corridorWidth , wallThickness, (int)shortWallLength),
                        true, GameSettings.Directions.Left, false));
                    Walls.Add(new Wall(
                        new Rectangle(x + centerOffset + corridorWidth - wallThickness, y + centerOffset + corridorWidth, wallThickness, (int)shortWallLength),
                        true, GameSettings.Directions.Right, false));
                    break;

                case GameSettings.Directions.Left:
                    Walls.Add(new Wall(
                        new Rectangle(x + centerOffset, y, wallThickness, (int)shortWallLength),
                        true, oppositeSide, false));
                    Walls.Add(new Wall(
                        new Rectangle(x + centerOffset, y + size - (int)shortWallLength, wallThickness, (int)shortWallLength),
                        true, oppositeSide, false));

                    Walls.Add(new Wall(
                        new Rectangle(x, y + centerOffset, (int)shortWallLength, wallThickness),
                        true, GameSettings.Directions.Up, false));
                    Walls.Add(new Wall(
                        new Rectangle(x, y + centerOffset + corridorWidth - wallThickness, (int)shortWallLength, wallThickness),
                        true, GameSettings.Directions.Down, false));
                    break;
            }
        }

        private void HandleFourConnections(Room room, int x, int y, int size, int wallThickness, int corridorWidth, int centerOffset)
        {
            // For each side, add two short walls
            for (int dir = 0; dir < 4; dir++)
            {
                float shortWallLength = size * GameSettings.Graphics.WallGeneration.ShortWallLengthRatio;
                int gap = (int)((size - 2 * shortWallLength) / 2);

                switch (dir)
                {
                    case GameSettings.Directions.Up:
                        Walls.Add(new Wall(
                            new Rectangle(x, y + centerOffset, (int)shortWallLength, wallThickness),
                            true, dir, false));
                        Walls.Add(new Wall(
                            new Rectangle(x + size - (int)shortWallLength, y + centerOffset, (int)shortWallLength, wallThickness),
                            true, dir, false));
                        break;
                    case GameSettings.Directions.Right:
                        Walls.Add(new Wall(
                            new Rectangle(x + centerOffset + corridorWidth - wallThickness, y, wallThickness, (int)shortWallLength),
                            true, dir, false));
                        Walls.Add(new Wall(
                            new Rectangle(x + centerOffset + corridorWidth - wallThickness, y + size - (int)shortWallLength, wallThickness, (int)shortWallLength),
                            true, dir, false));
                        break;
                    case GameSettings.Directions.Down:
                        Walls.Add(new Wall(
                            new Rectangle(x, y + centerOffset + corridorWidth - wallThickness, (int)shortWallLength, wallThickness),
                            true, dir, false));
                        Walls.Add(new Wall(
                            new Rectangle(x + size - (int)shortWallLength, y + centerOffset + corridorWidth - wallThickness, (int)shortWallLength, wallThickness),
                            true, dir, false));
                        break;
                    case GameSettings.Directions.Left:
                        Walls.Add(new Wall(
                            new Rectangle(x + centerOffset, y, wallThickness, (int)shortWallLength),
                            true, dir, false));
                        Walls.Add(new Wall(
                            new Rectangle(x + centerOffset, y + size - (int)shortWallLength, wallThickness, (int)shortWallLength),
                            true, dir, false));
                        break;
                }
            }
        }

        private void GenerateRoomWalls(Room room, Vector2 position, int size, int wallThickness, int corridorWidth, int centerOffset)
        {
            int x = (int)position.X;
            int y = (int)position.Y;

            // For each direction (up, right, down, left)
            for (int dir = 0; dir < 4; dir++)
            {
                bool hasConnection = room.HasConnection(dir);

                if (hasConnection)
                {
                    // For connected sides, create two wall segments with a gap for the corridor
                    switch (dir)
                    {
                        case GameSettings.Directions.Up:
                            // Left
                            Walls.Add(new Wall(
                                new Rectangle(x, y, centerOffset, wallThickness),
                                false, dir, false));
                            // Door
                            Walls.Add(new Wall(
                                new Rectangle(x + centerOffset, y, corridorWidth, wallThickness),
                                false, dir, false, true));
                            // Right
                            Walls.Add(new Wall(
                                new Rectangle(x + centerOffset + corridorWidth, y, size - (centerOffset + corridorWidth), wallThickness),
                                false, dir, false));
                            break;

                        case GameSettings.Directions.Right:
                            // Top
                            Walls.Add(new Wall(
                                new Rectangle(x + size - wallThickness, y, wallThickness, centerOffset),
                                false, dir, false));
                            // Door
                            Walls.Add(new Wall(
                                new Rectangle(x + size - wallThickness, y + centerOffset, wallThickness, corridorWidth),
                                false, dir, false, true));
                            // Down
                            Walls.Add(new Wall(
                                new Rectangle(x + size - wallThickness, y + centerOffset + corridorWidth, wallThickness, size - (centerOffset + corridorWidth)),
                                false, dir, false));
                            break;

                        case GameSettings.Directions.Down:
                            // Left
                            Walls.Add(new Wall(
                                new Rectangle(x, y + size - wallThickness, centerOffset, wallThickness),
                                false, dir, false));
                            // Door
                            Walls.Add(new Wall(
                                new Rectangle(x + centerOffset, y + size - wallThickness, corridorWidth, wallThickness),
                                false, dir, false, true));
                            // Right
                            Walls.Add(new Wall(
                                new Rectangle(x + centerOffset + corridorWidth, y + size - wallThickness, size - (centerOffset + corridorWidth), wallThickness),
                                false, dir, false));
                            break;

                        case GameSettings.Directions.Left:
                            // Top
                            Walls.Add(new Wall(
                                new Rectangle(x, y, wallThickness, centerOffset),
                                false, dir, false));
                            // Door
                            Walls.Add(new Wall(
                                new Rectangle(x, y + centerOffset, wallThickness, corridorWidth),
                                false, dir, false, true));
                            // Down
                            Walls.Add(new Wall(
                                new Rectangle(x, y + centerOffset + corridorWidth, wallThickness, size - (centerOffset + corridorWidth)),
                                false, dir, false));
                            break;
                    }
                }
                else
                {
                    // For unconnected sides, create a single full wall
                    switch (dir)
                    {
                        case GameSettings.Directions.Up:
                            Walls.Add(new Wall(
                                new Rectangle(x, y, size, wallThickness),
                                false, dir, false));
                            break;

                        case GameSettings.Directions.Right:
                            Walls.Add(new Wall(
                                new Rectangle(x + size - wallThickness, y, wallThickness, size),
                                false, dir, false));
                            break;

                        case GameSettings.Directions.Down:
                            Walls.Add(new Wall(
                                new Rectangle(x, y + size - wallThickness, size, wallThickness),
                                false, dir, false));
                            break;

                        case GameSettings.Directions.Left:
                            Walls.Add(new Wall(
                                new Rectangle(x, y, wallThickness, size),
                                false, dir, false));
                            break;
                    }
                }
            }
        }

        public bool CheckCollision(Rectangle playerBounds)
        {
            foreach (var wall in Walls)
            {
                if (!wall.IsPassable() && wall.Intersects(playerBounds))
                {
                    return true;
                }
            }
            return false;
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

        public void ToggleAllDoors()
        {
            _allDoorsOpen = !_allDoorsOpen;
            foreach (var wall in Walls)
            {
                if (wall.IsDoor)
                {
                    wall.ToggleDoor();
                }
            }
        }
    }
}