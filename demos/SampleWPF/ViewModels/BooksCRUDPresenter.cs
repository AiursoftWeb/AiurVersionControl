using AiurEventSyncer.Abstract;
using AiurStore.Models;
using AiurVersionControl.CRUD;
using AiurVersionControl.Models;
using AiurVersionControl.SampleWPF.Models;
using AiurVersionControl.SampleWPF.Services;
using AiurVersionControl.SampleWPF.ViewModels.MVVM;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;

namespace AiurVersionControl.SampleWPF.ViewModels
{
    internal sealed partial class BooksCRUDPresenter : Presenter, INotifyPropertyChanged
    {
        private string _newTitle = string.Empty;
        private string _buttonText = "Host new server";
        private bool _serverGridVisiable = false;
        private Book _selectedBook;
        private Counter _counter = new();
        private IHost _host;

        public CollectionRepository<Book> Repository { get; set; }

        public IOutOnlyDatabase<Commit<IModification<CollectionWorkSpace<Book>>>> History => Repository.Commits;

        public Book SelectedBook
        {
            get => _selectedBook;
            set
            {
                Update(ref _selectedBook, value, nameof(SelectedBook));
                _commitDrop.RaiseCanExecuteChanged();
            }
        }

        public string NewTitle
        {
            get => _newTitle;
            set
            {
                Update(ref _newTitle, value, nameof(NewTitle));
                _commitAddNew.RaiseCanExecuteChanged();
            }
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
    }
}