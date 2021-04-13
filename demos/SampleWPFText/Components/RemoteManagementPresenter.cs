using AiurVersionControl.Remotes;
using AiurVersionControl.SampleWPF.Libraries;
using AiurVersionControl.Text;
using System;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AiurVersionControl.SampleWPF.Components
{
    internal sealed class RemoteManagementPresenter : Presenter
    {
        private string _serverAddress = string.Empty;
        private readonly AsyncRelayCommand<object> _attach;
        private readonly TextRepository _repo;
        public ObservableCollection<RemoteControl> Remotes { get; set; } = new();
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

        public RemoteManagementPresenter(TextRepository repo)
        {
            _attach = new AsyncRelayCommand<object>(AttachToAServer, _ => !string.IsNullOrWhiteSpace(ServerAddress));
            _repo = repo;
        }

        public async Task AttachToAServer(object _)
        {
            RemoteControl control = null;
            try
            {
                var remote = new WebSocketRemoteWithWorkSpace<TextWorkSpace>(ServerAddress, autoRetry: true);
                control = new RemoteControl
                {
                    DataContext = new RemoteControlPresenter(remote)
                };
                Remotes.Add(control);
                await remote.AttachAsync(_repo, monitorInCurrentThread: true);
            }
            catch (UriFormatException e)
            {
                MessageBox.Show(
                    $"Invalid WebSocket URI! {e.Message}",
                    "Attach to a remote server",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (WebSocketException)
            {
                MessageBox.Show(
                    "Server detached!",
                    "Remote server",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    e.Message,
                    "Attach to a remote server",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw;
            }
            finally
            {
                Remotes.Remove(control);
            }
        }
    }
}
