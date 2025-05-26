using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace DungeonFlux.Model
{
    /// Кнопка в меню
    public class Button
    {
        private const float HoverScale = 1.1f;
        private const float ClickScale = 0.9f;
        private const float AnimationSpeed = 0.1f;

        public string Text { get; }

        public Vector2 Position { get; }
        public Vector2 Size { get; }

        public bool IsClicked { get; private set; }
        public bool IsHovered { get; private set; }

        private float _scale = 1.0f;
        private float _targetScale = 1.0f;


        public Button(string text, Vector2 position, Vector2 size)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Button text cannot be empty"); // Мб потом сделать возможность пустой кнопки
            if (size.X <= 0 || size.Y <= 0)
                throw new ArgumentException("Button size must be positive");

            Text = text;
            Position = position;
            Size = size;
            IsClicked = false;
            IsHovered = false;
        }


        public void Update(MouseState currentMouseState, MouseState previousMouseState, GameTime gameTime)
        {
            UpdateHoverState(currentMouseState);
            UpdateClickState(currentMouseState, previousMouseState);
            UpdateScale(gameTime);
        }


        private void UpdateHoverState(MouseState mouseState)
        {
            var mousePosition = new Vector2(mouseState.X, mouseState.Y);
            IsHovered = mousePosition.X >= Position.X && mousePosition.X <= Position.X + Size.X &&
                       mousePosition.Y >= Position.Y && mousePosition.Y <= Position.Y + Size.Y;

            _targetScale = IsHovered ? HoverScale : 1.0f;
        }


        private void UpdateClickState(MouseState currentMouseState, MouseState previousMouseState)
        {
            IsClicked = IsHovered && currentMouseState.LeftButton == ButtonState.Released &&
                       previousMouseState.LeftButton == ButtonState.Pressed;

            if (IsClicked)
            {
                _targetScale = ClickScale;
            }
        }


        private void UpdateScale(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds * AnimationSpeed;
            _scale = MathHelper.Lerp(_scale, _targetScale, delta);
        }


        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            var color = IsHovered ? Color.Purple : Color.White;
            var textSize = font.MeasureString(Text);
            var textPosition = Position + (Size - textSize) / 2;
            
            spriteBatch.DrawString(font, Text, textPosition, color, 0, Vector2.Zero, _scale, SpriteEffects.None, 0);
        }
    }
} 