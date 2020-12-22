using System;
using System.Collections.Generic;
using AiurEventSyncer.Models;
using AiurEventSyncer.Tools;
using Action = SnakeGame.Models.Action;

namespace SnakeGame.Services.Implements
{
    public class SnakeRecurrent : IRecurrent<Snake, Action>
    {
        public Snake Recurrent(Snake snake, Repository<Action> repo, int offset = 0)
        {
            var commits = repo.Commits;
            return doRecurrent(snake, commits, offset);
        }

        public Snake RecurrentFromId(Snake snake, Repository<Action> repo, string position = null)
        {
            var commits = repo.Commits.AfterCommitId(position);
            return doRecurrent(snake, commits);
        }

        private Snake doRecurrent(Snake snake, IEnumerable<Commit<Action>> commits, int offset = 0)
        {
            foreach (var commit in commits)
            {
                switch (commit.Item.Type)
                {
                    case ActionType.Move:
                        snake.Update(commit.Item.Direction);
                        break;
                    case ActionType.Eat:
                        snake.AddBody();
                        
                        // Draw next food
                        Console.SetCursorPosition(commit.Item.Direction.X + offset, commit.Item.Direction.Y);
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write("█");
                        
                        break;
                }
            }

            return snake;
        }
    }
}