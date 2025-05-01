using Microsoft.Xna.Framework;

namespace DungeonFlux.Model
{
    public class Room
    {
        public Point Position { get; set; }
        public Point Size { get; set; }
        public RoomType Type { get; set; }
        public bool IsVisited { get; set; }
        public bool[] Connections { get; set; }
        public Rectangle Bounds => new Rectangle(Position, Size);

        public Room(Point position, Point size, RoomType type)
        {
            Position = position;
            Size = size;
            Type = type;
            IsVisited = false;
            Connections = new bool[GameSettings.Room.ConnectionCount];
        }

        public bool HasConnection(int direction)
        {
            return Connections[direction];
        }

        public void SetConnection(int direction, bool value)
        {
            Connections[direction] = value;
        }

        public bool Intersects(Room other)
        {
            return Bounds.Intersects(other.Bounds);
        }

        public int ConnectionCount()
        {
            int count = 0;
            for (int i = 0; i < Connections.Length; i++)
            {
                if (Connections[i]) count++;
            }
            return count;
        }

        public bool IsDeadEnd()
        {
            return ConnectionCount() == 1;
        }
    }
} 