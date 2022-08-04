using AiurVersionControl.CRUD;
using AiurVersionControl.SampleWPF.Libraries;
using AiurVersionControl.SampleWPF.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace AiurVersionControl.SampleWPF.Components
{
    internal sealed class BooksCRUDPresenter : Presenter
    {
        private readonly RelayCommand<object> _commitAddNew;
        private string _newTitle = string.Empty;

        public ICommand CommitAddNew => _commitAddNew;

        public CollectionRepository<Book> Repository { get; set; }

        public IEnumerable<BookListItem> Books => Repository.Select(b => new BookListItem
        {
            DataContext = new BookListItemPresenter(b, onPatch: newTitle =>
            {
                Repository.Patch(nameof(Book.Id), b.Id, nameof(Book.Title), newTitle);
            }, onDrop: _ =>
            {
                Repository.Drop(nameof(Book.Id), b.Id);
                OnPropertyChanged(nameof(Books));
            })
        });

        public BooksCRUDPresenter(CollectionRepository<Book> repo)
        {
            _commitAddNew = new RelayCommand<object>(Add, _ => !string.IsNullOrWhiteSpace(NewTitle));
            Repository = repo;
            Repository.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(Books));
            };
        }

        public string NewTitle
        {
            get => _newTitle;
            set
            {
                Update(ref _newTitle, value);
                _commitAddNew.RaiseCanExecuteChanged();
            }
        }

        public void Add(object _)
        {
            Repository.Add(new Book
            {
                Title = NewTitle
            });
            OnPropertyChanged();
            NewTitle = string.Empty;
        }
    }
}