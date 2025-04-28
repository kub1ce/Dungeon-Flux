using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DungeonFlux.Model;

namespace DungeonFlux.Controller
{
    public class PlayerController
    {
        private readonly Player _model;
        private readonly float _moveSpeed;

        public PlayerController(Player model)
        {
            _model = model;
            _moveSpeed = GameSettings.Player.MoveSpeed;
        }

        public void HandleInput(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            var movement = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.W))
                movement.Y -= 1;
            if (keyboardState.IsKeyDown(Keys.S))
                movement.Y += 1;
            if (keyboardState.IsKeyDown(Keys.A))
                movement.X -= 1;
            if (keyboardState.IsKeyDown(Keys.D))
                movement.X += 1;

            if (movement != Vector2.Zero)
            {
                movement.Normalize();
                var newPosition = _model.Position + movement * _moveSpeed;

                // Чтоб за рамки не выходил
                newPosition.X = MathHelper.Clamp(newPosition.X, 0, GameSettings.Dungeon.Width - 1);
                newPosition.Y = MathHelper.Clamp(newPosition.Y, 0, GameSettings.Dungeon.Height - 1);

                _model.Position = newPosition;
            }
        }
    }
}
