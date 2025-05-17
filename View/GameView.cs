using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DungeonFlux.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonFlux.View
{
    public class WallInfo
    {
        public Rectangle Rectangle { get; set; }
        public Color Color { get; set; }
        public bool IsCorridor { get; set; }
        public int Direction { get; set; }
    }

    public class GameView
    {
        private readonly GameModel _model;
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _roomTexture;
        private readonly Texture2D _wallTexture;
        private readonly SpriteFont _font;
        private readonly Player _player;
        private Vector2 _dungeonOffset;
        private List<WallInfo> _walls = new List<WallInfo>();
        private Texture2D _enemyTexture;
        private Dictionary<Enemy, EnemyView> _enemyViews = new();

        public SpriteBatch SpriteBatch => _spriteBatch;
        public Vector2 CameraPosition => _model.CameraPosition;
        public float Scale => _model.Scale;
        public Room PlayerRoom { get; private set; }

        public GameView(GameModel model, SpriteBatch spriteBatch, SpriteFont font, Player player, Texture2D enemyTexture)
        {
            _model = model;
            _spriteBatch = spriteBatch;
            _font = font;
            _player = player;
            
            _roomTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _roomTexture.SetData(new[] { Color.White });
            
            _wallTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _wallTexture.SetData(new[] { Color.White });

            CalculateDungeonOffset();
            _enemyTexture = enemyTexture;
        }

        private void CalculateDungeonOffset()
        {
            int screenWidth = _spriteBatch.GraphicsDevice.Viewport.Width;
            int screenHeight = _spriteBatch.GraphicsDevice.Viewport.Height;

            int dungeonWidth = _model.Dungeon.GetLength(0) * GameSettings.Graphics.RoomSize;
            int dungeonHeight = _model.Dungeon.GetLength(1) * GameSettings.Graphics.RoomSize;

            _dungeonOffset = new Vector2(
                (screenWidth - dungeonWidth) / 2,
                (screenHeight - dungeonHeight) / 2
            );

            _dungeonOffset.X = MathHelper.Max(_dungeonOffset.X, GameSettings.Graphics.Padding);
            _dungeonOffset.Y = MathHelper.Max(_dungeonOffset.Y, GameSettings.Graphics.Padding);
        }

        public void UpdateCamera(Vector2 playerPosition)
        {
            int screenWidth = _spriteBatch.GraphicsDevice.Viewport.Width;
            int screenHeight = _spriteBatch.GraphicsDevice.Viewport.Height;
            float scale = Scale;

            float boxWidthPx = GameSettings.Player.Size * GameSettings.Camera.BoundingBox.WidthInPlayers * scale;
            float boxHeightPx = GameSettings.Player.Size * GameSettings.Camera.BoundingBox.HeightInPlayers * scale;
            boxWidthPx = Math.Min(boxWidthPx, screenWidth * GameSettings.Camera.ScreenLimits.MaxWidthFraction);
            boxHeightPx = Math.Min(boxHeightPx, screenHeight * GameSettings.Camera.ScreenLimits.MaxHeightFraction);
            float boxWidthWorld = boxWidthPx / scale;
            float boxHeightWorld = boxHeightPx / scale;

            Vector2 camera = CameraPosition;
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

            _model.SetCameraPosition(camera);
        }

        private (float left, float top, float width, float height) GetBoundingBoxWorldRect()
        {
            float scale = Scale;
            float boxWidthPx = GameSettings.Player.Size * GameSettings.Camera.BoundingBox.WidthInPlayers * scale;
            float boxHeightPx = GameSettings.Player.Size * GameSettings.Camera.BoundingBox.HeightInPlayers * scale;
            int screenWidth = _spriteBatch.GraphicsDevice.Viewport.Width;
            int screenHeight = _spriteBatch.GraphicsDevice.Viewport.Height;
            boxWidthPx = Math.Min(boxWidthPx, screenWidth * GameSettings.Camera.ScreenLimits.MaxWidthFraction);
            boxHeightPx = Math.Min(boxHeightPx, screenHeight * GameSettings.Camera.ScreenLimits.MaxHeightFraction);
            float boxWidthWorld = boxWidthPx / scale;
            float boxHeightWorld = boxHeightPx / scale;
            float camCenterX = CameraPosition.X * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2;
            float camCenterY = CameraPosition.Y * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2;
            float left = camCenterX - boxWidthWorld / 2;
            float top = camCenterY - boxHeightWorld / 2;
            return (left, top, boxWidthWorld, boxHeightWorld);
        }

        public void Draw(GameTime gameTime)
        {
            if (_model.Dungeon == null || _font == null)
                return;

            _walls.Clear();

            int screenWidth = _spriteBatch.GraphicsDevice.Viewport.Width;
            int screenHeight = _spriteBatch.GraphicsDevice.Viewport.Height;
            float scale = Scale;
            Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);
            Vector2 cameraWorldOrigin = new Vector2(
                CameraPosition.X * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2,
                CameraPosition.Y * GameSettings.Graphics.RoomSize + GameSettings.Graphics.RoomSize / 2
            );

            int roomsOnScreenX = (int)Math.Ceiling(screenWidth / (GameSettings.Graphics.RoomSize * scale));
            int roomsOnScreenY = (int)Math.Ceiling(screenHeight / (GameSettings.Graphics.RoomSize * scale));

            int camX = (int)CameraPosition.X;
            int camY = (int)CameraPosition.Y;

            int minX = Math.Max(0, camX - roomsOnScreenX / 2 - 1);
            int maxX = Math.Min(_model.Dungeon.GetLength(0), camX + roomsOnScreenX / 2 + 2);
            int minY = Math.Max(0, camY - roomsOnScreenY / 2 - 1);
            int maxY = Math.Min(_model.Dungeon.GetLength(1), camY + roomsOnScreenY / 2 + 2);

            Vector2 cameraOffset = screenCenter - 
                new Vector2(
                    CameraPosition.X * GameSettings.Graphics.RoomSize * scale + GameSettings.Graphics.RoomSize * scale / 2, 
                    CameraPosition.Y * GameSettings.Graphics.RoomSize * scale + GameSettings.Graphics.RoomSize * scale / 2
                );

            // Draw rooms and walls
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    var room = _model.Dungeon[x, y];
                    if (room == null) continue;

                    Vector2 position = new Vector2(x * GameSettings.Graphics.RoomSize, y * GameSettings.Graphics.RoomSize) * scale + cameraOffset;

                    DrawRoom(room, position, scale);
                    DrawWalls(room, position, scale);
                }
            }

            // Draw doors
            foreach (var wall in _model.Walls)
            {
                if (!wall.IsDoor) continue;

                int roomX = wall.Bounds.X / GameSettings.Graphics.RoomSize;
                int roomY = wall.Bounds.Y / GameSettings.Graphics.RoomSize;

                if (roomX >= minX && roomX < maxX && roomY >= minY && roomY < maxY)
                {
                    Vector2 position = new Vector2(roomX * GameSettings.Graphics.RoomSize, roomY * GameSettings.Graphics.RoomSize) * scale + cameraOffset;
                    
                    if (!wall.IsOpen)
                    {
                        Rectangle scaledBounds = new Rectangle(
                            (int)(position.X + (wall.Bounds.X % GameSettings.Graphics.RoomSize) * scale),
                            (int)(position.Y + (wall.Bounds.Y % GameSettings.Graphics.RoomSize) * scale),
                            (int)(wall.Bounds.Width * scale),
                            (int)(wall.Bounds.Height * scale)
                        );
                        DrawWall(scaledBounds.X, scaledBounds.Y, scaledBounds.Width, scaledBounds.Height, GameSettings.Graphics.WallColors.Room);
                    }
                }
            }

            // Draw enemies
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    var room = _model.Dungeon[x, y];
                    if (room == null) continue;
                    foreach (var enemy in room.Enemies)
                    {
                        if (!_enemyViews.ContainsKey(enemy))
                            _enemyViews[enemy] = new EnemyView(enemy, _enemyTexture, this, _font);
                        _enemyViews[enemy].Draw(_spriteBatch);
                    }
                }
            }

            // Draw debug info for rooms
            if (GameSettings.Debug.IsDebugModeEnabled)
            {
                // Update player's current room
                int playerRoomX = (int)Math.Round(_player.Position.X);
                int playerRoomY = (int)Math.Round(_player.Position.Y);
                PlayerRoom = _model.Dungeon[playerRoomX, playerRoomY];

                for (int x = minX; x < maxX; x++)
                {
                    for (int y = minY; y < maxY; y++)
                    {
                        var room = _model.Dungeon[x, y];
                        if (room == null) continue;

                        Vector2 position = new Vector2(x * GameSettings.Graphics.RoomSize, y * GameSettings.Graphics.RoomSize) * scale + cameraOffset;
                        DrawDebugInfo(room, position, scale, x, y);
                    }
                }

                // Draw debug info for doors
                foreach (var wall in _model.Walls)
                {
                    if (!wall.IsDoor) continue;

                    int roomX = wall.Bounds.X / GameSettings.Graphics.RoomSize;
                    int roomY = wall.Bounds.Y / GameSettings.Graphics.RoomSize;

                    if (roomX >= minX && roomX < maxX && roomY >= minY && roomY < maxY)
                    {
                        Vector2 position = new Vector2(roomX * GameSettings.Graphics.RoomSize, roomY * GameSettings.Graphics.RoomSize) * scale + cameraOffset;
                        Rectangle scaledBounds = new Rectangle(
                            (int)(position.X + (wall.Bounds.X % GameSettings.Graphics.RoomSize) * scale),
                            (int)(position.Y + (wall.Bounds.Y % GameSettings.Graphics.RoomSize) * scale),
                            (int)(wall.Bounds.Width * scale),
                            (int)(wall.Bounds.Height * scale)
                        );
                        
                        // Draw center line
                        if (scaledBounds.Width > scaledBounds.Height)
                        {
                            int centerY = scaledBounds.Y + scaledBounds.Height / 2;
                            _spriteBatch.Draw(_wallTexture, 
                                new Rectangle(scaledBounds.X, centerY - 1, scaledBounds.Width, 2),
                                wall.IsOpen ? Color.LimeGreen : Color.Red);
                        }
                        else
                        {
                            int centerX = scaledBounds.X + scaledBounds.Width / 2;
                            _spriteBatch.Draw(_wallTexture, 
                                new Rectangle(centerX - 1, scaledBounds.Y, 2, scaledBounds.Height),
                                wall.IsOpen ? Color.LimeGreen : Color.Red);
                        }
                        DrawRectangleBorder(scaledBounds, 2, wall.IsOpen ? Color.LimeGreen : Color.Red);
                    }
                }

                DrawDebugOverlay(screenWidth, screenHeight, scale, screenCenter);
            }
        }

        private void DrawRoom(Room room, Vector2 position, float scale)
        {
            if (room.Type == RoomType.Corridor)
            {
                DrawCorridor(room, position, scale);
            }
            else
            {
                Color roomColor = GetRoomColor(room.Type);
                _spriteBatch.Draw(_roomTexture, 
                    new Rectangle(
                        (int)position.X, 
                        (int)position.Y, 
                        (int)(GameSettings.Graphics.RoomSize * scale), 
                        (int)(GameSettings.Graphics.RoomSize * scale)
                    ), 
                    roomColor);
            }
        }

        private void DrawDebugInfo(Room room, Vector2 position, float scale, int roomX, int roomY)
        {
            float playerCenterX = _player.Position.X + (GameSettings.Player.Size / 2f) / GameSettings.Graphics.RoomSize;
            float playerCenterY = _player.Position.Y + (GameSettings.Player.Size / 2f) / GameSettings.Graphics.RoomSize;
            int playerRoomX = (int)(playerCenterX + 0.5f);
            int playerRoomY = (int)(playerCenterY + 0.5f);
            bool isPlayerRoom = (roomX == playerRoomX && roomY == playerRoomY);

            string roomTypeText = room.Type.ToString();
            Vector2 textSize = _font.MeasureString(roomTypeText);
            Vector2 textPos = position + new Vector2(
                (GameSettings.Graphics.RoomSize * scale - textSize.X) / 2,
                (GameSettings.Graphics.RoomSize * scale - textSize.Y) / 2
            );
            Color textColor = isPlayerRoom ? Color.LimeGreen : Color.White;
            _spriteBatch.DrawString(_font, roomTypeText, textPos, textColor);

            DrawRectangleBorder(
                new Rectangle(
                    (int)position.X, 
                    (int)position.Y, 
                    (int)(GameSettings.Graphics.RoomSize * scale), 
                    (int)(GameSettings.Graphics.RoomSize * scale)
                ),
                2,
                Color.Yellow
            );

            if (room.Type == RoomType.Corridor)
            {
                // DrawCorridorBorders(room, position, scale); // ! useless
            }

            // Отрисовка границ стен в режиме отладки
            if (GameSettings.Debug.IsDebugModeEnabled)
            {
                foreach (var wall in _model.Walls)
                {
                    if (wall.Bounds.X / GameSettings.Graphics.RoomSize == roomX && 
                        wall.Bounds.Y / GameSettings.Graphics.RoomSize == roomY)
                    {
                        Rectangle scaledBounds = new Rectangle(
                            (int)(wall.Bounds.X * scale) - (int)(roomX * GameSettings.Graphics.RoomSize * scale),
                            (int)(wall.Bounds.Y * scale) - (int)(roomY * GameSettings.Graphics.RoomSize * scale),
                            (int)(wall.Bounds.Width * scale),
                            (int)(wall.Bounds.Height * scale)
                        );

                        Color wallColor = wall.IsCorridor ? Color.Red : Color.Yellow;
                        if (wall.IsPassable())
                        {
                            wallColor = Color.LimeGreen;
                        }
                        else if (wall.IsDoor)
                        {
                            wallColor = wall.IsOpen ? Color.LimeGreen : Color.Red;
                        }

                        DrawRectangleBorder(
                            new Rectangle(
                                (int)position.X + scaledBounds.X,
                                (int)position.Y + scaledBounds.Y,
                                scaledBounds.Width,
                                scaledBounds.Height
                            ),
                            2,
                            wallColor
                        );
                    }
                }
            }
        }

        private void DrawDebugOverlay(int screenWidth, int screenHeight, float scale, Vector2 screenCenter)
        {
            DrawBoundingBox(screenWidth, screenHeight, scale, screenCenter);
            DrawCenterLines(screenWidth, screenHeight);

            try
            {
                float playerCenterX = _player.Position.X + (GameSettings.Player.Size / 2f) / GameSettings.Graphics.RoomSize;
                float playerCenterY = _player.Position.Y + (GameSettings.Player.Size / 2f) / GameSettings.Graphics.RoomSize;
                int playerRoomX = (int)(playerCenterX + 0.5f);
                int playerRoomY = (int)(playerCenterY + 0.5f);

                if (_model.Dungeon != null && 
                    playerRoomX >= 0 && playerRoomX < _model.Dungeon.GetLength(0) &&
                    playerRoomY >= 0 && playerRoomY < _model.Dungeon.GetLength(1))
                {
                    var currentRoom = _model.Dungeon[playerRoomX, playerRoomY];
                    string roomType = currentRoom?.Type.ToString() ?? "null";
                    string debugInfo =
                        $"Current Room Type: {roomType}\n" +
                        $"XY: {_player.Position.X:F2} {_player.Position.Y:F2}\n" +
                        $"Room: {playerRoomX} {playerRoomY}\n" +
                        $"XP: {_player.Health}\n" +
                        $"Enemies: {currentRoom?.Enemies.Count(e => e.IsAlive) ?? 0}";
                    _spriteBatch.DrawString(_font, debugInfo, new Vector2(10, 10), Color.DarkCyan);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error drawing debug info: {ex.Message}");
            }
        }

        private void DrawBoundingBox(int screenWidth, int screenHeight, float scale, Vector2 screenCenter)
        {
            var (boxLeftWorld, boxTopWorld, boxWidthWorld, boxHeightWorld) = GetBoundingBoxWorldRect();
            Vector2 boxWorldPos = new Vector2(boxLeftWorld, boxTopWorld);
            Vector2 boxScreenPos = (boxWorldPos - CameraPosition * GameSettings.Graphics.RoomSize - new Vector2(GameSettings.Graphics.RoomSize / 2)) * scale + screenCenter;
            float boxWidthPx = boxWidthWorld * scale;
            float boxHeightPx = boxHeightWorld * scale;

            DrawRectangleBorder(
                new Rectangle((int)boxScreenPos.X, (int)boxScreenPos.Y, (int)boxWidthPx, (int)boxHeightPx),
                GameSettings.Debug.BoundingBox.BorderThickness,
                GameSettings.Debug.BoundingBox.Color
            );
        }

        private void DrawCenterLines(int screenWidth, int screenHeight)
        {
            _spriteBatch.Draw(_roomTexture, 
                new Rectangle(
                    screenWidth / 2 - GameSettings.Debug.CenterLine.Thickness / 2, 
                    0, 
                    GameSettings.Debug.CenterLine.Thickness, 
                    screenHeight
                ), 
                GameSettings.Debug.CenterLine.Color);

            _spriteBatch.Draw(_roomTexture, 
                new Rectangle(
                    0, 
                    screenHeight / 2 - GameSettings.Debug.CenterLine.Thickness / 2, 
                    screenWidth, 
                    GameSettings.Debug.CenterLine.Thickness
                ), 
                GameSettings.Debug.CenterLine.Color);
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
                RoomType.Start => GameSettings.Graphics.RoomColors.StartRoom,
                RoomType.Exit => GameSettings.Graphics.RoomColors.ExitRoom,
                RoomType.Boss => GameSettings.Graphics.RoomColors.BossRoom,
                RoomType.Corridor => GameSettings.Graphics.RoomColors.Corridor,
                RoomType.DeadEnd => GameSettings.Graphics.RoomColors.DeadEnd,
                _ => GameSettings.Graphics.RoomColors.Default
            };
        }

        private void DrawWalls(Room room, Vector2 position, float scale)
        {
            int size = (int)(GameSettings.Graphics.RoomSize * scale);
            int wall = (int)(GameSettings.Graphics.WallThickness * scale);
            int corridorWidth = (int)(GameSettings.Graphics.RoomSize * scale * GameSettings.Graphics.Corridor.WidthRatio);
            int centerOffset = (size - corridorWidth) / 2;
            
            if (room.Type == RoomType.Corridor)
            {
                DrawCorridorWalls(room, position, scale, size, wall, corridorWidth, centerOffset);
            }
            else
            {
                DrawRoomWalls(room, position, scale, size, wall, corridorWidth, centerOffset);
            }
        }

        private void DrawCorridorWalls(Room room, Vector2 position, float scale, int size, int wall, int corridorWidth, int centerOffset)
        {
            Color wallColor = GameSettings.Graphics.WallColors.Corridor;

            if (!room.HasConnection(GameSettings.Directions.Up))
            {
                DrawWall(position.X + centerOffset, position.Y + centerOffset, corridorWidth, wall, wallColor);
            }

            if (!room.HasConnection(GameSettings.Directions.Right))
            {
                DrawWall(position.X + centerOffset + corridorWidth - wall, position.Y + centerOffset, wall, corridorWidth, wallColor);
            }

            if (!room.HasConnection(GameSettings.Directions.Down))
            {
                DrawWall(position.X + centerOffset, position.Y + centerOffset + corridorWidth - wall, corridorWidth, wall, wallColor);
            }

            if (!room.HasConnection(GameSettings.Directions.Left))
            {
                DrawWall(position.X + centerOffset, position.Y + centerOffset, wall, corridorWidth, wallColor);
            }

            if (room.HasConnection(GameSettings.Directions.Up))
            {
                DrawWall(position.X + centerOffset, position.Y, wall, centerOffset, wallColor);
                DrawWall(position.X + centerOffset + corridorWidth - wall, position.Y, wall, centerOffset, wallColor);
            }

            if (room.HasConnection(GameSettings.Directions.Right))
            {
                DrawWall(position.X + centerOffset + corridorWidth, position.Y + centerOffset, size - (centerOffset + corridorWidth), wall, wallColor);
                DrawWall(position.X + centerOffset + corridorWidth, position.Y + centerOffset + corridorWidth - wall, size - (centerOffset + corridorWidth), wall, wallColor);
            }

            if (room.HasConnection(GameSettings.Directions.Down))
            {
                DrawWall(position.X + centerOffset, position.Y + centerOffset + corridorWidth, wall, size - (centerOffset + corridorWidth), wallColor);
                DrawWall(position.X + centerOffset + corridorWidth - wall, position.Y + centerOffset + corridorWidth, wall, size - (centerOffset + corridorWidth), wallColor);
            }

            if (room.HasConnection(GameSettings.Directions.Left))
            {
                DrawWall(position.X, position.Y + centerOffset, centerOffset, wall, wallColor);
                DrawWall(position.X, position.Y + centerOffset + corridorWidth - wall, centerOffset, wall, wallColor);
            }
        }

        private void DrawRoomWalls(Room room, Vector2 position, float scale, int size, int wall, int corridorWidth, int centerOffset)
        {
            Color wallColor = GameSettings.Graphics.WallColors.Room;

            if (!room.HasConnection(GameSettings.Directions.Up))
            {
                DrawWall(position.X, position.Y, size, wall, wallColor);
            }
            else
            {
                DrawWall(position.X, position.Y, centerOffset, wall, wallColor);
                DrawWall(position.X + centerOffset + corridorWidth, position.Y, size - (centerOffset + corridorWidth), wall, wallColor);
            }

            if (!room.HasConnection(GameSettings.Directions.Right))
            {
                DrawWall(position.X + size - wall, position.Y, wall, size, wallColor);
            }
            else
            {
                DrawWall(position.X + size - wall, position.Y, wall, centerOffset, wallColor);
                DrawWall(position.X + size - wall, position.Y + centerOffset + corridorWidth, wall, size - (centerOffset + corridorWidth), wallColor);
            }

            if (!room.HasConnection(GameSettings.Directions.Down))
            {
                DrawWall(position.X, position.Y + size - wall, size, wall, wallColor);
            }
            else
            {
                DrawWall(position.X, position.Y + size - wall, centerOffset, wall, wallColor);
                DrawWall(position.X + centerOffset + corridorWidth, position.Y + size - wall, size - (centerOffset + corridorWidth), wall, wallColor);
            }

            if (!room.HasConnection(GameSettings.Directions.Left))
            {
                DrawWall(position.X, position.Y, wall, size, wallColor);
            }
            else
            {
                DrawWall(position.X, position.Y, wall, centerOffset, wallColor);
                DrawWall(position.X, position.Y + centerOffset + corridorWidth, wall, size - (centerOffset + corridorWidth), wallColor);
            }
        }

        private void DrawWall(float x, float y, int width, int height, Color color)
        {
            _spriteBatch.Draw(_wallTexture, new Rectangle((int)x, (int)y, width, height), color);
        }

        private void DrawCorridor(Room room, Vector2 position, float scale)
        {
            int size = (int)(GameSettings.Graphics.RoomSize * scale);
            int corridorWidth = (int)(GameSettings.Graphics.RoomSize * scale * GameSettings.Graphics.Corridor.WidthRatio);
            int centerOffset = (size - corridorWidth) / 2;

            DrawCorridorCenter(room, position, scale, corridorWidth, centerOffset);
            DrawCorridorConnections(room, position, scale, corridorWidth, centerOffset);
        }

        private void DrawCorridorCenter(Room room, Vector2 position, float scale, int corridorWidth, int centerOffset)
        {
            _spriteBatch.Draw(_roomTexture,
                new Rectangle(
                    (int)position.X + centerOffset,
                    (int)position.Y + centerOffset,
                    corridorWidth,
                    corridorWidth),
                GameSettings.Graphics.RoomColors.Corridor);
        }

        private void DrawCorridorConnections(Room room, Vector2 position, float scale, int corridorWidth, int centerOffset)
        {
            int size = (int)(GameSettings.Graphics.RoomSize * scale);

            if (room.HasConnection(GameSettings.Directions.Up))
            {
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle(
                        (int)position.X + centerOffset,
                        (int)position.Y,
                        corridorWidth,
                        centerOffset + corridorWidth / 2),
                    GameSettings.Graphics.RoomColors.Corridor);
            }

            if (room.HasConnection(GameSettings.Directions.Right))
            {
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle(
                        (int)position.X + centerOffset + corridorWidth / 2,
                        (int)position.Y + centerOffset,
                        size - (centerOffset + corridorWidth / 2),
                        corridorWidth),
                    GameSettings.Graphics.RoomColors.Corridor);
            }

            if (room.HasConnection(GameSettings.Directions.Down))
            {
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle(
                        (int)position.X + centerOffset,
                        (int)position.Y + centerOffset + corridorWidth / 2,
                        corridorWidth,
                        size - (centerOffset + corridorWidth / 2)),
                    GameSettings.Graphics.RoomColors.Corridor);
            }

            if (room.HasConnection(GameSettings.Directions.Left))
            {
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle(
                        (int)position.X,
                        (int)position.Y + centerOffset,
                        centerOffset + corridorWidth / 2,
                        corridorWidth),
                    GameSettings.Graphics.RoomColors.Corridor);
            }
        }

        private void DrawCorridorBorders(Room room, Vector2 position, float scale)
        {
            int size = (int)(GameSettings.Graphics.RoomSize * scale);
            int corridorWidth = (int)(GameSettings.Graphics.RoomSize * scale * GameSettings.Graphics.Corridor.WidthRatio);
            int centerOffset = (size - corridorWidth) / 2;

            DrawCorridorBorderUp(room, position, corridorWidth, centerOffset, GameSettings.Debug.CorridorBorder.Thickness);
            DrawCorridorBorderRight(room, position, size, corridorWidth, centerOffset, GameSettings.Debug.CorridorBorder.Thickness);
            DrawCorridorBorderDown(room, position, size, corridorWidth, centerOffset, GameSettings.Debug.CorridorBorder.Thickness);
            DrawCorridorBorderLeft(room, position, corridorWidth, centerOffset, GameSettings.Debug.CorridorBorder.Thickness);
        }

        private void DrawCorridorBorderUp(Room room, Vector2 position, int corridorWidth, int centerOffset, int borderThickness)
        {
            if (!room.HasConnection(GameSettings.Directions.Up))
            {
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle((int)position.X + centerOffset, (int)position.Y + centerOffset,
                        corridorWidth, borderThickness),
                    GameSettings.Debug.CorridorBorder.Color);
            }
            else
            {
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle((int)position.X + centerOffset, (int)position.Y,
                        borderThickness, centerOffset),
                    GameSettings.Debug.CorridorBorder.Color);
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle((int)position.X + centerOffset + corridorWidth - borderThickness, (int)position.Y,
                        borderThickness, centerOffset),
                    GameSettings.Debug.CorridorBorder.Color);
            }
        }

        private void DrawCorridorBorderRight(Room room, Vector2 position, int size, int corridorWidth, int centerOffset, int borderThickness)
        {
            if (!room.HasConnection(GameSettings.Directions.Right))
            {
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle((int)position.X + centerOffset + corridorWidth - borderThickness, (int)position.Y + centerOffset,
                        borderThickness, corridorWidth),
                    GameSettings.Debug.CorridorBorder.Color);
            }
            else
            {
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle((int)position.X + centerOffset + corridorWidth, (int)position.Y + centerOffset,
                        size - (centerOffset + corridorWidth) - borderThickness, borderThickness),
                    GameSettings.Debug.CorridorBorder.Color);
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle((int)position.X + centerOffset + corridorWidth, (int)position.Y + centerOffset + corridorWidth - borderThickness,
                        size - (centerOffset + corridorWidth) - borderThickness, borderThickness),
                    GameSettings.Debug.CorridorBorder.Color);
            }
        }

        private void DrawCorridorBorderDown(Room room, Vector2 position, int size, int corridorWidth, int centerOffset, int borderThickness)
        {
            if (!room.HasConnection(GameSettings.Directions.Down))
            {
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle((int)position.X + centerOffset, (int)position.Y + centerOffset + corridorWidth - borderThickness,
                        corridorWidth, borderThickness),
                    GameSettings.Debug.CorridorBorder.Color);
            }
            else
            {
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle((int)position.X + centerOffset, (int)position.Y + centerOffset + corridorWidth,
                        borderThickness, size - (centerOffset + corridorWidth)),
                    GameSettings.Debug.CorridorBorder.Color);
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle((int)position.X + centerOffset + corridorWidth - borderThickness, (int)position.Y + centerOffset + corridorWidth,
                        borderThickness, size - (centerOffset + corridorWidth)),
                    GameSettings.Debug.CorridorBorder.Color);
            }
        }

        private void DrawCorridorBorderLeft(Room room, Vector2 position, int corridorWidth, int centerOffset, int borderThickness)
        {
            if (!room.HasConnection(GameSettings.Directions.Left))
            {
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle((int)position.X + centerOffset, (int)position.Y + centerOffset,
                        borderThickness, corridorWidth),
                    GameSettings.Debug.CorridorBorder.Color);
            }
            else
            {
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle((int)position.X + borderThickness, (int)position.Y + centerOffset,
                        centerOffset - borderThickness, borderThickness),
                    GameSettings.Debug.CorridorBorder.Color);
                _spriteBatch.Draw(_roomTexture,
                    new Rectangle((int)position.X + borderThickness, (int)position.Y + centerOffset + corridorWidth - borderThickness,
                        centerOffset - borderThickness, borderThickness),
                    GameSettings.Debug.CorridorBorder.Color);
            }
        }
    }
}