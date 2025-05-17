using Microsoft.Xna.Framework;
using System;

namespace DungeonFlux.Model
{
    public class Enemy
    {
        public Vector2 Position { get; set; }
        public int Health { get; private set; }
        public bool IsAlive => Health > 0;
        public Room Room { get; set; }
        private float _moveSpeed = 0.48f; // Скорость движения врага

        public Enemy(Vector2 position, int health = 30)
        {
            Position = position;
            Health = health;
        }

        public void TakeDamage(int amount)
        {
            if (amount < 0) return;
            Health = System.Math.Max(0, Health - amount);
        }

        public void MoveTowards(Player player, GameTime gameTime)
        {
            if (!IsAlive)
                return;

            Vector2 direction = player.Position - Position;
            
            if (direction.Length() < 0.01f)
                return;

            direction.Normalize();

            // Вычисляем новую позицию
            var delta = direction * _moveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            var newPosition = Position + delta;

            // // Проверяем, что новая позиция находится в пределах комнаты
            // var roomX = (int)Math.Round(Position.X) - 0.5f;
            // var roomY = (int)Math.Round(Position.Y) - 0.5f;

            // // Ограничиваем движение в пределах комнаты
            // newPosition.X = MathHelper.Clamp(newPosition.X, roomX, roomX);
            // newPosition.Y = MathHelper.Clamp(newPosition.Y, roomY, roomY);

            Position = newPosition;
        }
    }
} 