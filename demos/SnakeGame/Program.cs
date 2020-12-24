using SnakeGame.Launcher;
using SnakeGame.Services;
using System.Threading.Tasks;

namespace SnakeGame
{
    class Program
    {
        static async Task Main(string[] args)
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
