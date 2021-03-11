using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AiurVersionControl.CRUD;
using AiurVersionControl.SampleWPF.Models;
using AiurVersionControl.SampleWPF.ViewModels.MVVM;

namespace AiurVersionControl.SampleWPF.Components
{
    internal sealed partial class BooksCRUDPresenter : Presenter, INotifyPropertyChanged
    {
        private readonly RelayCommand<object> _commitAddNew;
        private readonly RelayCommand<object> _commitDrop;
        private readonly RelayCommand<object> _commitEdit;
        private readonly RelayCommand<object> _commitModify;
        private string _newTitle = string.Empty;
        private Book _selectedBook;

        public ICommand CommitAddNew => _commitAddNew;
        public ICommand CommitDrop => _commitDrop;
        public ICommand CommitEdit => _commitEdit;
        public ICommand CommitModify => _commitModify;

        public CollectionRepository<Book> Repository { get; set; }

        public BooksCRUDPresenter(CollectionRepository<Book> repo)
        {
            _commitAddNew = new RelayCommand<object>(Add, _ => !string.IsNullOrWhiteSpace(NewTitle));
            _commitDrop = new RelayCommand<object>(Drop, _ => SelectedBook != null);
            _commitEdit = new RelayCommand<object>(Edit, _=>true);
            _commitModify = new RelayCommand<object>(Modify, _=>!string.IsNullOrWhiteSpace(EditTitle));
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

        private Guid _editingId = default;
        
        public Guid EditingId
        {
            get => _editingId;
            set => Update(ref _editingId, value, nameof(EditingId));
        }

        private void Edit(object id)
        {
            EditingId = !EditingId.Equals((Guid) id) ? (Guid) id : default;
        }

        private string _editTitle = string.Empty;
        public string EditTitle
        {
            get => _editTitle;
            set
            {
                Update(ref _editTitle, value, nameof(NewTitle));
                _commitModify.RaiseCanExecuteChanged();
            }
        }

        private void Modify(object id)
        {
            Repository.Patch(nameof(Book.Id), (Guid)id, nameof(Book.Title), EditTitle);
            EditingId = default;
            EditTitle = string.Empty;
        }
    }
}