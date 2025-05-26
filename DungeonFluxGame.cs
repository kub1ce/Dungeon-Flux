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
    private Texture2D _enemyTexture;
    private Texture2D _coinTexture;
    private Texture2D _aidTexture;

    private KeyboardState _previousKeyboardState;
    private MouseState _previousMouseState;
    private SpriteFont _menuFont;
    private MenuState _menuState;
    private bool _isInMenu = true;

    public DungeonFluxGame()
    {
        Logger.Log("Initializing DungeonFluxGame");
        InitializeGraphics();
    }

    private void InitializeGraphics()
    {
        try
        {
            Logger.Log("Initializing graphics device...");
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            Logger.Log("Setting fullscreen mode...");
            _graphics.IsFullScreen = true;
            
            var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            Logger.Log($"Current display mode: {displayMode.Width}x{displayMode.Height}");
            
            _graphics.PreferredBackBufferWidth = displayMode.Width;
            _graphics.PreferredBackBufferHeight = displayMode.Height;
            Logger.Log($"Setting screen size to: {_graphics.PreferredBackBufferWidth}x{_graphics.PreferredBackBufferHeight}");
            
            Logger.Log("Applying graphics changes...");
            _graphics.ApplyChanges();
            Logger.Log("Graphics initialization completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError("Error during graphics initialization", ex);
            throw;
        }
    }

    protected override void Initialize()
    {
        try
        {
            Logger.Log("Starting game initialization...");
            base.Initialize();
            Logger.Log("Base initialization completed");
        }
        catch (Exception ex)
        {
            Logger.LogError("Error during game initialization", ex);
            throw;
        }
    }

    protected override void LoadContent()
    {
        try
        {
            Logger.Log("Starting content loading...");
            InitializeSpriteBatch();
            InitializeMenu();
            Logger.Log("Content loading completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError("Error in LoadContent", ex);
            throw;
        }
    }

    private void InitializeSpriteBatch()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    private void InitializeMenu()
    {
        try
        {
            Logger.Log("Starting menu initialization...");
            Logger.Log("Loading MenuFont...");
            _menuFont = Content.Load<SpriteFont>("MenuFont");
            if (_menuFont == null)
            {
                throw new Exception("Failed to load MenuFont - font is null");
            }
            Logger.Log("MenuFont loaded successfully");
            
            Logger.Log("Creating MenuState...");
            _menuState = new MenuState(_menuFont);
            Logger.Log("MenuState initialized successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError("Error during menu initialization", ex);
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

        try
        {
            Logger.Log("Initializing game...");
            LoadGameTextures();
            InitializeGameComponents();
            InitializeViews();
            Logger.Log("Game initialized successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError("Error during game initialization", ex);
            throw;
        }
    }

    private void LoadGameTextures()
    {
        try
        {
            Logger.Log("Loading game textures...");
            
            Logger.Log("Loading player texture (David)...");
            _playerTexture = Content.Load<Texture2D>("David");
            if (_playerTexture == null) throw new Exception("Failed to load player texture");
            Logger.Log("Player texture loaded successfully");
            
            Logger.Log("Loading enemy texture (Timarokk)...");
            _enemyTexture = Content.Load<Texture2D>("Timarokk");
            if (_enemyTexture == null) throw new Exception("Failed to load enemy texture");
            Logger.Log("Enemy texture loaded successfully");
            
            Logger.Log("Loading coin texture...");
            _coinTexture = Content.Load<Texture2D>("coin");
            if (_coinTexture == null) throw new Exception("Failed to load coin texture");
            Logger.Log("Coin texture loaded successfully");
            
            Logger.Log("Loading aid texture...");
            _aidTexture = Content.Load<Texture2D>("aid");
            if (_aidTexture == null) throw new Exception("Failed to load aid texture");
            Logger.Log("Aid texture loaded successfully");
            
            Logger.Log("All game textures loaded successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError("Error loading game textures", ex);
            throw;
        }
    }

    private void InitializeGameComponents()
    {
        _model = new GameModel();
        _controller = new GameController(_model);
        _playerModel = new Player(_model.PlayerPosition, _model);
        _model.SetPlayer(_playerModel);
        _playerController = new PlayerController(_playerModel, _model, GraphicsDevice, this);
    }

    private void InitializeViews()
    {
        _view = new GameView(_model, _spriteBatch, _menuFont, _playerModel, _enemyTexture, _coinTexture, _aidTexture, this);
        _playerView = new PlayerView(_playerModel, _playerTexture, _view, Content);
    }

    public void ReturnToMenu()
    {
        _isInMenu = true;
        _model = null;
        _view = null;
        _controller = null;
        _playerModel = null;
        _playerController = null;
        _playerView = null;
    }

    protected override void Update(GameTime gameTime)
    {
        try
        {
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            if (_isInMenu)
            {
                UpdateMenu(mouseState, gameTime);
            }
            else
            {
                UpdateGame(keyboardState, gameTime);
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

    private void UpdateMenu(MouseState mouseState, GameTime gameTime)
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

    private void UpdateGame(KeyboardState keyboardState, GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            keyboardState.IsKeyDown(Keys.Escape))
        {
            ReturnToMenu();
            return;
        }

        UpdateDebugMode(keyboardState);
        UpdateGameComponents(gameTime);
    }

    private void UpdateDebugMode(KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(GameSettings.Debug.DebugToggleKey) && 
            !_previousKeyboardState.IsKeyDown(GameSettings.Debug.DebugToggleKey))
        {
            GameSettings.Debug.IsDebugModeEnabled = !GameSettings.Debug.IsDebugModeEnabled;
        }
    }

    private void UpdateGameComponents(GameTime gameTime)
    {
        if (_playerModel != null && _playerModel.IsAlive())
        {
            _controller.Update();
            _playerModel.Update(gameTime);
            _playerController.Update(gameTime);
            _model.Update(gameTime);
            _view.UpdateCamera(_playerModel.Position);
        }

        if (_view != null)
        {
            _view.Update();
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
                DrawGame(gameTime);
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

    private void DrawGame(GameTime gameTime)
    {
        _view.Draw(gameTime);
        if (_playerModel != null && _playerModel.IsAlive())
        {
            _playerView.Draw(_spriteBatch);
        }
    }
}