using Microsoft.Xna.Framework;

namespace DungeonFlux.Model
{
    public class Enemy
    {
        public Vector2 Position { get; set; }
        public int Health { get; private set; }
        public bool IsAlive => Health > 0;

        public Enemy(Vector2 position, int health = 30)
        {
            Position = position;
            Health = health;
        }

        public void TakeDamage(int amount)
        {
            if (amount < 0) return;
            Health = System.Math.Max(0, Health - amount);
        }
    }
} 