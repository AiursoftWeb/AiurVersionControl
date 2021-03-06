using AiurVersionControl.CRUD;
using AiurVersionControl.SampleWPF.Models;
using AiurVersionControl.SampleWPF.Services;
using AiurVersionControl.SampleWPF.Windows;
using System.Windows;

namespace AiurVersionControl.SampleWPF
{
    internal sealed partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var repo = new CollectionRepository<Book>();

            AppController.Repo = repo;
            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowModel(repo)
            };

            mainWindow.Show();
        }
    }

}