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
        private Point _currentPosition;
        private int _roomCount;

        // Направления: Up, Right, Down, Left
        private readonly Point[] _directions = new[]
        {
            new Point(0, -1),  // Up
            new Point(1, 0),   // Right
            new Point(0, 1),   // Down
            new Point(-1, 0)   // Left
        }; // TODO: вынести дирекшнс в гейм сеттингс

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
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _dungeon[x, y] = null;
                }
            }

            _currentPosition = new Point(_random.Next(_width), _random.Next(_height));
            _dungeon[_currentPosition.X, _currentPosition.Y] = new Room(_currentPosition, RoomType.Start);
            _roomCount++;
            Console.WriteLine($"Start room placed at: {_currentPosition.X}, {_currentPosition.Y}");

            // Генерация комнат методом случайного блуждения
            while (_roomCount < _maxRooms)
            {
                if (!TryAddNextRoom())
                {
                    _currentPosition = GetRandomExistingRoomPosition();
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
                        _dungeon[pos.X, pos.Y] = new Room(pos, GetRandomRoomType());
                        _roomCount++;
                    }
                }
            }

            // Выход
            AddExit();
            Console.WriteLine($"Dungeon generation complete. Total rooms: {_roomCount}");

            return _dungeon;
        }

        private bool TryAddNextRoom()
        {
            int direction = _random.Next(4);
            Point nextPos = _currentPosition + _directions[direction];

            if (nextPos.X < 0 || nextPos.X >= _width || nextPos.Y < 0 || nextPos.Y >= _height)
                return false;

            if (_dungeon[nextPos.X, nextPos.Y] != null)
                return false;

            _dungeon[nextPos.X, nextPos.Y] = new Room(nextPos, GetRandomRoomType());
            _roomCount++;

            _dungeon[_currentPosition.X, _currentPosition.Y].SetConnection(direction, true);
            _dungeon[nextPos.X, nextPos.Y].SetConnection((direction + 2) % 4, true);

            _currentPosition = nextPos;
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

        private RoomType GetRandomRoomType()
        {
            // Распределение вероятностей для разных тип комнат
            double roll = _random.NextDouble();
            if (roll < 0.4) return RoomType.Empty;
            if (roll < 0.6) return RoomType.Enemy;
            if (roll < 0.75) return RoomType.Treasure;
            if (roll < 0.85) return RoomType.Shop;
            return RoomType.Boss;
        }

        private void AddExit()
        {
            Point exitPos = FindFurthestRoom();
            _dungeon[exitPos.X, exitPos.Y].Type = RoomType.Exit;
            Console.WriteLine($"Exit room placed at: {exitPos.X}, {exitPos.Y}");
        }

        private Point FindFurthestRoom()
        {
            Point startPos = new Point(0, 0);
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_dungeon[x, y]?.Type == RoomType.Start)
                    {
                        startPos = new Point(x, y);
                        break;
                    }
                }
            }

            Point furthestRoom = startPos;
            float maxDistance = 0;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_dungeon[x, y] != null)
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
    }
} 