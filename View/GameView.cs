using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DungeonFlux.Model;
using System;

namespace DungeonFlux.View
{
    public class GameView
    {
        private readonly GameModel _model;
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _roomTexture;
        private readonly Texture2D _wallTexture;
        private Vector2 _dungeonOffset;

        public GameView(GameModel model, SpriteBatch spriteBatch)
        {
            _model = model;
            _spriteBatch = spriteBatch;
            
            _roomTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _roomTexture.SetData(new[] { Color.White });
            
            _wallTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _wallTexture.SetData(new[] { Color.White });

            CalculateDungeonOffset();
        }

        private void CalculateDungeonOffset()
        {
            int screenWidth = _spriteBatch.GraphicsDevice.Viewport.Width;
            int screenHeight = _spriteBatch.GraphicsDevice.Viewport.Height;

            int dungeonWidth = _model.Dungeon.GetLength(0) * GameSettings.Graphics.RoomSize;
            int dungeonHeight = _model.Dungeon.GetLength(1) * GameSettings.Graphics.RoomSize;

            Console.WriteLine($"Screen size: {screenWidth}x{screenHeight}");
            Console.WriteLine($"Dungeon size in pixels: {dungeonWidth}x{dungeonHeight}");

            _dungeonOffset = new Vector2(
                (screenWidth - dungeonWidth) / 2,
                (screenHeight - dungeonHeight) / 2
            );

            _dungeonOffset.X = MathHelper.Max(_dungeonOffset.X, GameSettings.Graphics.Padding);
            _dungeonOffset.Y = MathHelper.Max(_dungeonOffset.Y, GameSettings.Graphics.Padding);

            Console.WriteLine($"Dungeon offset: {_dungeonOffset}");
        }

        public void Draw(GameTime gameTime)
        {
            if (_model.Dungeon == null)
            {
                Console.WriteLine("Dungeon is null!");
                return;
            }

            Console.WriteLine($"Drawing dungeon with size: {_model.Dungeon.GetLength(0)}x{_model.Dungeon.GetLength(1)}");

            for (int x = 0; x < _model.Dungeon.GetLength(0); x++)
            {
                for (int y = 0; y < _model.Dungeon.GetLength(1); y++)
                {
                    var room = _model.Dungeon[x, y];
                    if (room == null) continue;

                    Vector2 position = new Vector2(x * GameSettings.Graphics.RoomSize, y * GameSettings.Graphics.RoomSize) + _dungeonOffset;

                    Color roomColor = GetRoomColor(room.Type);

                    _spriteBatch.Draw(_roomTexture, 
                        new Rectangle((int)position.X, (int)position.Y, GameSettings.Graphics.RoomSize, GameSettings.Graphics.RoomSize), 
                        roomColor);

                    DrawWalls(room, position);
                }
            }
        }

        private Color GetRoomColor(RoomType type)
        {
            return type switch
            {
                RoomType.Start => Color.Green,
                RoomType.Exit => Color.Red,
                // RoomType.Treasure => Color.Yellow,
                // RoomType.Enemy => Color.Orange,
                // RoomType.Boss => Color.Purple,
                // RoomType.Shop => Color.Blue,
                _ => Color.Gray // TODO: вынести в гейм сеттингс или вообще эта дебаг инфа не понадобится - незн. 
            };
        }

        private void DrawWalls(Room room, Vector2 position)
        {
            if (!room.HasConnection(GameSettings.Directions.Up))
            {
                _spriteBatch.Draw(_wallTexture,
                    new Rectangle((int)position.X, (int)position.Y, GameSettings.Graphics.RoomSize, GameSettings.Graphics.WallThickness),
                    Color.DarkGray);
            }
            if (!room.HasConnection(GameSettings.Directions.Right))
            {
                _spriteBatch.Draw(_wallTexture,
                    new Rectangle((int)position.X + GameSettings.Graphics.RoomSize - GameSettings.Graphics.WallThickness, (int)position.Y, GameSettings.Graphics.WallThickness, GameSettings.Graphics.RoomSize),
                    Color.DarkGray);
            }
            if (!room.HasConnection(GameSettings.Directions.Down))
            {
                _spriteBatch.Draw(_wallTexture,
                    new Rectangle((int)position.X, (int)position.Y + GameSettings.Graphics.RoomSize - GameSettings.Graphics.WallThickness, GameSettings.Graphics.RoomSize, GameSettings.Graphics.WallThickness),
                    Color.DarkGray);
            }
            if (!room.HasConnection(GameSettings.Directions.Left))
            {
                _spriteBatch.Draw(_wallTexture,
                    new Rectangle((int)position.X, (int)position.Y, GameSettings.Graphics.WallThickness, GameSettings.Graphics.RoomSize),
                    Color.DarkGray);
            }
        }

        public Vector2 GetDungeonOffset() => _dungeonOffset;
    }
}