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
        private const float ATTACK_RANGE = 0.08f; // Дистанция атаки
        private float _attackCooldown = 0.6f; // Время между атаками
        private float _currentCooldown = 0f; // Текущее время перезарядки
        // TODO: Вынести в константы
        // TODO: Добавить анимацию атаки
        // TODO: Вынести Дистанцию приближения к врагу и дистанцию атаки (if атака < приближения, в логгах варним)
        public bool CanAttack => _currentCooldown <= 0;

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

        public void Update(GameTime gameTime, Player player)
        {
            if (_currentCooldown > 0)
            {
                _currentCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_currentCooldown < 0)
                    _currentCooldown = 0;
            } else
            {
                AttackPlayer(player);
            }
        }

        public void AttackPlayer(Player player)
        {
            if (!IsAlive || !CanAttack) return;

            float distance = Vector2.Distance(Position, player.Position);
            if (distance <= ATTACK_RANGE)
            {
                player.TakeDamage(1);
                _currentCooldown = _attackCooldown;
            }
        }

        public void MoveTowards(Player player, GameTime gameTime)
        {
            if (!IsAlive)
                return;
            
            Update(gameTime, player);

            Vector2 direction = player.Position - Position;

            if (direction.Length() < ATTACK_RANGE)
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