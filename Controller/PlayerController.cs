using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DungeonFlux.Model;
using System;
using System.Linq;

namespace DungeonFlux.Controller
{
    public class PlayerController
    {
        private readonly Player _player;
        private readonly GameModel _gameModel;
        private readonly GraphicsDevice _graphicsDevice;
        private MouseState _previousMouseState;
        private readonly Game _game;

        public PlayerController(Player player, GameModel gameModel, GraphicsDevice graphicsDevice, Game game)
        {
            _player = player;
            _gameModel = gameModel;
            _graphicsDevice = graphicsDevice;
            _game = game;
            _previousMouseState = Mouse.GetState();
        }

        public void Update(GameTime gameTime)
        {
            // Skip input processing if window is not active
            if (!_game.IsActive)
                return;

            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();
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

            if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                var viewport = _graphicsDevice.Viewport;
                var mousePosition = new Vector2(mouseState.X, mouseState.Y);

                Vector2 mouseWorld = (mousePosition - new Vector2(viewport.Width / 2, viewport.Height / 2)) / (_gameModel.Scale * GameSettings.Graphics.RoomSize) + _gameModel.CameraPosition;

                Vector2 weaponCenter = _player.Position + new Vector2(
                    GameSettings.Player.Size / (2f * GameSettings.Graphics.RoomSize),
                    GameSettings.Player.Size / (2f * GameSettings.Graphics.RoomSize)
                );
                weaponCenter += new Vector2(
                    GameSettings.Player.Size * 0.1f / GameSettings.Graphics.RoomSize,
                    GameSettings.Player.Size * 0.15f / GameSettings.Graphics.RoomSize
                );

                var attackDirection = mouseWorld - weaponCenter;

                if (attackDirection != Vector2.Zero)
                {
                    attackDirection.Normalize();
                    _player.Attack(attackDirection);
                }
            }

            _previousMouseState = mouseState;
        }
    }
}
