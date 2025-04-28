using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DungeonFlux.Model;
using DungeonFlux.View;
using DungeonFlux.Controller;
using System;

namespace DungeonFlux;

public class DungeonFluxGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    private GameModel _model;
    private GameView _view;
    private GameController _controller;

    private Player _playerModel;
    private PlayerController _playerController;
    private PlayerView _playerView;
    private Texture2D _playerTexture;

    private KeyboardState _previousKeyboardState;

    public DungeonFluxGame()
    {
        Console.WriteLine("Initializing DungeonFluxGame");
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        _graphics.IsFullScreen = true;
        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        Console.WriteLine($"Setting screen size to: {_graphics.PreferredBackBufferWidth}x{_graphics.PreferredBackBufferHeight}");
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        try
        {
            _model = new GameModel();
            _controller = new GameController(_model);
            _playerModel = new Player(_model.PlayerPosition);
            _playerController = new PlayerController(_playerModel);
            base.Initialize();
        }
        catch
        {
            throw; // TODO: это надо было для отладки. Но я боюсь уберать - вдруг опять крашиться будет. Коммитну а потом когда-нибудь удалю.
        }
    }

    protected override void LoadContent()
    {
        try
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _view = new GameView(_model, _spriteBatch);

            _playerTexture = Content.Load<Texture2D>("David"); // TODO: вынести в гейм сеттингс скин игрока

            _playerView = new PlayerView(_playerModel, _playerTexture, _view);
        }
        catch
        {
            throw;
        }
    }

    protected override void Update(GameTime gameTime)
    {
        try
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(GameSettings.Debug.DebugToggleKey) && 
                !_previousKeyboardState.IsKeyDown(GameSettings.Debug.DebugToggleKey))
            {
                GameSettings.Debug.IsDebugModeEnabled = !GameSettings.Debug.IsDebugModeEnabled;
            }
            _previousKeyboardState = keyboardState;

            _controller.Update();
            _playerController.HandleInput(gameTime);
            _view.UpdateCamera(_playerModel.Position);
            base.Update(gameTime);
        }
        catch
        {
            throw;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        try
        {
            GraphicsDevice.Clear(GameSettings.Graphics.BackgroundColor);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            
            _view.Draw(gameTime);
            _playerView.Draw(_spriteBatch);
            
            _spriteBatch.End();

            base.Draw(gameTime);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Draw: {ex}");
            throw;
        }
    }
}