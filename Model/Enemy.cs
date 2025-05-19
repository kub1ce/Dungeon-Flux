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
        private const float STUCK_THRESHOLD = 0.5f; // Time in seconds before trying to escape stuck
        private Random _random = new Random();

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
            var newPosition = Position + delta;

            // Check collision with other enemies in the same room
            if (Room != null)
            {
                bool isBlocked = false;
                Vector2 alternativeDirection = Vector2.Zero;

                foreach (var otherEnemy in Room.Enemies)
                {
                    if (otherEnemy != this && otherEnemy.IsAlive && WouldCollideWithEnemy(newPosition, otherEnemy))
                    {
                        isBlocked = true;
                        
                        Vector2 toOther = otherEnemy.Position - Position;
                        alternativeDirection = new Vector2(-toOther.Y, toOther.X);
                        alternativeDirection.Normalize();
                        break;
                    }
                }

                if (isBlocked)
                {
                    // Try to move in the alternative direction
                    var alternativePosition = Position + alternativeDirection * _moveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    
                    bool alternativeValid = true;
                    foreach (var otherEnemy in Room.Enemies)
                    {
                        if (otherEnemy != this && otherEnemy.IsAlive && WouldCollideWithEnemy(alternativePosition, otherEnemy))
                        {
                            alternativeValid = false;
                            break;
                        }
                    }

                    if (alternativeValid)
                    {
                        Position = alternativePosition;
                    }
                    else
                    {
                        var partialDelta = delta * 0.5f;
                        var partialPosition = Position + partialDelta;
                        
                        bool partialValid = true;
                        foreach (var otherEnemy in Room.Enemies)
                        {
                            if (otherEnemy != this && otherEnemy.IsAlive && WouldCollideWithEnemy(partialPosition, otherEnemy))
                            {
                                partialValid = false;
                                break;
                            }
                        }

                        if (partialValid)
                        {
                            Position = partialPosition;
                        }
                    }
                }
                else
                {
                    Position = newPosition;
                }
            }
            else
            {
                Position = newPosition;
            }
        }

        private bool WouldCollideWithEnemy(Vector2 newPosition, Enemy otherEnemy)
        {
            float enemySize = GameSettings.Player.Size / (float)GameSettings.Graphics.RoomSize;
            float minDistance = enemySize * 0.6f;

            return Vector2.Distance(newPosition, otherEnemy.Position) < minDistance;
        }
    }
} 