using System.Collections.Generic;
using System.Linq;
using AiurEventSyncer.Models;
using AiurEventSyncer.Tools;
using SnakeGame.Models;

namespace SnakeGame.Services.Implements
{
    public class SnakeRecurrent : IRecurrent<Snake, Position>
    {
        public Snake Recurrent(Snake snake, Repository<Position> repo)
        {
            var commits = repo.Commits;
            return doRecurrent(snake, commits);
        }

        public Snake RecurrentFromId(Snake snake, Repository<Position> repo, string position = null)
        {
            var commits = repo.Commits.AfterCommitId(position);
            return doRecurrent(snake, commits);
        }

        private Snake doRecurrent(Snake snake, IEnumerable<Commit<Position>> commits)
        {
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