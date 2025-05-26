using System;
using Microsoft.Xna.Framework;

namespace DungeonFlux.Model
{
    public class Player
    {
        private Vector2 _position;
        private int _health;
        private int _coins;
        private Weapon _weapon;
        private GameModel _gameModel;

        public Vector2 Position
        {
            get => _position;
            private set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPositionChanged();
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
                    OnHealthChanged();
                }
            }
        }

        public int Coins
        {
            get => _coins;
            private set
            {
                if (_coins != value)
                {
                    _coins = value;
                    OnCoinsChanged();
                }
            }
        }

        public Weapon Weapon => _weapon;

        public event Action<Vector2> PositionChanged;
        public event Action<int> HealthChanged;
        public event Action<int> CoinsChanged;

        public Player()
        {
            Position = GameSettings.Player.DefaultStartPosition;
            Health = GameSettings.Player.MaxHealth;
            InitializeWeapon();
        }

        public Player(Vector2 position, GameModel gameModel)
        {
            _gameModel = gameModel;
            Position = position;
            Health = GameSettings.Player.MaxHealth;
            InitializeWeapon();
        }

        private void InitializeWeapon()
        {
            _weapon = new Weapon(
                GameSettings.Weapon.Default.Cooldown,
                GameSettings.Weapon.Default.Damage,
                GameSettings.Weapon.Default.Range,
                _gameModel
            );
            _weapon.SetPosition(Position);
        }

        private void OnPositionChanged()
        {
            PositionChanged?.Invoke(_position);
            _weapon?.SetPosition(_position);
        }

        private void OnHealthChanged()
        {
            HealthChanged?.Invoke(_health);
        }

        private void OnCoinsChanged()
        {
            CoinsChanged?.Invoke(_coins);
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
            foreach (var damage in _weapon.Attack())
            {
                if (damage)
                {
                    AddRewardForKill();
                }
            }
            return true;
        }

        private void AddRewardForKill()
        {
            int playerRoomX = (int)Math.Round(Position.X);
            int playerRoomY = (int)Math.Round(Position.Y);
            var currentRoom = _gameModel.Dungeon[playerRoomX, playerRoomY];
            
            if (currentRoom == null) return;

            if (currentRoom.SubType == RoomSubType.Boss)
            {
                AddCoins(2);
            }
            else if (currentRoom.Type == RoomType.Exit)
            {
                AddCoins(10);
            }
            else
            {
                AddCoins(1);
            }
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

        public void AddCoins(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Coins amount cannot be negative");

            UpdateCoins(amount);
        }

        public void RemoveCoins(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Coins amount cannot be negative");

            UpdateCoins(-amount);
        }

        private void UpdateCoins(int delta)
        {
            Coins = MathHelper.Clamp(Coins + delta, 0, int.MaxValue);
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
