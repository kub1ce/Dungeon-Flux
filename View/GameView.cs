using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DungeonFlux.Model;

namespace DungeonFlux.View
{
    public class GameView
    {
        private readonly GameModel _model;
        private readonly SpriteBatch _spriteBatch;

        public GameView(GameModel model, SpriteBatch spriteBatch)
        {
            _model = model;
            _spriteBatch = spriteBatch;
        }

        public void Draw(GameTime gameTime)
        {
            // Тут пока что пусто :з
        }
    }
}