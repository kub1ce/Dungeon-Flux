using System;
using Microsoft.Xna.Framework;

namespace DungeonFlux.Model
{
    public class Player
    {
        public Vector2 Position { get; set; }
        public int Health { get; private set; }

        public event Action<Vector2> PositionChanged;
        public event Action<int> HealthChanged;

        public Player()
        {
            Position = GameSettings.Player.DefaultStartPosition;
            Health = GameSettings.Player.MaxHealth;
        }

        public Player(Vector2 position)
        {
            Position = position;
            Health = GameSettings.Player.MaxHealth;
            PositionChanged?.Invoke(Position);
        }

        public void Move(Vector2 direction, GameTime gameTime)
        {
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                var delta = direction * GameSettings.Player.MoveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                Position += delta;
                PositionChanged?.Invoke(Position);
            }
        }

        public void TakeDamage(int amount)
        {
            Health = Math.Max(0, Health - amount);
            HealthChanged?.Invoke(Health);
        }

        public void Heal(int amount)
        {
            Health = MathHelper.Min(GameSettings.Player.MaxHealth, Health + amount);
        }
    }
}
