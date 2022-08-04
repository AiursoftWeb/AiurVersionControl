using AiurEventSyncer.Abstract;
using AiurStore.Models;
using AiurVersionControl.CRUD;
using AiurVersionControl.Models;
using AiurVersionControl.SampleWPF.Libraries;
using AiurVersionControl.SampleWPF.Models;
using AiurVersionControl.SampleWPF.Services;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace AiurVersionControl.SampleWPF.Components
{
    internal sealed class CommitsManagementPresenter : Presenter
    {
        private readonly AsyncRelayCommand<object> _hostServer;
        private readonly CollectionRepository<Book> _repository;
        private readonly int _port = Network.GetAvailablePort();
        private string _buttonText = "Host new server";
        private bool _serverGridVisible;
        private IHost _host;

        public bool ServerHosting => _host != null;

        public ICommand HostServerCommand => _hostServer;

        public IOutOnlyDatabase<Commit<IModification<CollectionWorkSpace<Book>>>> History => _repository.Commits;

        public CommitsManagementPresenter(CollectionRepository<Book> repo)
        {
            _repository = repo;
            _hostServer = new AsyncRelayCommand<object>(HostServer, _ => true);
        }

        public string Address => $"ws://localhost:{_port}/repo.ares";

        public string ServerButtonText
        {
            get => _buttonText;
            set
            {
                Update(ref _buttonText, value);
            }
        }

        public bool ServerGridVisiable
        {
            get => _serverGridVisible;
            set
            {
                Update(ref _serverGridVisible, value);
            }
        }

        private async Task HostServer(object _)
        {
            if (_host == null)
            {
                _host = ServerProgram.BuildHost(Array.Empty<string>(), _repository, Dispatcher.CurrentDispatcher, _port);
                await _host.StartAsync();
                ServerButtonText = "Stop server";
                ServerGridVisiable = true;
            }
            else
            {
                await StopServer();
            }
        }

        private async Task StopServer()
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
                _host = null;
            }
            ServerButtonText = "Host new server";
            ServerGridVisiable = false;
        }
    }
}
