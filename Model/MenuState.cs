using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
            _font = font;
            _buttons = new List<Button>();
            
            _buttons.Add(new Button("Start Game", new Vector2(400, 400), new Vector2(200, 50)));
            _buttons.Add(new Button("Settings", new Vector2(400, 600), new Vector2(200, 50)));
            _buttons.Add(new Button("Exit", new Vector2(400, 800), new Vector2(200, 50)));

            _authorText = "Created by Kubice";
            _authorPosition = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 600, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100);
        }

        public void Update(MouseState currentMouseState, MouseState previousMouseState)
        {
            foreach (var button in _buttons)
            {
                button.Update(currentMouseState, previousMouseState);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var button in _buttons)
            {
                button.Draw(spriteBatch, _font);
            }

            var textSize = _font.MeasureString(_authorText);
            var textPosition = _authorPosition;
            spriteBatch.DrawString(_font, _authorText, textPosition, Color.Gray);
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

    public class Button
    {
        public string Text { get; }
        public Vector2 Position { get; }
        public Vector2 Size { get; }
        public bool IsClicked { get; private set; }
        public bool IsHovered { get; private set; }

        public Button(string text, Vector2 position, Vector2 size)
        {
            Text = text;
            Position = position;
            Size = size;
            IsClicked = false;
            IsHovered = false;
        }

        public void Update(MouseState currentMouseState, MouseState previousMouseState)
        {
            var mousePosition = new Vector2(currentMouseState.X, currentMouseState.Y);
            IsHovered = mousePosition.X >= Position.X && mousePosition.X <= Position.X + Size.X &&
                       mousePosition.Y >= Position.Y && mousePosition.Y <= Position.Y + Size.Y;

            IsClicked = IsHovered && currentMouseState.LeftButton == ButtonState.Released &&
                       previousMouseState.LeftButton == ButtonState.Pressed;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            var color = IsHovered ? Color.Purple : Color.White;
            var textSize = font.MeasureString(Text);
            var textPosition = Position; // Выравнивание по центру: + (Size - textSize) / 2;
            
            spriteBatch.DrawString(font, Text, textPosition, color);
        }
    }
} 