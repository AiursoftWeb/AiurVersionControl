using AiurEventSyncer.Models;

namespace SnakeGame.Services
{
    public interface IRecurrent<T, R>
    {
        /// <summary>
        /// Recurrent the commits in local repository on t.
        /// </summary>
        /// <param name="t">Model</param>
        /// <param name="r">Local repository</param>
        /// <returns>Model after recurrent</returns>
        T Recurrent(T t, Repository<R> r, int offset = 0);

        T RecurrentFromId(T t, Repository<R> r, string id = null);
    }
}