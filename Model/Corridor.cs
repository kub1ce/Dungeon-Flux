using Microsoft.Xna.Framework;
using System;

namespace DungeonFlux.Model
{
    public class Corridor
    {
        public Point Start { get; set; }
        public Point End { get; set; }
        public Point Size { get; set; }
        public Rectangle Bounds => new Rectangle(Start, Size);

        public Corridor(Point start, Point end)
        {
            if (start.X < 0 || start.Y < 0 || end.X < 0 || end.Y < 0)
                throw new ArgumentException("Coordinates must be non-negative");

            Start = start;
            End = end;
            Size = CalculateSize(start, end);
        }

        private Point CalculateSize(Point start, Point end)
        {
            if (start.X == end.X) // Вертикальный коридор
            {
                int height = Math.Abs(end.Y - start.Y) + 1;
                return new Point(1, height);
            }
            else // Горизонтальный коридор
            {
                int width = Math.Abs(end.X - start.X) + 1;
                return new Point(width, 1);
            }
        }

        public bool Intersects(Room room)
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room));

            return Bounds.Intersects(room.Bounds);
        }

        public bool Intersects(Corridor corridor)
        {
            if (corridor == null)
                throw new ArgumentNullException(nameof(corridor));

            return Bounds.Intersects(corridor.Bounds);
        }

        public bool IsVertical()
        {
            return Start.X == End.X;
        }

        public bool IsHorizontal()
        {
            return Start.Y == End.Y;
        }

        public int GetLength()
        {
            return IsVertical() ? Size.Y : Size.X;
        }
    }
} 