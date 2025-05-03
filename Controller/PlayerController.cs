using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DungeonFlux.Model;

namespace DungeonFlux.Controller
{
    public class PlayerController
    {
        private readonly Player _player;
        private readonly GameModel _gameModel;

        public PlayerController(Player player, GameModel gameModel)
        {
            _player = player;
            _gameModel = gameModel;
        }

        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            var direction = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
                direction.Y -= 1;
            if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
                direction.Y += 1;
            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                direction.X -= 1;
            if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                direction.X += 1;

            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                _player.Move(direction, gameTime);

                // Проверяем границы подземелья
                var position = _player.Position;
                if (position.X < 0) position.X = 0;
                if (position.Y < 0) position.Y = 0;
                if (position.X >= GameSettings.Dungeon.Size.Width) position.X = GameSettings.Dungeon.Size.Width - 1;
                if (position.Y >= GameSettings.Dungeon.Size.Height) position.Y = GameSettings.Dungeon.Size.Height - 1;

                // Обновляем позицию игрока
                _player.SetPosition(position);
            }
        }
    }
}
