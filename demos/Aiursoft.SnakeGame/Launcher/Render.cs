using Aiursoft.SnakeGame.Services;

namespace Aiursoft.SnakeGame.Launcher
{
    public static class Render
    {
        private static decimal _gameSpeed = Constants.InitialSpeed;

        public static async Task StartGame(Game game)
        {
            // Add Remote Repository
            await game.AddRemote(Constants.EndPointUrl);
            
            Console.Clear();
            
            // Draw Items
            game.Draw();
            
            while (!game.IsGameEnd)
            {
                game.UpdateDirection();
                game.UpdateFrame();
                if (game.NeedSpeedUp())
                {
                    _gameSpeed *= 0.95m;
                }
                
                // Listening input command
                game.ListenInput();

                await Task.Delay((int)_gameSpeed);
            }
        }

        public static async Task StartGameWithObserver(Game game, Game observer)
        {
            // Add Remote Repository
            await game.AddRemote(Constants.EndPointUrl);
            await observer.AddRemote(Constants.EndPointUrl);
            
            observer.GenerateRecurrent();
            
            Console.Clear();
            
            // Draw Items
            game.Draw();
            observer.Draw();

            // Wait the input to start the game.
            game.ListenInput(true);

            while (!game.IsGameEnd)
            {
                // Player's panel
                game.UpdateDirection();
                game.UpdateFrame();
                if (game.NeedSpeedUp())
                {
                    _gameSpeed *= 0.95m;
                }
                
                // Observer's panel
                observer.RecurrentFromRepo();
                observer.UpdateFrame();
                
                // Listening input command
                game.ListenInput();
                
                await Task.Delay(Convert.ToInt32(_gameSpeed));
            }

            while (!observer.IsGameEnd)
            {
                observer.RecurrentFromRepo();
                observer.UpdateFrame();
            }
        }
    }
}