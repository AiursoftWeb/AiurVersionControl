namespace SnakeGame
{
    public sealed class Constants
    {
        /// <summary>
        /// WebSocketRemote repository port.
        /// </summary>
        private const int Port = 15000;
        
        /// <summary>
        /// WebSocketRemote endpoint for connecting.
        /// </summary>
        public static readonly string EndPointUrl = $"ws://localhost:{Port}/repo.ares";
        
        /// <summary>
        /// Game initial refresh rate. (milliseconds)
        /// </summary>
        public static readonly int InitialSpeed = 150;

        /// <summary>
        /// Game initial grid size.
        /// </summary>
        public static readonly int GridSize = 40;
    }
}