using Aiursoft.AiurEventSyncer.ConnectionProviders;
using Aiursoft.AiurVersionControl.CRUD;
using Aiursoft.AiurVersionControl.Models;
using Aiursoft.AiurVersionControl.Remotes;
using Aiursoft.AiurVersionControl.SampleWPF.Libraries;
using Aiursoft.AiurVersionControl.SampleWPF.Models;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Aiursoft.AiurVersionControl.SampleWPF.Components
{
    internal sealed class RemoteControlPresenter : Presenter
    {
        private readonly AsyncRelayCommand<object> _detach;
        public ICommand DetachIt => _detach;
        public WebSocketRemoteWithWorkSpace<CollectionWorkSpace<Book>> Remote { get; set; }

        public RetryableWebSocketConnection<IModification<CollectionWorkSpace<Book>>> Connection => Remote.ConnectionProvider as RetryableWebSocketConnection<IModification<CollectionWorkSpace<Book>>>;

        public RemoteControlPresenter(WebSocketRemoteWithWorkSpace<CollectionWorkSpace<Book>> remote)
        {
            _detach = new AsyncRelayCommand<object>(Detach, _ => true);
            Remote = remote;
        }

        public string EndPoint => Remote.EndPoint;

        public async Task Detach(object _)
        {
            await Remote.DetachAsync();
        }
    }
}
