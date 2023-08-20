using Aiursoft.SnakeGame.Launcher;
using Aiursoft.SnakeGame.Services;

namespace Aiursoft.SnakeGame
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            // Game with Observer
            await Render.StartGameWithObserver(
                new Game(Constants.GridSize, 0),
                new Game(Constants.GridSize, Constants.GridSize + Constants.GridSize / 2));
            
            // Game without observer.
            // await Render.StartGame(new Game(Constants.GridSize, 0));
        }
    }
}
