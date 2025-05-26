using Microsoft.Xna.Framework;

namespace DungeonFlux.Model
{
    public class Wall
    {
        public Rectangle Bounds { get; private set; }
        public bool IsCorridor { get; private set; }
        public int Direction { get; private set; }
        public bool IsConnection { get; private set; }
        public bool IsDoor { get; private set; }
        public bool IsOpen { get; set; }

        public Wall(Rectangle bounds, bool isCorridor, int direction, bool isConnection, bool isDoor = false)
        {
            Bounds = bounds;
            IsCorridor = isCorridor;
            Direction = direction;
            IsConnection = isConnection;
            IsDoor = isDoor;
            IsOpen = isDoor;
        }

        public bool Intersects(Rectangle other)
        {
            return Bounds.Intersects(other);
        }

        public bool IsPassable()
        {
            return IsConnection || (IsDoor && IsOpen);
        }

        public void ToggleDoor()
        {
            if (IsDoor)
            {
                IsOpen = !IsOpen;
            }
        }
    }
} 