using AiurVersionControl.CRUD;
using AiurVersionControl.SampleWPF.Models;
using AiurVersionControl.SampleWPF.ViewModels.MVVM;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AiurVersionControl.SampleWPF.Components
{
    internal sealed partial class BooksCRUDPresenter
    {
        private readonly RelayCommand<object> _commitAddNew;
        private readonly RelayCommand<object> _commitDrop;
        private readonly AsyncRelayCommand<object> _hostServer;

        public ICommand CommitAddNew => _commitAddNew;
        public ICommand CommitDrop => _commitDrop;
        public ICommand HostServerCommand => _hostServer;

        public BooksCRUDPresenter(CollectionRepository<Book> repo)
        {
            _commitAddNew = new RelayCommand<object>(Add, _ => !string.IsNullOrWhiteSpace(NewTitle));
            _commitDrop = new RelayCommand<object>(Drop, _ => SelectedBook != null);
            _hostServer = new AsyncRelayCommand<object>(HostServer, _ => true);
            Repository = repo;
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
                await _host.StopAsync();
                _host.Dispose();
                _host = null;
                ServerButtonText = "Host new server";
                ServerGridVisiable = false;
            }
        }

        public void Add(object _)
        {
            Repository.Add(new Book
            {
                Title = NewTitle,
                Id = _counter.GetUniqueNo()
            });

            NewTitle = string.Empty;
        }

        public void Drop(object _)
        {
            Repository.Drop(nameof(Book.Id), SelectedBook.Id);
        }
    }
}
