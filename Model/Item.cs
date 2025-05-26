using Microsoft.Xna.Framework;
using System;

namespace DungeonFlux.Model
{
    public abstract class Item
    {
        public Vector2 Position { get; set; }
        public Rectangle Bounds { get; protected set; }
        public bool IsCollected { get; private set; }
        public Room Room { get; set; }

        protected Item(Vector2 position)
        {
            Position = position;
            IsCollected = false;
            UpdateBounds();
        }

        protected virtual void UpdateBounds()
        {
            // Размер предмета в пикселях
            int itemSize = 64;
            // Центр предмета в пикселях
            float centerX = (Position.X + 0.5f) * GameSettings.Graphics.RoomSize;
            float centerY = (Position.Y + 0.5f) * GameSettings.Graphics.RoomSize;
            Bounds = new Rectangle(
                (int)(centerX - itemSize / 2),
                (int)(centerY - itemSize / 2),
                itemSize,
                itemSize
            );
        }

        public virtual void Collect(Player player)
        {
            if (!IsCollected)
            {
                IsCollected = true;
                OnCollect(player);
            }
        }

        protected abstract void OnCollect(Player player);
    }

    public class Coin : Item
    {
        public Coin(Vector2 position) : base(position) { }

        protected override void OnCollect(Player player)
        {
            player.AddCoins(1);
        }
    }

    public class HealthPotion : Item
    {
        private const int HEAL_AMOUNT = 30;

        public HealthPotion(Vector2 position) : base(position) { }

        protected override void OnCollect(Player player)
        {
            player.Heal(HEAL_AMOUNT);
        }
    }
} 