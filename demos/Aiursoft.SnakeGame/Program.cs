using System.Threading.Tasks;
using Aiursoft.SnakeGame.Launcher;
using Aiursoft.SnakeGame.Services;

namespace Aiursoft.SnakeGame
{
    class Program
    {
        static async Task Main()
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
