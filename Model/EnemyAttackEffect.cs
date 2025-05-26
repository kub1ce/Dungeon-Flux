using System;
using Microsoft.Xna.Framework;

namespace DungeonFlux.Model
{
    public class EnemyAttackEffect
    {
        private Vector2 _position;
        private float _duration;
        private float _currentTime;
        private float _range;
        private bool _isActive;

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

        public float Duration => _duration;
        public float CurrentTime => _currentTime;
        public float Range => _range;
        public bool IsActive => _isActive;
        public float Progress => _currentTime / _duration;

        public event Action<Vector2> PositionChanged;
        public event Action OnEffectEnd;

        public EnemyAttackEffect(float duration = GameSettings.Enemy.AttackEffect.Duration, 
                               float range = GameSettings.Enemy.Movement.AttackRange)
        {
            _duration = duration;
            _range = range;
            _currentTime = 0;
            _isActive = false;
        }

        public void Start(Vector2 position)
        {
            Position = position;
            _currentTime = 0;
            _isActive = true;
        }

        public void Update(GameTime gameTime)
        {
            if (!_isActive)
                return;

            _currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (_currentTime >= _duration)
            {
                _isActive = false;
                _currentTime = 0;
                OnEffectEnd?.Invoke();
            }
        }
    }
} 