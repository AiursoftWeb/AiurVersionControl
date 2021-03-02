using AiurEventSyncer.Abstract;
using AiurStore.Models;
using AiurVersionControl.CRUD;
using AiurVersionControl.Models;
using AiurVersionControl.SampleWPF.Models;
using AiurVersionControl.SampleWPF.Services;
using AiurVersionControl.SampleWPF.ViewModels.MVVM;
using System.ComponentModel;
using System.Windows.Input;

namespace AiurVersionControl.SampleWPF.ViewModels
{
    internal sealed class BooksCRUDPresenter : Presenter, INotifyPropertyChanged
    {
        private readonly RelayCommand<object> _commitAddNew;
        private string _newTitle = string.Empty;
        private Counter _counter = new Counter();

        public CollectionRepository<Book> Repository { get; set; } = new CollectionRepository<Book>();

        public IOutOnlyDatabase<Commit<IModification<CollectionWorkSpace<Book>>>> History => Repository.Commits;

        public ICommand CommitAddNew => _commitAddNew;

        public string NewTitle
        {
            get => _newTitle;
            set
            {
                Update(ref _newTitle, value, nameof(NewTitle));
                _commitAddNew.RaiseCanExecuteChanged();
            }
        }

        public BooksCRUDPresenter()
        {
            _commitAddNew = new RelayCommand<object>(newTitle =>
            {
                Repository.Add(new Book
                {
                    Title = (newTitle as string).ToUpper(),
                    Id = _counter.GetUniqueNo()
                });

                NewTitle = string.Empty;
            }, newTitle =>
            {
                return !string.IsNullOrWhiteSpace(newTitle as string);
            });
        }
    }
}