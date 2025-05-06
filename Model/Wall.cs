using Microsoft.Xna.Framework;

namespace DungeonFlux.Model
{
    public class Wall
    {
        public Rectangle Bounds { get; private set; }
        public bool IsCorridor { get; private set; }
        public int Direction { get; private set; }
        public bool IsConnection { get; private set; }

        public Wall(Rectangle bounds, bool isCorridor, int direction, bool isConnection)
        {
            Bounds = bounds;
            IsCorridor = isCorridor;
            Direction = direction;
            IsConnection = isConnection;
        }

        public bool Intersects(Rectangle other)
        {
            return Bounds.Intersects(other);
        }

        public bool IsPassable()
        {
            return IsConnection;
        }
    }
} 