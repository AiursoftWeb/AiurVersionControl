namespace SnakeGame.Models
{
    public struct Action
    {
        public ActionType Type { get; set; }

        public Position Direction { get; set; }
    }
}