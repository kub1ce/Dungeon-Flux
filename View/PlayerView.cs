using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using DungeonFlux.Model;
using System;

namespace DungeonFlux.View
{
    public class PlayerView
    {
        private readonly Player _model;
        private readonly Texture2D _texture;
        private readonly Texture2D _weaponTexture;
        private readonly GameView _gameView;
        private readonly Texture2D _pixel;

        public PlayerView(Player model, Texture2D texture, GameView gameView, ContentManager content)
        {
            _model = model;
            _texture = texture;
            _gameView = gameView;
            _pixel = new Texture2D(_gameView.SpriteBatch.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
            _weaponTexture = content.Load<Texture2D>("MosinRifle");
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
                int weaponSize = (int)(playerSize * 1.2f);

                // Отрисовка игрока
                spriteBatch.Draw(_texture, 
                    new Rectangle((int)position.X, (int)position.Y, playerSize, playerSize), 
                    Color.White);

                // Отрисовка оружия
                Vector2 weaponPosition = position + new Vector2(playerSize / 2, playerSize / 2);
                float weaponAngle = 0f;
                
                if (_model.Weapon.Direction != Vector2.Zero)
                {
                    weaponAngle = (float)Math.Atan2(_model.Weapon.Direction.Y, _model.Weapon.Direction.X);
                    weaponPosition += _model.Weapon.Direction * (playerSize * 0.2f);
                }

                weaponPosition += new Vector2(playerSize * 0.1f, playerSize * 0.15f);

                spriteBatch.Draw(
                    _weaponTexture,
                    weaponPosition,
                    null,
                    Color.White,
                    weaponAngle,
                    new Vector2(_weaponTexture.Width / 2, _weaponTexture.Height / 2),
                    weaponSize / (float)_weaponTexture.Width,
                    SpriteEffects.None,
                    0
                );

                // Отрисовка эффекта атаки
                var attackEffect = _model.Weapon.CurrentAttackEffect;
                if (attackEffect != null && attackEffect.IsActive)
                {
                    Vector2 effectStart = weaponPosition;
                    
                    Vector2 effectEnd = effectStart + attackEffect.Direction * _model.Weapon.Range * scale;

                    Vector2 direction = effectEnd - effectStart;
                    float length = _model.Weapon.Range * scale;
                    float angle = (float)Math.Atan2(direction.Y, direction.X);

                    Rectangle lineRect = new Rectangle(
                        (int)effectStart.X,
                        (int)effectStart.Y,
                        (int)length,
                        GameSettings.Weapon.AttackEffect.Thickness
                    );

                    float alpha = 1.0f - attackEffect.Progress;
                    Color effectColor = GameSettings.Weapon.AttackEffect.Color * alpha;

                    spriteBatch.Draw(
                        _pixel,
                        lineRect,
                        null,
                        effectColor,
                        angle,
                        new Vector2(0, GameSettings.Weapon.AttackEffect.Thickness / 2),
                        SpriteEffects.None,
                        0
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PlayerView.Draw: {ex}");
            }
        }
    }
}
