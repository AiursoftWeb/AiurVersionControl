using AiurEventSyncer.ConnectionProviders;
using AiurVersionControl.Models;
using AiurVersionControl.Remotes;
using AiurVersionControl.SampleWPF.Libraries;
using AiurVersionControl.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AiurVersionControl.SampleWPF.Components
{
    internal sealed class RemoteControlPresenter : Presenter
    {
        private readonly AsyncRelayCommand<object> _detach;
        public ICommand DetachIt => _detach;
        public WebSocketRemoteWithWorkSpace<TextWorkSpace> Remote { get; set; }

        public RetryableWebSocketConnection<IModification<TextWorkSpace>> Connection => Remote.ConnectionProvider as RetryableWebSocketConnection<IModification<TextWorkSpace>>;

        public RemoteControlPresenter(WebSocketRemoteWithWorkSpace<TextWorkSpace> remote)
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
