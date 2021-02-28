using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using AiurVersionControl.CRUD;
using AiurVersionControl.SampleWPF.Models;
using AiurVersionControl.SampleWPF.ViewModels.MVVM;

namespace AiurVersionControl.SampleWPF.ViewModels
{
    internal sealed class ConverterPresenter : Presenter, INotifyPropertyChanged
    {
        public CollectionRepository<Book> Repository { get; set; } = new CollectionRepository<Book>();

        public string SomeText { get; set; }

        public ICommand Commit => new Command(_ => 
        {
            if (string.IsNullOrWhiteSpace(SomeText))
            {
                return;
            }

            Repository.Add(new Book
            {
                Title = SomeText.ToUpper(),
                Id = 7
            });

            SomeText = string.Empty;
        });
    }
}