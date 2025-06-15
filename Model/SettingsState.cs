using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DungeonFlux.Model
{
    public class SettingsState
    {
        private readonly List<Button> _buttons;
        private readonly SpriteFont _font;
        private float _scaleValue;
        private Rectangle _sliderBounds;
        private bool _isDragging;
        private const float MIN_SCALE = 0.1f;
        private const float MAX_SCALE = 2.0f;
        private const string TITLE = "Settings";
        private const int ELEMENT_SPACING = 75;
        private const int SLIDER_WIDTH = 200;
        private const int SLIDER_HEIGHT = 20;
        private const int TEXT_SLIDER_SPACING = 20;

        public SettingsState(SpriteFont font)
        {
            try
            {
                Logger.Log("Initializing SettingsState...");
                _font = font;
                _buttons = new List<Button>();
                _scaleValue = GameSettings.Graphics.Scale;
                
                var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                var buttonSize = new Vector2(GameSettings.Menu.ButtonWidth, GameSettings.Menu.ButtonHeight);
                
                var titleSize = _font.MeasureString(TITLE);
                var titlePosition = new Vector2(
                    displayMode.Width / 2 - titleSize.X / 2,
                    displayMode.Height / 4
                );

                string scaleText = $"Scale: {_scaleValue:F2}x";
                var textSize = _font.MeasureString(scaleText);
                float totalWidth = textSize.X + TEXT_SLIDER_SPACING + SLIDER_WIDTH;
                
                float scaleControlX = displayMode.Width / 2 - totalWidth / 2;
                float scaleControlY = titlePosition.Y + titleSize.Y + ELEMENT_SPACING;
                
                // Slider
                _sliderBounds = new Rectangle(
                    (int)(scaleControlX + textSize.X + TEXT_SLIDER_SPACING),
                    (int)(scaleControlY + textSize.Y/2 - SLIDER_HEIGHT/2),
                    SLIDER_WIDTH,
                    SLIDER_HEIGHT
                );
                
                // Back
                var backButtonPosition = new Vector2(
                    displayMode.Width / 2 - GameSettings.Menu.ButtonWidth / 2,
                    scaleControlY + SLIDER_HEIGHT + ELEMENT_SPACING*3
                );
                _buttons.Add(new Button("Back", backButtonPosition, buttonSize));
                
                Logger.Log("SettingsState initialization completed successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error during SettingsState initialization", ex);
                throw;
            }
        }

        public void Update(MouseState currentMouseState, MouseState previousMouseState, GameTime gameTime)
        {
            foreach (var button in _buttons)
            {
                button.Update(currentMouseState, previousMouseState, gameTime);
            }

            if (currentMouseState.LeftButton == ButtonState.Pressed && 
                previousMouseState.LeftButton == ButtonState.Released)
            {
                if (_sliderBounds.Contains(currentMouseState.Position))
                {
                    _isDragging = true;
                }
            }
            else if (currentMouseState.LeftButton == ButtonState.Released)
            {
                _isDragging = false;
            }

            if (_isDragging)
            {
                float relativeX = (currentMouseState.X - _sliderBounds.X) / (float)_sliderBounds.Width;
                relativeX = MathHelper.Clamp(relativeX, 0, 1);
                _scaleValue = MIN_SCALE + (MAX_SCALE - MIN_SCALE) * relativeX;
                GameSettings.Graphics.Scale = _scaleValue;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var titleSize = _font.MeasureString(TITLE);
            var titlePosition = new Vector2(
                spriteBatch.GraphicsDevice.Viewport.Width / 2 - titleSize.X / 2,
                spriteBatch.GraphicsDevice.Viewport.Height / 4
            );
            spriteBatch.DrawString(_font, TITLE, titlePosition, Color.White);


            string scaleText = $"Scale: {_scaleValue:F2}x";
            var textSize = _font.MeasureString(scaleText);
            float totalWidth = textSize.X + TEXT_SLIDER_SPACING + SLIDER_WIDTH;
            float scaleControlX = spriteBatch.GraphicsDevice.Viewport.Width / 2 - totalWidth / 2;
            float scaleControlY = titlePosition.Y + titleSize.Y + ELEMENT_SPACING;
            
            Vector2 textPosition = new Vector2(
                scaleControlX,
                scaleControlY
            );
            spriteBatch.DrawString(_font, scaleText, textPosition, Color.White);

            spriteBatch.Draw(GameSettings.Game.WhitePixel, _sliderBounds, Color.Gray);
            
            float handleX = _sliderBounds.X + (_scaleValue - MIN_SCALE) / (MAX_SCALE - MIN_SCALE) * _sliderBounds.Width;
            Rectangle handleBounds = new Rectangle(
                (int)handleX - 5,
                _sliderBounds.Y - 5,
                10,
                _sliderBounds.Height + 10
            );
            spriteBatch.Draw(GameSettings.Game.WhitePixel, handleBounds, Color.White);

            foreach (var button in _buttons)
            {
                button.Draw(spriteBatch, _font);
            }
        }

        public bool IsBackClicked()
        {
            return _buttons[0].IsClicked;
        }
    }
} 