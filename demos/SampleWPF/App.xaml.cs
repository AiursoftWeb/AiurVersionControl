using AiurVersionControl.CRUD;
using AiurVersionControl.SampleWPF.Models;
using AiurVersionControl.SampleWPF.ViewModels;
using AiurVersionControl.SampleWPF.Views;
using System.Windows;

namespace AiurVersionControl.SampleWPF
{
    internal sealed partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var repo = new CollectionRepository<Book>();
            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowModel
                {
                    CrudPresenter = new BooksCRUDPresenter(repo)
                }
            };

            mainWindow.Show();
        }
    }

}