namespace AiurEventSyncer.Abstract
{
    public interface IRemote<T>
    {
        public string PullPointer { get; set; }
        public string PushPointer { get; set; }
    }
}
