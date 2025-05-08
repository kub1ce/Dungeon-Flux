using System;
using Microsoft.Xna.Framework;

namespace DungeonFlux.Model
{
    public class Player
    {
        private Vector2 _position;
        private int _health;
        private Weapon _weapon;

        public Vector2 Position
        {
            get => _position;
            private set
            {
                if (_position != value)
                {
                    _position = value;
                    PositionChanged?.Invoke(_position);
                    _weapon?.SetPosition(_position);
                }
            }
        }

        public int Health
        {
            get => _health;
            private set
            {
                if (_health != value)
                {
                    _health = value;
                    HealthChanged?.Invoke(_health);
                }
            }
        }

        public Weapon Weapon => _weapon;

        public event Action<Vector2> PositionChanged;
        public event Action<int> HealthChanged;

        public Player()
        {
            Position = GameSettings.Player.DefaultStartPosition;
            Health = GameSettings.Player.MaxHealth;
            InitializeWeapon();
        }

        public Player(Vector2 position)
        {
            Position = position;
            Health = GameSettings.Player.MaxHealth;
            InitializeWeapon();
        }

        private void InitializeWeapon()
        {
            _weapon = new Weapon(
                GameSettings.Weapon.Default.Cooldown,
                GameSettings.Weapon.Default.Damage,
                GameSettings.Weapon.Default.Range
            );
            _weapon.SetPosition(Position);
        }

        public void Move(Vector2 direction, GameTime gameTime)
        {
            if (direction == Vector2.Zero)
                return;

            direction.Normalize();
            var delta = direction * GameSettings.Player.MoveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += delta;
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }

        public void Update(GameTime gameTime)
        {
            _weapon?.Update(gameTime);
        }

        public bool Attack(Vector2 direction)
        {
            if (_weapon == null)
                return false;

            _weapon.Direction = direction;
            return _weapon.Attack();
        }

        public void TakeDamage(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Damage amount cannot be negative");

            UpdateHealth(-amount);
        }

        public void Heal(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Heal amount cannot be negative");

            UpdateHealth(amount);
        }

        private void UpdateHealth(int delta)
        {
            if (delta > 0)
            {
                Health = MathHelper.Min(GameSettings.Player.MaxHealth, Health + delta);
            }
            else
            {
                Health = Math.Max(0, Health + delta);
            }
        }

        public bool IsAlive()
        {
            return Health > 0;
        }

        public float GetHealthPercentage()
        {
            return (float)Health / GameSettings.Player.MaxHealth * 100;
        }
    }
}
