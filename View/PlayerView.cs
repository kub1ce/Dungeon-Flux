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
                Vector2 cameraPosition = _gameView.CameraPosition;
                float scale = _gameView.Scale;
                int screenWidth = spriteBatch.GraphicsDevice.Viewport.Width;
                int screenHeight = spriteBatch.GraphicsDevice.Viewport.Height;

                Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);

                Vector2 playerWorld = new Vector2(_model.Position.X * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2,
                                                  _model.Position.Y * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2);

                Vector2 position = (playerWorld - cameraPosition * GameSettings.Graphics.RoomSize - new Vector2(GameSettings.Graphics.RoomSize / 2)) * scale + screenCenter;

                int playerSize = (int)(GameSettings.Player.Size * scale);

                spriteBatch.Draw(_texture, 
                    new Rectangle((int)position.X, (int)position.Y, playerSize, playerSize), 
                    Color.White);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PlayerView.Draw: {ex}");
            }
        }
    }
}
