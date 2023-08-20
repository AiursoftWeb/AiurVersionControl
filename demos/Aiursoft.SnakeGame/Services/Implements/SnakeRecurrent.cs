using Aiursoft.AiurEventSyncer.Abstract;
using Aiursoft.AiurEventSyncer.Models;
using Aiursoft.AiurEventSyncer.Tools;
using System;
using System.Collections.Generic;
using Aiursoft.SnakeGame.Models;

namespace Aiursoft.SnakeGame.Services.Implements
{
    public class SnakeRecurrent : IRecurrent<Snake, Models.Action>
    {
        public Snake Recurrent(Snake snake, Repository<Models.Action> repo, int offset = 0)
        {
            var commits = repo.Commits;
            return DoRecurrent(snake, commits, offset);
        }

        public Snake RecurrentFromId(Snake snake, Repository<Models.Action> repo, string position = null)
        {
            var commits = repo.Commits.GetCommitsAfterId<Commit<Models.Action>, Models.Action>(position);
            return DoRecurrent(snake, commits);
        }

        private static Snake DoRecurrent(Snake snake, IEnumerable<Commit<Models.Action>> commits, int offset = 0)
        {
            Position foodPosition = new Position{ X = 0, Y = 0 };
            foreach (var commit in commits)
            {
                switch (commit.Item.Type)
                {
                    case ActionType.Move:
                        snake.Update(commit.Item.Direction);
                        break;
                    case ActionType.Eat:
                        snake.AddBody();
                        foodPosition.X = commit.Item.Direction.X + offset;
                        foodPosition.Y = commit.Item.Direction.Y;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (foodPosition.X != 0 && foodPosition.Y != 0)
            {
                GameObject.Draw(foodPosition, ConsoleColor.DarkRed);
            }

            return snake;
        }
    }
}