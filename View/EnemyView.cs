using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DungeonFlux.Model;
using System;

namespace DungeonFlux.View
{
    public class EnemyView
    {
        private readonly Enemy _model;
        private readonly Texture2D _texture;
        private readonly GameView _gameView;
        private readonly Texture2D _pixel;
        private readonly SpriteFont _font;

        public EnemyView(Enemy model, Texture2D texture, GameView gameView, SpriteFont font)
        {
            _model = model;
            _texture = texture;
            _gameView = gameView;
            _font = font;
            _pixel = new Texture2D(_gameView.SpriteBatch.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_model.IsAlive) return;
            Vector2 cameraPosition = _gameView.CameraPosition;
            float scale = _gameView.Scale;
            int screenWidth = spriteBatch.GraphicsDevice.Viewport.Width;
            int screenHeight = spriteBatch.GraphicsDevice.Viewport.Height;
            Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);
            Vector2 enemyWorld = new Vector2(_model.Position.X * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2,
                                             _model.Position.Y * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2);
            Vector2 position = (enemyWorld - cameraPosition * GameSettings.Graphics.RoomSize - new Vector2(GameSettings.Graphics.RoomSize / 2)) * scale + screenCenter;
            int enemySize = (int)(GameSettings.Player.Size * scale);
            spriteBatch.Draw(_texture,
                new Rectangle((int)position.X, (int)position.Y, enemySize, enemySize),
                Color.White);

            // Хитбокс врага в режиме отладки
            if (GameSettings.Debug.IsDebugModeEnabled)
            {
                Rectangle hitbox = new Rectangle((int)position.X, (int)position.Y, enemySize, enemySize);
                
                Color hitboxColor = Color.Red;
                if (_model.Room != null && _gameView.PlayerRoom != null && _model.Room == _gameView.PlayerRoom)
                {
                    hitboxColor = Color.Orange;
                }
                
                spriteBatch.Draw(_pixel, hitbox, null, hitboxColor * 0.3f, 0f, Vector2.Zero, SpriteEffects.None, 1f);
                int border = 2;
                spriteBatch.Draw(_pixel, new Rectangle(hitbox.X, hitbox.Y, hitbox.Width, border), hitboxColor); // top
                spriteBatch.Draw(_pixel, new Rectangle(hitbox.X, hitbox.Y + hitbox.Height - border, hitbox.Width, border), hitboxColor); // down
                spriteBatch.Draw(_pixel, new Rectangle(hitbox.X, hitbox.Y, border, hitbox.Height), hitboxColor); // left
                spriteBatch.Draw(_pixel, new Rectangle(hitbox.X + hitbox.Width - border, hitbox.Y, border, hitbox.Height), hitboxColor); // right

                var positionText = $"{_model.Position.X:F2} {_model.Position.Y:F2}";
                var healthText = $"{_model.Health}";
                Vector2 positionTextSize = _font.MeasureString(positionText);
                Vector2 healthTextSize = _font.MeasureString(healthText);
                
                Vector2 positionTextPos = new Vector2(
                    position.X - (positionTextSize.X / 2) + enemySize / 2,
                    position.Y - positionTextSize.Y - 5 - healthTextSize.Y
                ); 
                spriteBatch.DrawString(_font, positionText, positionTextPos, Color.DarkRed);

                Vector2 healthTextPos = new Vector2(
                    position.X - (healthTextSize.X / 2) + enemySize / 2,
                    position.Y - healthTextSize.Y
                );
                spriteBatch.DrawString(_font, healthText, healthTextPos, Color.DarkRed);
            }
        }

        public void DrawAttackEffect(SpriteBatch spriteBatch)
        {
            var attackEffect = _model.CurrentAttackEffect;
            if (attackEffect != null && attackEffect.IsActive)
            {
                Vector2 cameraPosition = _gameView.CameraPosition;
                float scale = _gameView.Scale;
                int screenWidth = spriteBatch.GraphicsDevice.Viewport.Width;
                int screenHeight = spriteBatch.GraphicsDevice.Viewport.Height;
                Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);
                Vector2 enemyWorld = new Vector2(_model.Position.X * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2,
                                                _model.Position.Y * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2);
                Vector2 position = (enemyWorld - cameraPosition * GameSettings.Graphics.RoomSize - new Vector2(GameSettings.Graphics.RoomSize / 2)) * scale + screenCenter;
                int enemySize = (int)(GameSettings.Player.Size * scale);
                Vector2 effectCenter = position + new Vector2(enemySize / 2, enemySize / 2);
                float radius = attackEffect.Range * GameSettings.Graphics.RoomSize * scale;
                float alpha = 1.0f - attackEffect.Progress;
                Color effectColor = GameSettings.Enemy.AttackEffect.Color * alpha;

                // Вычисляем направление к игроку
                Vector2 playerWorld = new Vector2(
                    _gameView.Player.Position.X * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2,
                    _gameView.Player.Position.Y * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2
                );
                Vector2 playerScreen = (playerWorld - cameraPosition * GameSettings.Graphics.RoomSize - new Vector2(GameSettings.Graphics.RoomSize / 2)) * scale + screenCenter;
                Vector2 playerCenter = playerScreen + new Vector2(GameSettings.Player.Size * scale / 2, GameSettings.Player.Size * scale / 2);
                Vector2 directionToPlayer = playerCenter - effectCenter;
                directionToPlayer.Normalize();

                // Рисуем линию атаки
                Vector2 endPoint = effectCenter + directionToPlayer * radius;
                DrawLine(spriteBatch, effectCenter, endPoint, effectColor, GameSettings.Enemy.AttackEffect.Thickness);
            }
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            direction.Normalize();
            float angle = (float)Math.Atan2(direction.Y, direction.X);

            Rectangle lineRect = new Rectangle(
                (int)start.X,
                (int)start.Y,
                (int)length,
                thickness
            );

            spriteBatch.Draw(
                _pixel,
                lineRect,
                null,
                color,
                angle,
                new Vector2(0, thickness / 2),
                SpriteEffects.None,
                0
            );
        }
    }
} 