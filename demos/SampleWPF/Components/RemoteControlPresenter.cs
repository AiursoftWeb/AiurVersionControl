﻿using AiurEventSyncer.ConnectionProviders;
using AiurVersionControl.CRUD;
using AiurVersionControl.Models;
using AiurVersionControl.Remotes;
using AiurVersionControl.SampleWPF.Libraries;
using AiurVersionControl.SampleWPF.Models;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AiurVersionControl.SampleWPF.Components
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
