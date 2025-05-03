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
    private MouseState _previousMouseState;
    private SpriteFont _menuFont;
    private MenuState _menuState;
    private bool _isInMenu = true;

    public DungeonFluxGame()
    {
        Logger.Log("Initializing DungeonFluxGame");
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        _graphics.IsFullScreen = true;
        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        Logger.Log($"Setting screen size to: {_graphics.PreferredBackBufferWidth}x{_graphics.PreferredBackBufferHeight}");
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        try
        {
            base.Initialize();
        }
        catch
        {
            throw;
        }
    }

    protected override void LoadContent()
    {
        try
        {
            Logger.Log("Loading content...");
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Logger.Log("Loading MenuFont...");
            _menuFont = Content.Load<SpriteFont>("MenuFont");
            Logger.Log("MenuFont loaded successfully");
            _menuState = new MenuState(_menuFont);
            Logger.Log("MenuState initialized");
        }
        catch (Exception ex)
        {
            Logger.LogError("Error in LoadContent", ex);
            throw;
        }
    }

    private void InitializeGame()
    {
        if (_menuFont == null)
        {
            Logger.LogError("Font is not loaded yet!");
            return;
        }

        Logger.Log("Initializing game...");
        _model = new GameModel();
        _controller = new GameController(_model);
        _playerModel = new Player(_model.PlayerPosition);
        _playerController = new PlayerController(_playerModel, _model);
        _view = new GameView(_model, _spriteBatch, _menuFont, _playerModel);
        _playerTexture = Content.Load<Texture2D>("David");
        _playerView = new PlayerView(_playerModel, _playerTexture, _view);
        Logger.Log("Game initialized successfully");
    }

    protected override void Update(GameTime gameTime)
    {
        try
        {
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            if (_isInMenu)
            {
                _menuState.Update(mouseState, _previousMouseState, gameTime);

                if (_menuState.IsStartGameClicked())
                {
                    InitializeGame();
                    _isInMenu = false;
                }
                else if (_menuState.IsExitClicked())
                {
                    Exit();
                }
            }
            else
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                    keyboardState.IsKeyDown(Keys.Escape))
                {
                    _isInMenu = true;
                    return;
                }

                if (keyboardState.IsKeyDown(GameSettings.Debug.DebugToggleKey) && 
                    !_previousKeyboardState.IsKeyDown(GameSettings.Debug.DebugToggleKey))
                {
                    GameSettings.Debug.IsDebugModeEnabled = !GameSettings.Debug.IsDebugModeEnabled;
                }

                _controller.Update();
                _playerController.Update(gameTime);
                _view.UpdateCamera(_playerModel.Position);
            }

            _previousKeyboardState = keyboardState;
            _previousMouseState = mouseState;
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
            
            if (_isInMenu)
            {
                _menuState.Draw(_spriteBatch);
            }
            else
            {
                _view.Draw(gameTime);
                _playerView.Draw(_spriteBatch);
            }
            
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