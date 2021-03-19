using AiurVersionControl.CRUD;
using AiurVersionControl.Remotes;
using AiurVersionControl.SampleWPF.Models;
using AiurVersionControl.SampleWPF.ViewModels.MVVM;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AiurVersionControl.SampleWPF.Components
{
    internal sealed class RemoteControlPresenter : Presenter
    {
        private readonly AsyncRelayCommand<object> _detach;
        public ICommand DetachIt => _detach;
        public WebSocketRemoteWithWorkSpace<CollectionWorkSpace<Book>> Remote { get; set; }
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
