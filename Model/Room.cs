using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DungeonFlux.Model
{
    public class Room
    {
        public Point Position { get; set; }
        public Point Size { get; set; }
        public RoomType Type { get; set; }
        public bool IsVisited { get; set; }
        public bool[] Connections { get; private set; }
        public Rectangle Bounds => new Rectangle(Position, Size);

        public Room(Point position, Point size, RoomType type)
        {
            if (position.X < 0 || position.Y < 0)
                throw new ArgumentException("Position coordinates must be non-negative");
            if (size.X <= 0 || size.Y <= 0)
                throw new ArgumentException("Size must be positive");

            Position = position;
            Size = size;
            Type = type;
            IsVisited = false;
            Connections = new bool[GameSettings.Room.ConnectionCount];
        }

        public bool HasConnection(int direction)
        {
            if (direction < 0 || direction >= Connections.Length)
                throw new ArgumentException("Invalid direction");

            return Connections[direction];
        }

        public void SetConnection(int direction, bool value)
        {
            if (direction < 0 || direction >= Connections.Length)
                throw new ArgumentException("Invalid direction");

            Connections[direction] = value;
        }

        public bool Intersects(Room other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return Bounds.Intersects(other.Bounds);
        }

        public int GetConnectionCount()
        {
            int count = 0;
            foreach (bool connection in Connections)
            {
                if (connection) count++;
            }
            return count;
        }

        public bool IsDeadEnd()
        {
            return GetConnectionCount() == 1;
        }

        public int[] GetConnectedDirections()
        {
            var directions = new List<int>();
            for (int i = 0; i < Connections.Length; i++)
            {
                if (Connections[i])
                {
                    directions.Add(i);
                }
            }
            return directions.ToArray();
        }
    }
} 