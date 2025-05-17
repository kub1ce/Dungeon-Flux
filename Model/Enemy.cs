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
        private float _moveSpeed = 0.5f; // Скорость движения врага

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
            if (!IsAlive) return;

            // Вычисляем направление к игроку
            Vector2 direction = player.Position - Position;
            
            // Если расстояние слишком маленькое, не двигаемся
            if (direction.Length() < 0.01f) return;

            // Нормализуем вектор направления
            direction.Normalize();

            // Вычисляем новую позицию
            var delta = direction * _moveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            var newPosition = Position + delta;

            // Проверяем, что новая позиция находится в пределах комнаты
            int roomX = (int)Math.Floor(Position.X);
            int roomY = (int)Math.Floor(Position.Y);
            
            // Ограничиваем движение в пределах комнаты (с небольшим отступом от стен)
            newPosition.X = MathHelper.Clamp(newPosition.X, roomX + 0.1f, roomX + 0.9f);
            newPosition.Y = MathHelper.Clamp(newPosition.Y, roomY + 0.1f, roomY + 0.9f);

            Position = newPosition;
        }
    }
} 