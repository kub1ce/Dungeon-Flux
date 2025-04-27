using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using DungeonFlux.Model;

namespace DungeonFlux.Controller
{
    public class GameController
    {
        private readonly GameModel _model;
        private KeyboardState _previousKeyboardState;

        public GameController(GameModel model)
        {
            _model = model;
            _previousKeyboardState = Keyboard.GetState();
        }

        public void Update()
        {
            var currentKeyboardState = Keyboard.GetState();

            if (currentKeyboardState.IsKeyDown(Keys.W) && !_previousKeyboardState.IsKeyDown(Keys.W))
                _model.TryMovePlayer(0); // Up
            if (currentKeyboardState.IsKeyDown(Keys.D) && !_previousKeyboardState.IsKeyDown(Keys.D))
                _model.TryMovePlayer(1); // Right
            if (currentKeyboardState.IsKeyDown(Keys.S) && !_previousKeyboardState.IsKeyDown(Keys.S))
                _model.TryMovePlayer(2); // Down
            if (currentKeyboardState.IsKeyDown(Keys.A) && !_previousKeyboardState.IsKeyDown(Keys.A))
                _model.TryMovePlayer(3); // Left
            // TODO: мб можно как-то организовать промерку предыдущей кбд заранее на непохожесть с предыдущей
            // TODO: и вот эти изкейдаун сократятсяы

            _previousKeyboardState = currentKeyboardState;
        }
    }
}