using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using AiurStore.Models;
using AiurVersionControl.CRUD;
using AiurVersionControl.Models;
using AiurVersionControl.SampleWPF.Models;
using AiurVersionControl.SampleWPF.ViewModels.MVVM;

namespace AiurVersionControl.SampleWPF.ViewModels
{
    internal sealed class ConverterPresenter : Presenter, INotifyPropertyChanged
    {
        public CollectionRepository<Book> Repository { get; set; } = new CollectionRepository<Book>();

        public IOutDatabase<Commit<IModification<CollectionWorkSpace<Book>>>> History => Repository.Commits;

        private readonly RelayCommand<object> _commit;
        private string _someText;

        public ICommand Commit => _commit;

        public string SomeText
        {
            get => _someText;
            set
            {
                Update(ref _someText, value, nameof(SomeText));
                _commit.RaiseCanExecuteChanged();
            }
        }

        public ConverterPresenter()
        {
            _commit = new RelayCommand<object>(_ =>
            {
                Repository.Add(new Book
                {
                    Title = SomeText.ToUpper(),
                    Id = 7
                });

                SomeText = string.Empty;
            }, _ =>
            {
                return !string.IsNullOrWhiteSpace(SomeText);
            });
        }
    }
}