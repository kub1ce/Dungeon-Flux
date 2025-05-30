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
        private Player _player;

        private readonly DungeonGenerator _dungeonGenerator;
        private Vector2 _cameraPosition;
        private float _scale;

        public Vector2 CameraPosition => _cameraPosition;
        public float Scale => _scale;
        private readonly Random _random;
        private Room _previousPlayerRoom;

        public GameModel()
        {
            _dungeonGenerator = new DungeonGenerator(
                GameSettings.Dungeon.Size.Width,
                GameSettings.Dungeon.Size.Height,
                GameSettings.Dungeon.RoomCount.Min,
                GameSettings.Dungeon.RoomCount.Max
            );
            Walls = new List<Wall>();
            _cameraPosition = Vector2.Zero;
            _scale = GameSettings.Graphics.Scale;
            _random = new Random();
            GenerateNewDungeon();
        }

        public void SetCameraPosition(Vector2 position)
        {
            _cameraPosition = position;
        }

        public void SetScale(float scale)
        {
            _scale = scale;
        }

        public void SetPlayer(Player player)
        {
            _player = player;
        }

        public void GenerateNewDungeon()
        {
            Dungeon = _dungeonGenerator.Generate();
            Logger.Log($"Dungeon generated with size: {Dungeon.GetLength(0)}x{Dungeon.GetLength(1)}");

            SetPlayerStartPosition();
            GenerateWalls();
            IsGameOver = false;

            // Add enemies and items to rooms
            for (int x = 0; x < Dungeon.GetLength(0); x++)
            {
                for (int y = 0; y < Dungeon.GetLength(1); y++)
                {
                    var room = Dungeon[x, y];
                    if (room == null) continue;

                    if (room.Type == RoomType.DeadEnd)
                    {
                        switch (room.SubType)
                        {
                            case RoomSubType.Enemy:
                                SpawnEnemiesInRoom(room, x, y);
                                break;
                            case RoomSubType.Boss:
                                SpawnBossInRoom(room, x, y);
                                break;
                            case RoomSubType.Treasure:
                                SpawnItemsInTreasureRoom(room, x, y);
                                break;
                        }
                    }
                    else if (room.Type == RoomType.Exit)
                    {
                        SpawnExitBoss(room, x, y);
                    }
                }
            }
        }

        private void SpawnEnemiesInRoom(Room room, int x, int y)
        {
            int enemyCount = _random.Next(GameSettings.Enemy.Spawn.MinEnemiesPerRoom, GameSettings.Enemy.Spawn.MaxEnemiesPerRoom + 1);
            
            for (int i = 0; i < enemyCount; i++)
            {
                Vector2 roomCenter = new Vector2(x, y);
                float randomOffsetX = (float)(_random.NextDouble() * 2 - 1);
                float randomOffsetY = (float)(_random.NextDouble() * 2 - 1);
                
                float randomX = roomCenter.X + Math.Clamp(randomOffsetX, -GameSettings.Enemy.Spawn.SpawnRadiusRatio, GameSettings.Enemy.Spawn.SpawnRadiusRatio);
                float randomY = roomCenter.Y + Math.Clamp(randomOffsetY, -GameSettings.Enemy.Spawn.SpawnRadiusRatio, GameSettings.Enemy.Spawn.SpawnRadiusRatio);
                
                if (!IsPositionOccupiedByEnemy(room, randomX, randomY))
                {
                    var enemy = new Enemy(new Vector2(randomX, randomY));
                    enemy.Room = room;
                    room.Enemies.Add(enemy);
                }
            }
        }

        private bool IsPositionOccupiedByEnemy(Room room, float x, float y)
        {
            foreach (var existingEnemy in room.Enemies)
            {
                float enemySize = GameSettings.Player.Size / (float)GameSettings.Graphics.RoomSize;
                float minDistance = enemySize * 0.6f;
                if (Vector2.Distance(new Vector2(x, y), existingEnemy.Position) < minDistance)
                {
                    return true;
                }
            }
            return false;
        }

        private void SpawnBossInRoom(Room room, int x, int y)
        {
            Vector2 roomCenter = new Vector2(x, y);
            var boss = new Enemy(roomCenter, GameSettings.Enemy.Movement.DefaultHealth * 2);
            boss.Room = room;
            room.Enemies.Add(boss);
        }

        private void SpawnExitBoss(Room room, int x, int y)
        {
            Vector2 roomCenter = new Vector2(x, y);
            var boss = new Enemy(roomCenter, GameSettings.Enemy.Movement.DefaultHealth * 5);
            boss.Room = room;
            room.Enemies.Add(boss);
        }

        private void SpawnItemsInTreasureRoom(Room room, int x, int y)
        {
            int itemCount = _random.Next(3, 6);
            for (int i = 0; i < itemCount; i++)
            {
                Vector2 roomCenter = new Vector2(x, y);
                float randomOffsetX = (float)(_random.NextDouble() * 2 - 1) * 0.3f;
                float randomOffsetY = (float)(_random.NextDouble() * 2 - 1) * 0.3f;
                
                float randomX = roomCenter.X + randomOffsetX;
                float randomY = roomCenter.Y + randomOffsetY;
                
                Item item = _random.NextDouble() < 0.9f ? 
                    new Coin(new Vector2(randomX, randomY)) : 
                    new HealthPotion(new Vector2(randomX, randomY));
                
                item.Room = room;
                room.Items.Add(item);
            }
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
                Logger.Log("Player reached the exit room.");
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

        public void Update(GameTime gameTime)
        {
            try
            {
                UpdateEnemies(gameTime);
                UpdateItems();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Critical error in GameModel.Update: {ex.Message}");
                Logger.LogError($"Stack trace: {ex.StackTrace}");
                Logger.LogError($"Inner exception: {ex.InnerException?.Message}");
                Logger.LogError($"Inner stack trace: {ex.InnerException?.StackTrace}");
            }
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            var currentRoom = GetCurrentRoom();
            if (currentRoom == null) return;

            UpdateDoorStates(currentRoom);
            UpdateEnemiesInRoom(currentRoom, gameTime);
        }

        private void UpdateDoorStates(Room currentRoom)
        {
            if (currentRoom == null) return;

            bool hasAliveEnemies = currentRoom.Enemies.Any(e => e.IsAlive);
            bool isPlayerFullyInside = IsPlayerFullyInsideRoom(currentRoom);
            bool hasChangedRoom = _previousPlayerRoom != currentRoom;

            UpdateDoorsBasedOnState(hasAliveEnemies, isPlayerFullyInside, hasChangedRoom, currentRoom);
            _previousPlayerRoom = currentRoom;
        }

        private void UpdateDoorsBasedOnState(bool hasAliveEnemies, bool isPlayerFullyInside, bool hasChangedRoom, Room currentRoom)
        {
            if (hasAliveEnemies && isPlayerFullyInside)
            {
                CloseAllDoors();
            }
            else if (hasAliveEnemies && !isPlayerFullyInside && hasChangedRoom)
            {
                PushPlayerIntoRoom(currentRoom);
                CloseAllDoors();
            }
            else
            {
                OpenAllDoors();
            }
        }

        private void CloseAllDoors()
        {
            foreach (var wall in Walls)
            {
                if (wall.IsDoor)
                {
                    wall.IsOpen = false;
                }
            }
        }

        private void OpenAllDoors()
        {
            foreach (var wall in Walls)
            {
                if (wall.IsDoor)
                {
                    wall.IsOpen = true;
                }
            }
        }

        private void PushPlayerIntoRoom(Room room)
        {
            try
            {
                if (room == null)
                {
                    Logger.LogError("PushPlayerIntoRoom: room is null");
                    return;
                }

                if (_player == null)
                {
                    Logger.LogError("PushPlayerIntoRoom: player is null");
                    return;
                }

                // Вычисляем вектор от игрока к центру комнаты
                float dirX = room.Position.X - _player.Position.X;
                float dirY = room.Position.Y - _player.Position.Y;

                float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);
                if (length < 0.001f)
                {
                    dirX = 1.0f;
                    dirY = 0.0f;
                    length = 1.0f;
                }

                dirX /= length;
                dirY /= length;

                // Определяем, по какой оси больше отклонение
                float newX = _player.Position.X + GameSettings.Player.Size / 2 / GameSettings.Graphics.RoomSize;
                float newY = _player.Position.Y + GameSettings.Player.Size / 2 / GameSettings.Graphics.RoomSize;
                
                // Минимальное расстояние для выталкивания, чтобы игрок не застрял в двери
                float minPushDistance = GameSettings.Player.Size * 0.8f / GameSettings.Graphics.RoomSize;
                float pushDistance = Math.Max(GameSettings.Player.Size * 1.5f / GameSettings.Graphics.RoomSize, minPushDistance);

                if (Math.Abs(dirX) > Math.Abs(dirY))
                {
                    // Двигаемся по X
                    newX += dirX * pushDistance;
                }
                else
                {
                    // Двигаемся по Y
                    newY += dirY * pushDistance;
                }
                
                _player.SetPosition(new Vector2(newX, newY));
            }
            catch (Exception e)
            {
                Logger.LogError($"PushPlayerIntoRoom error: {e.Message}\n{e.StackTrace}");
            }
        }

        private bool IsPlayerFullyInsideRoom(Room room)
        {
            float roomX = (room.Position.X - 0.5f) * GameSettings.Graphics.RoomSize;
            float roomY = (room.Position.Y - 0.5f) * GameSettings.Graphics.RoomSize;

            float playerLeft = _player.Position.X * GameSettings.Graphics.RoomSize;
            float playerRight = playerLeft + GameSettings.Player.Size;
            float playerTop = _player.Position.Y * GameSettings.Graphics.RoomSize;
            float playerBottom = playerTop + GameSettings.Player.Size;

            float margin = GameSettings.Player.Size * 0.2f;
            
            return playerLeft - margin >= roomX &&
                   playerRight + margin <= roomX + GameSettings.Graphics.RoomSize &&
                   playerTop - margin >= roomY &&
                   playerBottom + margin <= roomY + GameSettings.Graphics.RoomSize;
        }

        private void UpdateItems()
        {
            if (_player == null) return;

            var currentRoom = GetCurrentRoom();
            if (currentRoom == null) return;

            var playerBounds = GetPlayerBounds();
            CollectItemsInRoom(currentRoom, playerBounds);
        }

        private Room GetCurrentRoom()
        {
            int playerRoomX = (int)Math.Round(_player.Position.X);
            int playerRoomY = (int)Math.Round(_player.Position.Y);
            return Dungeon[playerRoomX, playerRoomY];
        }

        private Rectangle GetPlayerBounds()
        {
            float playerX = _player.Position.X * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2f;
            float playerY = _player.Position.Y * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2f;
            int playerSize = GameSettings.Player.Size;
            return new Rectangle(
                (int)(playerX),
                (int)(playerY),
                playerSize,
                playerSize
            );
        }

        private void CollectItemsInRoom(Room room, Rectangle playerBounds)
        {
            for (int i = room.Items.Count - 1; i >= 0; i--)
            {
                var item = room.Items[i];
                if (!item.IsCollected && item.Bounds.Intersects(playerBounds))
                {
                    item.Collect(_player);
                    room.Items.RemoveAt(i);
                }
            }
        }

        private void UpdateEnemiesInRoom(Room room, GameTime gameTime)
        {
            foreach (var enemy in room.Enemies)
            {
                enemy.MoveTowards(_player, gameTime);
            }
        }

        public void SetGameOver(bool value)
        {
            IsGameOver = value;
        }
    }
}