using AiurEventSyncer.Models;
using SnakeGame.Models;

namespace SnakeGame.Services.Implements
{
    public class SnakeRecurrent : IRecurrent<Snake, Position>
    {
        public Snake Recurrent(Snake snake, Repository<Position> repo)
        {
            var commits = repo.Commits;
            foreach (var commit in commits)
            {
                // if snake move.
                snake.Update(commit.Item);
                // if snake eat.
            }

            return snake;
        }
    }
}