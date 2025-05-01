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
        private Vector2 _cameraPosition;
        private float _scale = 1.5f; // TODO: Масштаб, вынести в GameSettings

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

        public void UpdateCamera(Vector2 playerPosition)
        {
            int screenWidth = _spriteBatch.GraphicsDevice.Viewport.Width;
            int screenHeight = _spriteBatch.GraphicsDevice.Viewport.Height;
            float scale = _scale;

            float boxWidthPx = GameSettings.Player.Size * GameSettings.Camera.BoundingBoxWidthInPlayers * scale;
            float boxHeightPx = GameSettings.Player.Size * GameSettings.Camera.BoundingBoxHeightInPlayers * scale;
            boxWidthPx = Math.Min(boxWidthPx, screenWidth * GameSettings.Camera.MaxScreenWidthFraction);
            boxHeightPx = Math.Min(boxHeightPx, screenHeight * GameSettings.Camera.MaxScreenHeightFraction);
            float boxWidthWorld = boxWidthPx / scale;
            float boxHeightWorld = boxHeightPx / scale;

            Vector2 camera = _cameraPosition;
            if (camera == Vector2.Zero)
                camera = playerPosition;

            float camCenterX = camera.X * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2;
            float camCenterY = camera.Y * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2;
            float playerX = playerPosition.X * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2;
            float playerY = playerPosition.Y * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2;

            float left = camCenterX - boxWidthWorld / 2;
            float right = camCenterX + boxWidthWorld / 2 - GameSettings.Player.Size;
            float top = camCenterY - boxHeightWorld / 2;
            float bottom = camCenterY + boxHeightWorld / 2 - GameSettings.Player.Size;

            if (playerX < left)
                camera.X -= (left - playerX) / GameSettings.Graphics.RoomSize;
            if (playerX > right)
                camera.X += (playerX - right) / GameSettings.Graphics.RoomSize;
            if (playerY < top)
                camera.Y -= (top - playerY) / GameSettings.Graphics.RoomSize;
            if (playerY > bottom)
                camera.Y += (playerY - bottom) / GameSettings.Graphics.RoomSize;

            float minX = 0;
            float maxX = _model.Dungeon.GetLength(0) - 1;
            float minY = 0;
            float maxY = _model.Dungeon.GetLength(1) - 1;
            camera.X = MathHelper.Clamp(camera.X, minX, maxX);
            camera.Y = MathHelper.Clamp(camera.Y, minY, maxY);

            _cameraPosition = camera;
        }

        private (float left, float top, float width, float height) GetBoundingBoxWorldRect()
        {
            float scale = _scale;
            float boxWidthPx = GameSettings.Player.Size * GameSettings.Camera.BoundingBoxWidthInPlayers * scale;
            float boxHeightPx = GameSettings.Player.Size * GameSettings.Camera.BoundingBoxHeightInPlayers * scale;
            int screenWidth = _spriteBatch.GraphicsDevice.Viewport.Width;
            int screenHeight = _spriteBatch.GraphicsDevice.Viewport.Height;
            boxWidthPx = Math.Min(boxWidthPx, screenWidth * GameSettings.Camera.MaxScreenWidthFraction);
            boxHeightPx = Math.Min(boxHeightPx, screenHeight * GameSettings.Camera.MaxScreenHeightFraction);
            float boxWidthWorld = boxWidthPx / scale;
            float boxHeightWorld = boxHeightPx / scale;
            float camCenterX = _cameraPosition.X * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2;
            float camCenterY = _cameraPosition.Y * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2;
            float left = camCenterX - boxWidthWorld / 2;
            float top = camCenterY - boxHeightWorld / 2;
            return (left, top, boxWidthWorld, boxHeightWorld);
        }

        public void Draw(GameTime gameTime)
        {
            if (_model.Dungeon == null)
            {
                Console.WriteLine("Dungeon is null!");
                return;
            }

            int screenWidth = _spriteBatch.GraphicsDevice.Viewport.Width;
            int screenHeight = _spriteBatch.GraphicsDevice.Viewport.Height;
            float scale = _scale;
            Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);
            Vector2 cameraWorldOrigin = new Vector2(_cameraPosition.X * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2,
                                                    _cameraPosition.Y * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2);

            int roomsOnScreenX = (int)Math.Ceiling(screenWidth / (GameSettings.Graphics.RoomSize * _scale));
            int roomsOnScreenY = (int)Math.Ceiling(screenHeight / (GameSettings.Graphics.RoomSize * _scale));

            int camX = (int)_cameraPosition.X;
            int camY = (int)_cameraPosition.Y;

            int minX = Math.Max(0, camX - roomsOnScreenX / 2 - 1);
            int maxX = Math.Min(_model.Dungeon.GetLength(0), camX + roomsOnScreenX / 2 + 2);
            int minY = Math.Max(0, camY - roomsOnScreenY / 2 - 1);
            int maxY = Math.Min(_model.Dungeon.GetLength(1), camY + roomsOnScreenY / 2 + 2);

            Vector2 cameraOffset = new Vector2(screenWidth / 2, screenHeight / 2) - new Vector2(_cameraPosition.X * GameSettings.Graphics.RoomSize * _scale + GameSettings.Graphics.RoomSize * _scale / 2, _cameraPosition.Y * GameSettings.Graphics.RoomSize * _scale + GameSettings.Graphics.RoomSize * _scale / 2);

            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    var room = _model.Dungeon[x, y];
                    if (room == null) continue;

                    Vector2 position = new Vector2(x * GameSettings.Graphics.RoomSize, y * GameSettings.Graphics.RoomSize) * _scale + cameraOffset;

                    if (room.Type == RoomType.Corridor)
                    {
                        DrawCorridor(room, position, _scale);
                    }
                    else
                    {
                        Color roomColor = GetRoomColor(room.Type);
                        _spriteBatch.Draw(_roomTexture, 
                            new Rectangle((int)position.X, (int)position.Y, (int)(GameSettings.Graphics.RoomSize * _scale), (int)(GameSettings.Graphics.RoomSize * _scale)), 
                            roomColor);
                    }

                    DrawWalls(room, position, _scale);
                }
            }

            // Debug информация
            if (GameSettings.Debug.IsDebugModeEnabled)
            {
                DrawBoundingBox(screenWidth, screenHeight, scale, screenCenter);
                DrawCenterLines(screenWidth, screenHeight);
            }
        }

        private void DrawBoundingBox(int screenWidth, int screenHeight, float scale, Vector2 screenCenter)
        {
            var (boxLeftWorld, boxTopWorld, boxWidthWorld, boxHeightWorld) = GetBoundingBoxWorldRect();
            Vector2 boxWorldPos = new Vector2(boxLeftWorld, boxTopWorld);
            Vector2 boxScreenPos = (boxWorldPos - _cameraPosition * GameSettings.Graphics.RoomSize - new Vector2(GameSettings.Graphics.RoomSize / 2)) * scale + screenCenter;
            float boxWidthPx = boxWidthWorld * scale;
            float boxHeightPx = boxHeightWorld * scale;

            DrawRectangleBorder(
                new Rectangle((int)boxScreenPos.X, (int)boxScreenPos.Y, (int)boxWidthPx, (int)boxHeightPx),
                GameSettings.Debug.BoundingBoxBorderThickness,
                GameSettings.Debug.BoundingBoxColor
            );
        }

        private void DrawCenterLines(int screenWidth, int screenHeight)
        {
            _spriteBatch.Draw(_roomTexture, 
                new Rectangle(screenWidth / 2 - GameSettings.Debug.CenterLineThickness / 2, 0, 
                    GameSettings.Debug.CenterLineThickness, screenHeight), 
                GameSettings.Debug.CenterLineColor);

            _spriteBatch.Draw(_roomTexture, 
                new Rectangle(0, screenHeight / 2 - GameSettings.Debug.CenterLineThickness / 2, 
                    screenWidth, GameSettings.Debug.CenterLineThickness), 
                GameSettings.Debug.CenterLineColor);
        }

        private void DrawRectangleBorder(Rectangle rect, int borderThickness, Color color)
        {
            _spriteBatch.Draw(_roomTexture, 
                new Rectangle(rect.X, rect.Y, rect.Width, borderThickness), 
                color);
            _spriteBatch.Draw(_roomTexture, 
                new Rectangle(rect.X, rect.Y + rect.Height - borderThickness, rect.Width, borderThickness), 
                color);
            _spriteBatch.Draw(_roomTexture, 
                new Rectangle(rect.X, rect.Y, borderThickness, rect.Height), 
                color);
            _spriteBatch.Draw(_roomTexture, 
                new Rectangle(rect.X + rect.Width - borderThickness, rect.Y, borderThickness, rect.Height), 
                color);
        }

        private Color GetRoomColor(RoomType type)
        {
            return type switch
            {
                RoomType.Start => GameSettings.Graphics.StartRoomColor,
                RoomType.Exit => GameSettings.Graphics.ExitRoomColor,
                RoomType.Boss => GameSettings.Graphics.BossRoomColor,
                RoomType.Corridor => GameSettings.Graphics.CorridorColor,
                RoomType.DeadEnd => GameSettings.Graphics.DeadEndColor,
                _ => GameSettings.Graphics.DefaultRoomColor
            };
        }

        private void DrawWalls(Room room, Vector2 position, float scale)
        {
            int size = (int)(GameSettings.Graphics.RoomSize * scale);
            int wall = (int)(GameSettings.Graphics.WallThickness * scale);
            int corridorWidth = (int)(GameSettings.Graphics.RoomSize * scale * 0.3f);
            int centerOffset = (size - corridorWidth) / 2;
            
            if (room.Type == RoomType.Corridor)
            {
                if (!room.HasConnection(GameSettings.Directions.Up))
                {
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X + centerOffset, (int)position.Y + centerOffset, corridorWidth, wall),
                        GameSettings.Graphics.CorridorWallColor);
                }

                if (!room.HasConnection(GameSettings.Directions.Right))
                {
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X + centerOffset + corridorWidth - wall, (int)position.Y + centerOffset, wall, corridorWidth),
                        GameSettings.Graphics.CorridorWallColor);
                }

                if (!room.HasConnection(GameSettings.Directions.Down))
                {
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X + centerOffset, (int)position.Y + centerOffset + corridorWidth - wall, corridorWidth, wall),
                        GameSettings.Graphics.CorridorWallColor);
                }

                if (!room.HasConnection(GameSettings.Directions.Left))
                {
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X + centerOffset, (int)position.Y + centerOffset, wall, corridorWidth),
                        GameSettings.Graphics.CorridorWallColor);
                }

                if (room.HasConnection(GameSettings.Directions.Up))
                {
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X + centerOffset, (int)position.Y, wall, centerOffset),
                        GameSettings.Graphics.CorridorWallColor);
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X + centerOffset + corridorWidth - wall, (int)position.Y, wall, centerOffset),
                        GameSettings.Graphics.CorridorWallColor);
                }

                if (room.HasConnection(GameSettings.Directions.Right))
                {
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X + centerOffset + corridorWidth, (int)position.Y + centerOffset, size - (centerOffset + corridorWidth), wall),
                        GameSettings.Graphics.CorridorWallColor);
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X + centerOffset + corridorWidth, (int)position.Y + centerOffset + corridorWidth - wall, size - (centerOffset + corridorWidth), wall),
                        GameSettings.Graphics.CorridorWallColor);
                }

                if (room.HasConnection(GameSettings.Directions.Down))
                {
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X + centerOffset, (int)position.Y + centerOffset + corridorWidth, wall, size - (centerOffset + corridorWidth)),
                        GameSettings.Graphics.CorridorWallColor);
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X + centerOffset + corridorWidth - wall, (int)position.Y + centerOffset + corridorWidth, wall, size - (centerOffset + corridorWidth)),
                        GameSettings.Graphics.CorridorWallColor);
                }

                if (room.HasConnection(GameSettings.Directions.Left))
                {
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X, (int)position.Y + centerOffset, centerOffset, wall),
                        GameSettings.Graphics.CorridorWallColor);
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X, (int)position.Y + centerOffset + corridorWidth - wall, centerOffset, wall),
                        GameSettings.Graphics.CorridorWallColor);
                }
            }
            else
            {
                if (!room.HasConnection(GameSettings.Directions.Up))
                {
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X, (int)position.Y, size, wall),
                        GameSettings.Graphics.RoomWallColor);
                }
                else
                {
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X, (int)position.Y, centerOffset, wall),
                        GameSettings.Graphics.RoomWallColor);
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X + centerOffset + corridorWidth, (int)position.Y, size - (centerOffset + corridorWidth), wall),
                        GameSettings.Graphics.RoomWallColor);
                }

                if (!room.HasConnection(GameSettings.Directions.Right))
                {
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X + size - wall, (int)position.Y, wall, size),
                        GameSettings.Graphics.RoomWallColor);
                }
                else
                {
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X + size - wall, (int)position.Y, wall, centerOffset),
                        GameSettings.Graphics.RoomWallColor);
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X + size - wall, (int)position.Y + centerOffset + corridorWidth, wall, size - (centerOffset + corridorWidth)),
                        GameSettings.Graphics.RoomWallColor);
                }

                if (!room.HasConnection(GameSettings.Directions.Down))
                {
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X, (int)position.Y + size - wall, size, wall),
                        GameSettings.Graphics.RoomWallColor);
                }
                else
                {
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X, (int)position.Y + size - wall, centerOffset, wall),
                        GameSettings.Graphics.RoomWallColor);
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X + centerOffset + corridorWidth, (int)position.Y + size - wall, size - (centerOffset + corridorWidth), wall),
                        GameSettings.Graphics.RoomWallColor);
                }

                if (!room.HasConnection(GameSettings.Directions.Left))
                {
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X, (int)position.Y, wall, size),
                        GameSettings.Graphics.RoomWallColor);
                }
                else
                {
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X, (int)position.Y, wall, centerOffset),
                        GameSettings.Graphics.RoomWallColor);
                    _spriteBatch.Draw(_wallTexture,
                        new Rectangle((int)position.X, (int)position.Y + centerOffset + corridorWidth, wall, size - (centerOffset + corridorWidth)),
                        GameSettings.Graphics.RoomWallColor);
                }
            }
        }

        private void DrawCorridor(Room room, Vector2 position, float scale)
        {
            int size = (int)(GameSettings.Graphics.RoomSize * scale);
            int corridorWidth = (int)(GameSettings.Graphics.RoomSize * scale * 0.3f); // Ширина коридора 30% от размера комнаты
            int centerOffset = (size - corridorWidth) / 2;

            _spriteBatch.Draw(_roomTexture,
                new Rectangle(
                    (int)position.X + centerOffset,
                    (int)position.Y + centerOffset,
                    corridorWidth,
                    corridorWidth),
                Color.DarkGray);

            if (room.HasConnection(GameSettings.Directions.Up))
            {
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle(
                        (int)position.X + centerOffset,
                        (int)position.Y,
                        corridorWidth,
                        centerOffset + corridorWidth / 2),
                    Color.DarkGray);
            }

            if (room.HasConnection(GameSettings.Directions.Right))
            {
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle(
                        (int)position.X + centerOffset + corridorWidth / 2,
                        (int)position.Y + centerOffset,
                        size - (centerOffset + corridorWidth / 2),
                        corridorWidth),
                    Color.DarkGray);
            }

            if (room.HasConnection(GameSettings.Directions.Down))
            {
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle(
                        (int)position.X + centerOffset,
                        (int)position.Y + centerOffset + corridorWidth / 2,
                        corridorWidth,
                        size - (centerOffset + corridorWidth / 2)),
                    Color.DarkGray);
            }

            if (room.HasConnection(GameSettings.Directions.Left))
            {
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle(
                        (int)position.X,
                        (int)position.Y + centerOffset,
                        centerOffset + corridorWidth / 2,
                        corridorWidth),
                    Color.DarkGray);
            }
        }

        public Vector2 CameraPosition => _cameraPosition;
        public float Scale => _scale;
    }
}