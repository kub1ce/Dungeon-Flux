using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DungeonFlux.Model;
using System;

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
                var delta = direction * GameSettings.Player.MoveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                // Check X-axis collision
                var newPositionX = _player.Position + new Vector2(delta.X, 0);
                var playerBoundsX = new Rectangle(
                    (int)(newPositionX.X * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2),
                    (int)(_player.Position.Y * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2),
                    GameSettings.Player.Size,
                    GameSettings.Player.Size
                );

                // Check Y-axis collision
                var newPositionY = _player.Position + new Vector2(0, delta.Y);
                var playerBoundsY = new Rectangle(
                    (int)(_player.Position.X * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2),
                    (int)(newPositionY.Y * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2),
                    GameSettings.Player.Size,
                    GameSettings.Player.Size
                );

                var finalDirection = Vector2.Zero;
                bool hasCollisionX = _gameModel.CheckCollision(playerBoundsX);
                bool hasCollisionY = _gameModel.CheckCollision(playerBoundsY);

                if (!hasCollisionX)
                    finalDirection.X = direction.X;

                if (!hasCollisionY)
                    finalDirection.Y = direction.Y;

                // Move only if there's valid movement in any direction
                if (finalDirection != Vector2.Zero)
                {
                    finalDirection.Normalize();
                    _player.Move(finalDirection, gameTime);
                }
            }
        }
    }
}
