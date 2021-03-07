using AiurVersionControl.CRUD;
using AiurVersionControl.Remotes;
using AiurVersionControl.SampleWPF.Models;
using AiurVersionControl.SampleWPF.ViewModels.MVVM;
using System;
using System.ComponentModel;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Windows;
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

        public async Task AttachToAServer(object _)
        {
            try
            {
                var remote = new WebSocketRemoteWithWorkSpace<CollectionWorkSpace<Book>>(ServerAddress);
                await remote.AttachAsync(_repo);
            }
            catch (UriFormatException e)
            {
                MessageBox.Show(
                    $"Invalid WebSocket URI! {e.Message}",
                    "Attach to a remote server",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch(WebSocketException e)
            {
                MessageBox.Show(
                    $"Invalid server response! {e.Message} Please make sure the remote address is a valid server!",
                    "Attach to a remote server",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch(Exception e)
            {
                MessageBox.Show(
                    e.Message,
                    "Attach to a remote server",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
