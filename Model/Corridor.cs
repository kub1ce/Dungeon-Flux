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
            Start = start;
            End = end;
            
            // Определяем размер коридора
            if (start.X == end.X) // Вертикальный коридор
            {
                Size = new Point(1, Math.Abs(end.Y - start.Y) + 1);
                if (end.Y < start.Y)
                {
                    Start = end;
                }
            }
            else // Горизонтальный коридор
            {
                Size = new Point(Math.Abs(end.X - start.X) + 1, 1);
                if (end.X < start.X)
                {
                    Start = end;
                }
            }
        }

        public bool Intersects(Room room)
        {
            return Bounds.Intersects(room.Bounds);
        }

        public bool Intersects(Corridor corridor)
        {
            return Bounds.Intersects(corridor.Bounds);
        }
    }
} 