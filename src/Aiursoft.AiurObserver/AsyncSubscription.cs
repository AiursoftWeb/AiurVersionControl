namespace AiurObserver
{
    public class AsyncSubscription : ISubscription
    {
        private readonly Action _unRegisterAction;

        internal AsyncSubscription(Action unRegisterAction)
        {
            _unRegisterAction = unRegisterAction;
        }

        public void UnRegister()
        {
            _unRegisterAction();
        }
    }
}
