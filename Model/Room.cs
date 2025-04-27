using Microsoft.Xna.Framework;

namespace DungeonFlux.Model
{
    public class Room
    {
        public Point Position { get; set; }
        public RoomType Type { get; set; }
        public bool IsVisited { get; set; }
        public bool[] Connections { get; set; }

        public Room(Point position, RoomType type)
        {
            Position = position;
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
    }
} 