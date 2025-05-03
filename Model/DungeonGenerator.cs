using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DungeonFlux.Model
{
    public class DungeonGenerator
    {
        private readonly Random _random;
        private readonly int _width;
        private readonly int _height;
        private readonly int _minRooms;
        private readonly int _maxRooms;
        private Room[,] _dungeon;
        private int _roomCount;

        /// <param name="width">Ширина подземелья в комнатах</param>
        /// <param name="height">Высота подземелья в комнатах</param>
        /// <param name="minRooms">Минимальное количество комнат</param>
        /// <param name="maxRooms">Максимальное количество комнат</param>
        public DungeonGenerator(int width, int height, int minRooms, int maxRooms)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Width and height must be positive");
            if (minRooms <= 0 || maxRooms <= 0)
                throw new ArgumentException("Room counts must be positive");
            if (minRooms > maxRooms)
                throw new ArgumentException("Min rooms cannot be greater than max rooms");
            if (minRooms > width * height)
                throw new ArgumentException("Min rooms cannot be greater than total possible rooms");

            _random = new Random();
            _width = width;
            _height = height;
            _minRooms = minRooms;
            _maxRooms = maxRooms;
            _dungeon = new Room[width, height];
            _roomCount = 0;
        }

        public Room[,] Generate()
        {
            Logger.Log($"Generating dungeon with size {_width}x{_height}, rooms: {_minRooms}-{_maxRooms}");
            
            ClearDungeon();
            GenerateStartRoom();
            GenerateMainPath();
            EnsureMinimumRooms();
            PlaceExitRoom();
            MarkRoomTypes();

            Logger.Log($"Dungeon generation complete. Total rooms: {_roomCount}");
            return _dungeon;
        }

        private void ClearDungeon()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _dungeon[x, y] = null;
                }
            }
            _roomCount = 0;
        }

        private void GenerateStartRoom()
        {
            Point startPos = new Point(_random.Next(_width), _random.Next(_height));
            _dungeon[startPos.X, startPos.Y] = new Room(startPos, GameSettings.Dungeon.RoomSize.Default, RoomType.Start);
            _roomCount++;
            Logger.Log($"Start room placed at: {startPos.X}, {startPos.Y}");
        }

        // Генерация комнат методом случайного блуждания
        private void GenerateMainPath()
        {
            Point currentPos = GetRandomExistingRoomPosition();
            while (_roomCount < _maxRooms)
            {
                if (!TryAddNextRoom(currentPos))
                {
                    currentPos = GetRandomExistingRoomPosition();
                }
            }
        }

        private void EnsureMinimumRooms()
        {
            if (_roomCount < _minRooms)
            {
                Logger.Log($"Not enough rooms ({_roomCount}), adding more to reach {_minRooms}");
                while (_roomCount < _minRooms)
                {
                    Point pos = new Point(_random.Next(_width), _random.Next(_height));
                    if (_dungeon[pos.X, pos.Y] == null)
                    {
                        _dungeon[pos.X, pos.Y] = new Room(pos, GameSettings.Dungeon.RoomSize.Default, GetRandomRoomType());
                        _roomCount++;
                    }
                }
            }
        }

        private void PlaceExitRoom()
        {
            Point exitPos = FindFurthestRoom(GetStartRoomPosition());
            _dungeon[exitPos.X, exitPos.Y].Type = RoomType.Exit;
            Logger.Log($"Exit room placed at: {exitPos.X}, {exitPos.Y}");
        }

        private void MarkRoomTypes()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var room = _dungeon[x, y];
                    if (room != null && room.Type != RoomType.Start && room.Type != RoomType.Exit)
                    {
                        room.Type = room.IsDeadEnd() ? RoomType.DeadEnd : RoomType.Corridor;
                    }
                }
            }
        }

        private bool TryAddNextRoom(Point currentPos)
        {
            int direction = _random.Next(4);
            Point nextPos = currentPos + GameSettings.Directions.All[direction];

            if (!IsValidPosition(nextPos) || _dungeon[nextPos.X, nextPos.Y] != null)
                return false;

            _dungeon[nextPos.X, nextPos.Y] = new Room(nextPos, GameSettings.Dungeon.RoomSize.Default, GetRandomRoomType());
            _roomCount++;

            ConnectRooms(currentPos, nextPos, direction);
            return true;
        }

        private bool IsValidPosition(Point pos)
        {
            return pos.X >= 0 && pos.X < _width && pos.Y >= 0 && pos.Y < _height;
        }

        private void ConnectRooms(Point room1, Point room2, int direction)
        {
            _dungeon[room1.X, room1.Y].SetConnection(direction, true);
            _dungeon[room2.X, room2.Y].SetConnection((direction + 2) % 4, true);
        }

        private Point GetRandomExistingRoomPosition()
        {
            var existingRooms = new List<Point>();
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_dungeon[x, y] != null)
                    {
                        existingRooms.Add(new Point(x, y));
                    }
                }
            }
            return existingRooms[_random.Next(existingRooms.Count)];
        }

        private Point GetStartRoomPosition()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_dungeon[x, y]?.Type == RoomType.Start)
                    {
                        return new Point(x, y);
                    }
                }
            }
            throw new InvalidOperationException("Start room not found");
        }

        private Point FindFurthestRoom(Point startPos)
        {
            Point furthestRoom = startPos;
            float maxDistance = 0;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_dungeon[x, y] != null && (x != startPos.X || y != startPos.Y))
                    {
                        float distance = Vector2.Distance(
                            new Vector2(startPos.X, startPos.Y),
                            new Vector2(x, y)
                        );
                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                            furthestRoom = new Point(x, y);
                        }
                    }
                }
            }

            return furthestRoom;
        }

        private RoomType GetRandomRoomType()
        {
            double roll = _random.NextDouble();
            double cumulative = 0;

            if (roll < (cumulative += GameSettings.Dungeon.RoomProbabilities.Empty)) return RoomType.Empty;
            if (roll < (cumulative += GameSettings.Dungeon.RoomProbabilities.Enemy)) return RoomType.Enemy;
            if (roll < (cumulative += GameSettings.Dungeon.RoomProbabilities.Treasure)) return RoomType.Treasure;
            if (roll < (cumulative += GameSettings.Dungeon.RoomProbabilities.Shop)) return RoomType.Shop;
            return RoomType.Boss;
        }
    }
} 