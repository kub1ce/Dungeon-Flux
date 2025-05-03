using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace DungeonFlux.Model
{
    public class MenuState
    {
        private const string AuthorName = "Created by Kubice";
        private const int ButtonWidth = 200;
        private const int ButtonHeight = 50;
        private const int ButtonSpacing = 200;
        private const int StartButtonY = 400;
        private const int AuthorMargin = 100;

        private readonly List<Button> _buttons;
        private readonly SpriteFont _font;
        private readonly string _authorText;
        private readonly Vector2 _authorPosition;

        public MenuState(SpriteFont font)
        {
            _font = font;
            _buttons = new List<Button>();
            
            // Создаем кнопки меню
            var buttonSize = new Vector2(ButtonWidth, ButtonHeight);
            var startButtonPosition = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2 - ButtonWidth / 2, StartButtonY);
            
            _buttons.Add(new Button("Start Game", startButtonPosition, buttonSize));
            _buttons.Add(new Button("Settings", startButtonPosition + new Vector2(0, ButtonSpacing), buttonSize));
            _buttons.Add(new Button("Exit", startButtonPosition + new Vector2(0, ButtonSpacing * 2), buttonSize));

            // Устанавливаем позицию текста автора
            _authorText = AuthorName;
            var screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            var screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            var textSize = _font.MeasureString(_authorText);
            _authorPosition = new Vector2(screenWidth - textSize.X - AuthorMargin, screenHeight - textSize.Y - AuthorMargin);
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
            return _buttons[1].IsClicked;
        }

        public bool IsExitClicked()
        {
            return _buttons[2].IsClicked;
        }
    }
} 