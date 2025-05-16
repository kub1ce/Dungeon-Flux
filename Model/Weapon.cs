using System;
using Microsoft.Xna.Framework;

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
            _damage = 30; // ! временно для тестов. урон равен здорьвью врага, чтобы убить его за 1 удар
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

        public bool Attack()
        {
            if (!CanAttack)
                return false;

            _currentCooldown = _cooldown;
            _attackEffect.Start(Position, Direction);
            OnAttack?.Invoke();
            return true;
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }
    }
} 