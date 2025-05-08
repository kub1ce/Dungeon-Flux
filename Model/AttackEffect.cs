using System;
using Microsoft.Xna.Framework;

namespace DungeonFlux.Model
{
    public class AttackEffect
    {
        private Vector2 _position;
        private Vector2 _direction;
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

        public float Duration => _duration;
        public float CurrentTime => _currentTime;
        public float Range => _range;
        public bool IsActive => _isActive;
        public float Progress => _currentTime / _duration;

        public event Action<Vector2> PositionChanged;
        public event Action<Vector2> DirectionChanged;
        public event Action OnEffectEnd;

        public AttackEffect(float duration, float range)
        {
            _duration = duration;
            _range = range;
            _currentTime = 0;
            _isActive = false;
        }

        public void Start(Vector2 position, Vector2 direction)
        {
            Position = position;
            Direction = direction;
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

        public Vector2 GetEndPosition()
        {
            return Position + Direction * Range;
        }
    }
} 