using AiurVersionControl.SampleWPF.Libraries;
using AiurVersionControl.SampleWPF.Models;
using System;
using System.Windows.Input;

namespace AiurVersionControl.SampleWPF.Components
{
    internal sealed class BookListItemPresenter : Presenter
    {
        public Book Book { get; init; }
        private readonly RelayCommand<object> _beginEdit;
        private readonly RelayCommand<object> _save;
        private readonly RelayCommand<object> _commitDrop;
        private string _editTitle;
        private bool _isEditing;


        public ICommand BeginEdit => _beginEdit;
        public ICommand Save => _save;
        public ICommand CommitDrop => _commitDrop;

        private Action<string> _onPatch;
        
        public BookListItemPresenter(Book book, Action<string> onPatch, Action<object> onDrop)
        {
            Book = book;
            _onPatch = onPatch;
            _beginEdit = new RelayCommand<object>(SwitchEdit, _=>true);
            _save = new RelayCommand<object>(SaveCommand, _=>!string.IsNullOrWhiteSpace(EditTitle));
            _commitDrop = new RelayCommand<object>(onDrop, _ => true);
        }

        public string EditTitle
        {
            get => _editTitle;
            set
            {
                Update(ref _editTitle, value);
                _save.RaiseCanExecuteChanged();
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                Update(ref _isEditing, value);
                OnPropertyChanged(nameof(IsNotEditing));
            }
        }

        public bool IsNotEditing => !_isEditing;

        private void SwitchEdit(object _)
        {
            EditTitle = Book.Title;
            IsEditing = IsEditing == false;
        }
        
        private void SaveCommand(object _)
        {
            _onPatch(EditTitle);
            OnPropertyChanged(nameof(Book));
            IsEditing = false;
        }
    }
}