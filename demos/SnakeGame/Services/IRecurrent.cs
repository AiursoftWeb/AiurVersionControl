using AiurEventSyncer.Models;

namespace SnakeGame.Services
{
    public interface IRecurrent<T, TR>
    {
        T Recurrent(T t, Repository<TR> r, int offset = 0);

        T RecurrentFromId(T t, Repository<TR> r, string id = null);
    }
}