using AiurVersionControl.CRUD;
using AiurVersionControl.Remotes;
using AiurVersionControl.SampleWPF.Models;
using AiurVersionControl.SampleWPF.ViewModels.MVVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AiurVersionControl.SampleWPF.Components
{
    internal sealed class RemoteManagementPresenter : Presenter, INotifyPropertyChanged
    {
        private string _serverAddress = string.Empty;
        private readonly AsyncRelayCommand<object> _attach;
        private readonly CollectionRepository<Book> _repo;
        public ICommand Attach => _attach;
        public string ServerAddress
        {
            get => _serverAddress;
            set
            {
                Update(ref _serverAddress, value, nameof(ServerAddress));
                _attach.RaiseCanExecuteChanged();
            }
        }

        public RemoteManagementPresenter(CollectionRepository<Book> repo)
        {
            _attach = new AsyncRelayCommand<object>(AttachToAServer, _ => !string.IsNullOrWhiteSpace(ServerAddress));
            _repo = repo;
        }

        public Task AttachToAServer(object _)
        {
            var remote = new WebSocketRemoteWithWorkSpace<CollectionWorkSpace<Book>>(ServerAddress);
            return remote.AttachAsync(_repo);
        }
    }
}
