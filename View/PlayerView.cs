using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DungeonFlux.Model;
using System;

namespace DungeonFlux.View
{
    public class PlayerView
    {
        private readonly Player _model;
        private readonly Texture2D _texture;
        private readonly GameView _gameView;

        public PlayerView(Player model, Texture2D texture, GameView gameView)
        {
            _model = model;
            _texture = texture;
            _gameView = gameView;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            try
            {
                Vector2 dungeonOffset = _gameView.GetDungeonOffset();

                Vector2 position = new Vector2(
                    _model.Position.X * GameSettings.Graphics.RoomSize + dungeonOffset.X + GameSettings.Graphics.RoomSize / 2 - GameSettings.Player.Size / 2,
                    _model.Position.Y * GameSettings.Graphics.RoomSize + dungeonOffset.Y + GameSettings.Graphics.RoomSize / 2 - GameSettings.Player.Size / 2
                );

                position.X = MathHelper.Clamp(position.X, 0, spriteBatch.GraphicsDevice.Viewport.Width - GameSettings.Player.Size);
                position.Y = MathHelper.Clamp(position.Y, 0, spriteBatch.GraphicsDevice.Viewport.Height - GameSettings.Player.Size);

                spriteBatch.Draw(_texture, 
                    new Rectangle((int)position.X, (int)position.Y, GameSettings.Player.Size, GameSettings.Player.Size), 
                    Color.White);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PlayerView.Draw: {ex}");
            }
        }
    }
}
