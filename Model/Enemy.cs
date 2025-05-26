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
        private float _stuckTime = 0f;
        private Vector2 _lastPosition;
        private Random _random = new Random();

        private EnemyAttackEffect _attackEffect;
        public bool CanAttack => _currentCooldown <= 0;
        public EnemyAttackEffect CurrentAttackEffect => _attackEffect;

        public event Action OnAttack;

        public Enemy(Vector2 position, int health = GameSettings.Enemy.Movement.DefaultHealth)
        {
            Position = position;
            _lastPosition = position;
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
            UpdateCooldown(gameTime);
            if (CanAttack)
            {
                TryAttackPlayer(player);
            }
            _attackEffect.Update(gameTime);
        }

        private void UpdateCooldown(GameTime gameTime)
        {
            if (_currentCooldown > 0)
            {
                _currentCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_currentCooldown < 0)
                    _currentCooldown = 0;
            }
        }

        public void TryAttackPlayer(Player player)
        {
            if (!IsAlive) return;

            float distance = Vector2.Distance(Position, player.Position);
            if (distance <= GameSettings.Enemy.Movement.AttackRange)
            {
                PerformAttack(player);
            }
        }

        private void PerformAttack(Player player)
        {
            player.TakeDamage(1);
            _currentCooldown = _attackCooldown;
            _attackEffect.Start(Position);
            OnAttack?.Invoke();
        }

        public void MoveTowards(Player player, GameTime gameTime)
        {
            if (!IsAlive)
                return;
            
            Update(gameTime, player);

            Vector2 direction = CalculateMovementDirection(player);
            if (direction != Vector2.Zero)
            {
                MoveInDirection(direction, gameTime);
            }
        }

        private Vector2 CalculateMovementDirection(Player player)
        {
            Vector2 direction = player.Position - Position;
            if (direction.Length() < GameSettings.Enemy.Movement.AttackRange)
                return Vector2.Zero;

            direction.Normalize();
            return direction;
        }

        private void MoveInDirection(Vector2 direction, GameTime gameTime)
        {
            var delta = direction * _moveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            var newPosition = Position + delta;

            // Check if enemy is stuck
            if (Vector2.Distance(newPosition, _lastPosition) < GameSettings.Enemy.Movement.StuckDistance)
            {
                _stuckTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_stuckTime >= GameSettings.Enemy.Movement.StuckThreshold)
                {
                    // Try to move in a random direction
                    float randomAngle = (float)(_random.NextDouble() * Math.PI * 2);
                    direction = new Vector2(
                        (float)Math.Cos(randomAngle),
                        (float)Math.Sin(randomAngle)
                    );
                    delta = direction * _moveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    newPosition = Position + delta;
                    _stuckTime = 0;
                }
            }
            else
            {
                _stuckTime = 0;
            }

            Position = newPosition;
            _lastPosition = Position;
        }

        private bool WouldCollideWithEnemy(Vector2 newPosition, Enemy otherEnemy)
        {
            float enemySize = GameSettings.Player.Size / (float)GameSettings.Graphics.RoomSize;
            
            if (otherEnemy.Room != null && otherEnemy.Room.SubType == RoomSubType.Boss)
            {
                enemySize *= 1.5f;
            }
            else if (otherEnemy.Room != null && otherEnemy.Room.Type == RoomType.Exit)
            {
                enemySize *= 3f;
            }
            
            float minDistance = enemySize * 0.6f;

            return Vector2.Distance(newPosition, otherEnemy.Position) < minDistance;
        }
    }
} 