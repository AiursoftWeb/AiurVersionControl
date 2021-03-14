using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
        private string _newTitle = string.Empty;

        public ICommand CommitAddNew => _commitAddNew;

        public CollectionRepository<Book> Repository { get; set; }

        public IEnumerable<BookListItem> Books => Repository.Select(b => new BookListItem
        {
            DataContext = new BookListItemPresenter(b, onPatch: (string newTitle) =>
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
            Repository.CollectionChanged += new((object o, NotifyCollectionChangedEventArgs e) =>
           {
               OnPropertyChanged(nameof(Books));
           });
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
            OnPropertyChanged(nameof(Books));
            NewTitle = string.Empty;
        }
    }
}