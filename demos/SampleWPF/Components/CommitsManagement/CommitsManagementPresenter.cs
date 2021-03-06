using AiurEventSyncer.Abstract;
using AiurStore.Models;
using AiurVersionControl.CRUD;
using AiurVersionControl.Models;
using AiurVersionControl.SampleWPF.Models;
using AiurVersionControl.SampleWPF.Services;
using AiurVersionControl.SampleWPF.ViewModels.MVVM;
using Microsoft.Extensions.Hosting;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AiurVersionControl.SampleWPF.Components
{
    internal sealed class CommitsManagementPresenter : Presenter, INotifyPropertyChanged
    {
        private readonly AsyncRelayCommand<object> _hostServer;
        private readonly CollectionRepository<Book> _repository;
        private string _buttonText = "Host new server";
        private bool _serverGridVisiable = false;
        private IHost _host;

        public ICommand HostServerCommand => _hostServer;

        public IOutOnlyDatabase<Commit<IModification<CollectionWorkSpace<Book>>>> History => _repository.Commits;

        public CommitsManagementPresenter(CollectionRepository<Book> repo)
        {
            _repository = repo;
            _hostServer = new AsyncRelayCommand<object>(HostServer, _ => true);
        }

        public string ServerButtonText
        {
            get => _buttonText;
            set
            {
                Update(ref _buttonText, value, nameof(ServerButtonText));
            }
        }

        public bool ServerGridVisiable
        {
            get => _serverGridVisiable;
            set
            {
                Update(ref _serverGridVisiable, value, nameof(ServerGridVisiable));
            }
        }

        public async Task HostServer(object _)
        {
            if (_host == null)
            {
                _host = ServerProgram.BuildHost(Array.Empty<string>(), 15678);
                await _host.StartAsync();
                ServerButtonText = "Stop server";
                ServerGridVisiable = true;
            }
            else
            {
                await StopServer();
            }
        }

        public async Task StopServer()
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
            _host = null;
            ServerButtonText = "Host new server";
            ServerGridVisiable = false;
        }
    }
}
