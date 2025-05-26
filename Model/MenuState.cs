using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DungeonFlux.Model
{
    public class MenuState
    {
        private readonly List<Button> _buttons;
        private readonly SpriteFont _font;
        private readonly string _authorText;
        private readonly Vector2 _authorPosition;

        public MenuState(SpriteFont font)
        {
            try
            {
                Logger.Log("Initializing MenuState...");
                _font = font;
                _buttons = new List<Button>();
                
                // Создаем кнопки меню
                var buttonSize = new Vector2(GameSettings.Menu.ButtonWidth, GameSettings.Menu.ButtonHeight);
                var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                Logger.Log($"Current display mode: {displayMode.Width}x{displayMode.Height}");
                
                var startButtonPosition = new Vector2(displayMode.Width / 2 - GameSettings.Menu.ButtonWidth / 2, GameSettings.Menu.StartButtonY);
                Logger.Log($"Start button position: {startButtonPosition}");
                
                _buttons.Add(new Button("Start Game", startButtonPosition, buttonSize));
                Logger.Log("Added Start Game button");
                
                // _buttons.Add(new Button("Settings", startButtonPosition + new Vector2(0, GameSettings.Menu.ButtonSpacing), buttonSize)); // TODO реализовать настройки (не удалять)
                _buttons.Add(new Button("Exit", startButtonPosition + new Vector2(0, GameSettings.Menu.ButtonSpacing), buttonSize));
                Logger.Log("Added Exit button");

                // Устанавливаем позицию текста автора
                _authorText = GameSettings.Menu.AuthorName;
                var screenWidth = displayMode.Width;
                var screenHeight = displayMode.Height;
                var textSize = _font.MeasureString(_authorText);
                _authorPosition = new Vector2(screenWidth - textSize.X - GameSettings.Menu.AuthorMargin, screenHeight - textSize.Y - GameSettings.Menu.AuthorMargin);
                Logger.Log($"Author text position: {_authorPosition}");
                
                Logger.Log("MenuState initialization completed successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error during MenuState initialization", ex);
                throw;
            }
        }

        public void Update(MouseState currentMouseState, MouseState previousMouseState, GameTime gameTime)
        {
            foreach (var button in _buttons)
            {
                button.Update(currentMouseState, previousMouseState, gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var button in _buttons)
            {
                button.Draw(spriteBatch, _font);
            }

            spriteBatch.DrawString(_font, _authorText, _authorPosition, Color.Gray);
        }

        public bool IsStartGameClicked()
        {
            return _buttons[0].IsClicked;
        }

        public bool IsSettingsClicked()
        {
            return false; // Settings button is disabled
        }

        public bool IsExitClicked()
        {
            return _buttons[1].IsClicked; // Changed from [2] to [1] since we only have 2 buttons now
        }
    }
} 