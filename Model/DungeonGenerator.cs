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

        public DungeonGenerator(int width, int height, int minRooms, int maxRooms)
        {
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
            Console.WriteLine($"Generating dungeon with size {_width}x{_height}, rooms: {_minRooms}-{_maxRooms}");
            
            // Очищаем карту
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _dungeon[x, y] = null;
                }
            }

            // Создаем начальную комнату
            Point startPos = new Point(_random.Next(_width), _random.Next(_height));
            _dungeon[startPos.X, startPos.Y] = new Room(startPos, new Point(1, 1), RoomType.Start);
            _roomCount++;
            Console.WriteLine($"Start room placed at: {startPos.X}, {startPos.Y}");

            // Генерация комнат методом случайного блуждания
            Point currentPos = startPos;
            while (_roomCount < _maxRooms)
            {
                if (!TryAddNextRoom(currentPos))
                {
                    currentPos = GetRandomExistingRoomPosition();
                }
            }

            if (_roomCount < _minRooms)
            {
                Console.WriteLine($"Not enough rooms ({_roomCount}), adding more to reach {_minRooms}");
                while (_roomCount < _minRooms)
                {
                    Point pos = new Point(_random.Next(_width), _random.Next(_height));
                    if (_dungeon[pos.X, pos.Y] == null)
                    {
                        _dungeon[pos.X, pos.Y] = new Room(pos, new Point(1, 1), GetRandomRoomType());
                        _roomCount++;
                    }
                }
            }

            // Назначаем конечную комнату
            Point exitPos = FindFurthestRoom(startPos);
            _dungeon[exitPos.X, exitPos.Y].Type = RoomType.Exit;
            Console.WriteLine($"Exit room placed at: {exitPos.X}, {exitPos.Y}");

            // Помечаем тупиковые комнаты
            MarkDeadEnds();
            Console.WriteLine($"Dungeon generation complete. Total rooms: {_roomCount}");
            return _dungeon;
        }

        private bool TryAddNextRoom(Point currentPos)
        {
            int direction = _random.Next(4);
            Point nextPos = currentPos + GameSettings.Directions.All[direction];

            if (nextPos.X < 0 || nextPos.X >= _width || nextPos.Y < 0 || nextPos.Y >= _height)
                return false;

            if (_dungeon[nextPos.X, nextPos.Y] != null)
                return false;

            _dungeon[nextPos.X, nextPos.Y] = new Room(nextPos, new Point(1, 1), GetRandomRoomType());
            _roomCount++;

            // Устанавливаем соединения между комнатами
            _dungeon[currentPos.X, currentPos.Y].SetConnection(direction, true);
            _dungeon[nextPos.X, nextPos.Y].SetConnection((direction + 2) % 4, true);

            return true;
        }

        private Point GetRandomExistingRoomPosition()
        {
            List<Point> existingRooms = new List<Point>();
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
            if (roll < 0.4) return RoomType.Empty;
            if (roll < 0.6) return RoomType.Enemy;
            if (roll < 0.75) return RoomType.Treasure;
            if (roll < 0.85) return RoomType.Shop;
            return RoomType.Boss;
        }

        private void MarkDeadEnds()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var room = _dungeon[x, y];
                    if (room != null && room.Type != RoomType.Start && room.Type != RoomType.Exit)
                    {
                        if (room.IsDeadEnd())
                        {
                            room.Type = RoomType.DeadEnd;
                        }
                        else
                        {
                            room.Type = RoomType.Corridor;
                        }
                    }
                }
            }
        }
    }
} 