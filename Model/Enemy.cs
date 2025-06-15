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

            Vector2 direction = player.Position - Position;

            if (direction.Length() < GameSettings.Enemy.Movement.AttackRange)
                return;

            direction.Normalize();

            var delta = direction * _moveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            var newPosition = Position + delta;

            // Check collision with other enemies in the same room
            if (Room != null)
            {
                float enemySize = GameSettings.Player.Size / (float)GameSettings.Graphics.RoomSize;
                float roomX = Room.Position.X - 0.5f; // Add half-room offset
                float roomY = Room.Position.Y - 0.5f; // Add half-room offset
                float roomWidth = 1.0f; // Room size in world coordinates
                float roomHeight = 1.0f;

                // Add a small margin to prevent enemies from touching the walls
                float margin = enemySize * 0.1f;
                
                // Clamp the position to room boundaries
                newPosition.X = Math.Clamp(newPosition.X, roomX + margin, roomX + roomWidth - margin - enemySize);
                newPosition.Y = Math.Clamp(newPosition.Y, roomY + margin, roomY + roomHeight - margin - enemySize);

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
                    
                    // Clamp alternative position to room boundaries
                    alternativePosition.X = Math.Clamp(alternativePosition.X, roomX + margin, roomX + roomWidth - margin - enemySize);
                    alternativePosition.Y = Math.Clamp(alternativePosition.Y, roomY + margin, roomY + roomHeight - margin - enemySize);
                    
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
                        
                        // Clamp partial position to room boundaries
                        partialPosition.X = Math.Clamp(partialPosition.X, roomX + margin, roomX + roomWidth - margin - enemySize);
                        partialPosition.Y = Math.Clamp(partialPosition.Y, roomY + margin, roomY + roomHeight - margin - enemySize);
                        
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

                // Check if enemy is stuck
                float distanceMoved = Vector2.Distance(Position, _lastPosition);
                if (distanceMoved < GameSettings.Enemy.Movement.StuckDistance)
                {
                    _stuckTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (_stuckTime >= GameSettings.Enemy.Movement.StuckThreshold)
                    {
                        // Try to escape by moving in a random direction
                        float randomAngle = (float)(_random.NextDouble() * Math.PI * 2);
                        Vector2 escapeDirection = new Vector2(
                            (float)Math.Cos(randomAngle),
                            (float)Math.Sin(randomAngle)
                        );
                        escapeDirection.Normalize();

                        var escapePosition = Position + escapeDirection * _moveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        
                        // Clamp escape position to room boundaries
                        escapePosition.X = Math.Clamp(escapePosition.X, roomX + margin, roomX + roomWidth - margin - enemySize);
                        escapePosition.Y = Math.Clamp(escapePosition.Y, roomY + margin, roomY + roomHeight - margin - enemySize);

                        Position = escapePosition;
                        _stuckTime = 0f;
                    }
                }
                else
                {
                    _stuckTime = 0f;
                }

                _lastPosition = Position;
            }
            else
            {
                Position = newPosition;
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
            
            float minDistance = enemySize * .3f;

            return Vector2.Distance(newPosition, otherEnemy.Position) < minDistance;
        }
    }
} 