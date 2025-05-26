using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace DungeonFlux.View
{
    public class GameOverView
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;
        private readonly Texture2D _buttonTexture;
        private readonly Game _game;
        private Rectangle _mainMenuButton;
        private bool _isMainMenuHovered;
        private int _coins;
        private bool _wasMousePressed;

        public GameOverView(SpriteBatch spriteBatch, SpriteFont font, Game game)
        {
            _spriteBatch = spriteBatch;
            _font = font;
            _game = game;

            _buttonTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _buttonTexture.SetData(new[] { Color.White });

            UpdateButtonPositions();
        }

        private void UpdateButtonPositions()
        {
            int screenWidth = _spriteBatch.GraphicsDevice.Viewport.Width;
            int screenHeight = _spriteBatch.GraphicsDevice.Viewport.Height;

            string mainMenuText = "Main Menu";
            Vector2 mainMenuSize = _font.MeasureString(mainMenuText);

            int buttonWidth = (int)mainMenuSize.X + 40;
            int buttonHeight = (int)mainMenuSize.Y + 20;

            _mainMenuButton = new Rectangle(
                (screenWidth - buttonWidth) / 2,
                screenHeight / 2,
                buttonWidth,
                buttonHeight
            );
        }

        public void Update(int coins)
        {
            _coins = coins;
            var mouseState = Mouse.GetState();
            var mousePoint = new Point(mouseState.X, mouseState.Y);

            _isMainMenuHovered = _mainMenuButton.Contains(mousePoint);

            if (mouseState.LeftButton == ButtonState.Pressed && !_wasMousePressed)
            {
                if (_isMainMenuHovered)
                {
                    ((DungeonFluxGame)_game).ReturnToMenu();
                }
            }

            _wasMousePressed = mouseState.LeftButton == ButtonState.Pressed;
        }

        public void Draw()
        {
            int screenWidth = _spriteBatch.GraphicsDevice.Viewport.Width;
            int screenHeight = _spriteBatch.GraphicsDevice.Viewport.Height;

            _spriteBatch.Draw(_buttonTexture, 
                new Rectangle(0, 0, screenWidth, screenHeight), 
                Color.Black * 0.8f);

            string gameOverText = "Game Over";
            Vector2 gameOverSize = _font.MeasureString(gameOverText);
            _spriteBatch.DrawString(_font, gameOverText,
                new Vector2(
                    (screenWidth - gameOverSize.X) / 2,
                    screenHeight / 4
                ),
                Color.Red);

            string coinsText = $"Coins collected: {_coins}";
            Vector2 coinsSize = _font.MeasureString(coinsText);
            _spriteBatch.DrawString(_font, coinsText,
                new Vector2(
                    (screenWidth - coinsSize.X) / 2,
                    screenHeight / 4 + gameOverSize.Y + 20
                ),
                Color.Gold);

            DrawButton("Main Menu", _mainMenuButton, _isMainMenuHovered);
        }

        private void DrawButton(string text, Rectangle bounds, bool isHovered)
        {
            _spriteBatch.Draw(_buttonTexture, bounds, 
                isHovered ? Color.DarkGray : Color.Gray);

            int borderThickness = 2;
            _spriteBatch.Draw(_buttonTexture, 
                new Rectangle(bounds.X, bounds.Y, bounds.Width, borderThickness), 
                Color.White);
            _spriteBatch.Draw(_buttonTexture, 
                new Rectangle(bounds.X, bounds.Bottom - borderThickness, bounds.Width, borderThickness), 
                Color.White);
            _spriteBatch.Draw(_buttonTexture, 
                new Rectangle(bounds.X, bounds.Y, borderThickness, bounds.Height), 
                Color.White);
            _spriteBatch.Draw(_buttonTexture, 
                new Rectangle(bounds.Right - borderThickness, bounds.Y, borderThickness, bounds.Height), 
                Color.White);

            Vector2 textSize = _font.MeasureString(text);
            _spriteBatch.DrawString(_font, text,
                new Vector2(
                    bounds.X + (bounds.Width - textSize.X) / 2,
                    bounds.Y + (bounds.Height - textSize.Y) / 2
                ),
                isHovered ? Color.White : Color.LightGray);
        }
    }
} 