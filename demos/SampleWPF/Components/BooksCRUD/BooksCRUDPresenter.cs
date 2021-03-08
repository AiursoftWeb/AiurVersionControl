using AiurVersionControl.CRUD;
using AiurVersionControl.SampleWPF.Models;
using AiurVersionControl.SampleWPF.Services;
using AiurVersionControl.SampleWPF.ViewModels.MVVM;
using System.ComponentModel;
using System.Windows.Input;

namespace AiurVersionControl.SampleWPF.Components
{
    internal sealed partial class BooksCRUDPresenter : Presenter, INotifyPropertyChanged
    {
        private readonly RelayCommand<object> _commitAddNew;
        private readonly RelayCommand<object> _commitDrop;
        private string _newTitle = string.Empty;
        private Book _selectedBook;

        public ICommand CommitAddNew => _commitAddNew;
        public ICommand CommitDrop => _commitDrop;

        public CollectionRepository<Book> Repository { get; set; }

        public BooksCRUDPresenter(CollectionRepository<Book> repo)
        {
            _commitAddNew = new RelayCommand<object>(Add, _ => !string.IsNullOrWhiteSpace(NewTitle));
            _commitDrop = new RelayCommand<object>(Drop, _ => SelectedBook != null);
            Repository = repo;
        }

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

        public void Add(object _)
        {
            Repository.Add(new Book
            {
                Title = NewTitle
            });

            NewTitle = string.Empty;
        }

        public void Drop(object _)
        {
            Repository.Drop(nameof(Book.Id), SelectedBook.Id);
        }
    }
}