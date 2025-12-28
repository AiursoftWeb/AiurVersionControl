namespace Aiursoft.AiurEventSyncer.Abstract
{
    public interface IRemote<T>
    {
        public Commit<T> PullPointer { get; set; }
        public Commit<T> PushPointer { get; set; }
        public object StateLock { get; }
        void OnPullPointerMovedForwardOnce(Commit<T> pointer);
        void OnPullInsert();
    }
}
