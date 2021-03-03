namespace AiurEventSyncer.Abstract
{
    public interface IRemote<T>
    {
        public Commit<T> PullPointer { get; set; }
        public Commit<T> PushPointer { get; set; }
        void OnPullPointerMovedForwardOnce(Commit<T> pointer);
        void OnPullInsert();
    }
}
