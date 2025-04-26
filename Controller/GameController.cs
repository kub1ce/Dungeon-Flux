using DungeonFlux.Model;

namespace DungeonFlux.Controller
{
    public class GameController
    {
        private readonly GameModel _model;

        public GameController(GameModel model)
        {
            _model = model;
        }

        public void Update()
        {
            // Тут будет обработка ввода и изменение модели
        }
    }
}