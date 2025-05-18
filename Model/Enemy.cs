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
        private float _moveSpeed = GameSettings.Enemy.Movement.MoveSpeed;
        private float _attackCooldown = GameSettings.Enemy.Movement.AttackCooldown;
        private float _currentCooldown = 0f;

        private EnemyAttackEffect _attackEffect;
        public bool CanAttack => _currentCooldown <= 0;
        public EnemyAttackEffect CurrentAttackEffect => _attackEffect;

        public event Action OnAttack;

        public Enemy(Vector2 position, int health = GameSettings.Enemy.Movement.DefaultHealth)
        {
            Position = position;
            Health = health;
            _attackEffect = new EnemyAttackEffect(
                GameSettings.Enemy.AttackEffect.Duration,
                GameSettings.Enemy.Movement.AttackRange
            );
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
            }
            else
            {
                AttackPlayer(player);
            }

            _attackEffect.Update(gameTime);
        }

        public void AttackPlayer(Player player)
        {
            if (!IsAlive || !CanAttack) return;

            float distance = Vector2.Distance(Position, player.Position);
            if (distance <= GameSettings.Enemy.Movement.AttackRange)
            {
                player.TakeDamage(1);
                _currentCooldown = _attackCooldown;
                _attackEffect.Start(Position);
                OnAttack?.Invoke();
            }
        }

        public void MoveTowards(Player player, GameTime gameTime)
        {
            if (!IsAlive)
                return;
            
            Update(gameTime, player);

            Vector2 direction = player.Position - Position;

            if (direction.Length() < GameSettings.Enemy.Movement.AttackRange)
                return;

            direction.Normalize();

            var delta = direction * _moveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += delta;
        }
    }
} 