using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DungeonFlux.Model
{
    public class Weapon
    {
        private float _cooldown;
        private float _currentCooldown;
        private int _damage;
        private float _range;
        private Vector2 _position;
        private Vector2 _direction;
        private AttackEffect _attackEffect;
        private GameModel _gameModel;

        public Vector2 Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    PositionChanged?.Invoke(_position);
                }
            }
        }

        public Vector2 Direction
        {
            get => _direction;
            set
            {
                if (value != Vector2.Zero)
                {
                    _direction = Vector2.Normalize(value);
                    DirectionChanged?.Invoke(_direction);
                }
            }
        }

        public float Cooldown => _cooldown;
        public int Damage => _damage;
        public float Range => _range;
        public bool CanAttack => _currentCooldown <= 0;
        public AttackEffect CurrentAttackEffect => _attackEffect;

        public event Action<Vector2> PositionChanged;
        public event Action<Vector2> DirectionChanged;
        public event Action OnAttack;

        public Weapon(float cooldown, int damage, float range, GameModel gameModel)
        {
            _cooldown = cooldown;
            _damage = damage;
            _range = range;
            _currentCooldown = 0;
            _direction = Vector2.Zero;
            _attackEffect = new AttackEffect(
                GameSettings.Weapon.AttackEffect.Duration,
                GameSettings.Weapon.AttackEffect.Range
            );
            _gameModel = gameModel;
        }

        public void Update(GameTime gameTime)
        {
            if (_currentCooldown > 0)
            {
                _currentCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_currentCooldown < 0)
                    _currentCooldown = 0;
            }

            _attackEffect.Update(gameTime);
        }

        public IEnumerable<bool> Attack(Vector2? attackDirection = null)
        {
            if (!CanAttack)
                yield return false;

            _currentCooldown = _cooldown;
            _attackEffect.Start(Position, Direction);
            OnAttack?.Invoke();

            Vector2 direction = attackDirection ?? _direction;
            if (direction == Vector2.Zero)
                yield return false;

            var hitEnemies = GetHitEnemies(direction);
            foreach (var enemy in hitEnemies)
            {
                enemy.TakeDamage(_damage);
                if (!enemy.IsAlive)
                    yield return true;
            }

            yield return false;
        }

        private IEnumerable<Enemy> GetHitEnemies(Vector2 attackDirection)
        {
            Vector2 weaponCenter = Position + new Vector2(
                GameSettings.Player.Size / (2f * GameSettings.Graphics.RoomSize),
                GameSettings.Player.Size / (2f * GameSettings.Graphics.RoomSize)
            );
            weaponCenter += new Vector2(
                GameSettings.Player.Size * 0.1f / GameSettings.Graphics.RoomSize,
                GameSettings.Player.Size * 0.15f / GameSettings.Graphics.RoomSize
            );

            Vector2 startAttack = weaponCenter;
            Vector2 endAttack = weaponCenter + (_range / GameSettings.Graphics.RoomSize) * attackDirection;

            int roomX = (int)Math.Round(Position.X);
            int roomY = (int)Math.Round(Position.Y);
            
            var currentRoom = _gameModel.Dungeon[roomX, roomY];
            if (currentRoom == null) return Enumerable.Empty<Enemy>();

            return currentRoom.Enemies
                .Where(enemy => enemy.IsAlive && IsEnemyHit(startAttack, endAttack, enemy));
        }

        private bool IsEnemyHit(Vector2 lineStart, Vector2 lineEnd, Enemy enemy)
        {
            Vector2 enemyPos = enemy.Position;
            Vector2 enemySize = new Vector2(GameSettings.Player.Size) / GameSettings.Graphics.RoomSize;
            return LineIntersectsRect(lineStart, lineEnd, enemyPos, enemyPos + enemySize);
        }

        private bool LineIntersectsRect(Vector2 lineStart, Vector2 lineEnd, Vector2 rectMin, Vector2 rectMax)
        {
            return LineIntersectsLine(lineStart, lineEnd, rectMin, new Vector2(rectMax.X, rectMin.Y)) ||
                   LineIntersectsLine(lineStart, lineEnd, new Vector2(rectMax.X, rectMin.Y), rectMax) ||
                   LineIntersectsLine(lineStart, lineEnd, rectMax, new Vector2(rectMin.X, rectMax.Y)) ||
                   LineIntersectsLine(lineStart, lineEnd, new Vector2(rectMin.X, rectMax.Y), rectMin);
        }

        private bool LineIntersectsLine(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End)
        {
            float denominator = ((line1End.X - line1Start.X) * (line2End.Y - line2Start.Y)) - 
                               ((line1End.Y - line1Start.Y) * (line2End.X - line2Start.X));

            if (denominator == 0)
                return false;

            float ua = (((line1End.X - line1Start.X) * (line1Start.Y - line2Start.Y)) - 
                       ((line1End.Y - line1Start.Y) * (line1Start.X - line2Start.X))) / denominator;
            float ub = (((line2End.X - line2Start.X) * (line1Start.Y - line2Start.Y)) - 
                       ((line2End.Y - line2Start.Y) * (line1Start.X - line2Start.X))) / denominator;

            return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1;
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }
    }
} 