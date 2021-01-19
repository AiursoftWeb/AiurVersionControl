using AiurEventSyncer.ConnectionProviders;
using AiurEventSyncer.Models;

namespace AiurEventSyncer.Remotes
{
    public class ObjectRemote<T> : Remote<T>
    {
        public ObjectRemote(Repository<T> localRepository, bool autoPush = false, bool autoPull = false)
            :base (new FakeConnection<T>(localRepository), autoPush, autoPull)
        {

        }
    }
}
