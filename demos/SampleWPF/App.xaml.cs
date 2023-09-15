using Aiursoft.AiurVersionControl.CRUD;
using Aiursoft.AiurVersionControl.SampleWPF.Models;
using Aiursoft.AiurVersionControl.SampleWPF.Windows;
using System.Windows;

namespace Aiursoft.AiurVersionControl.SampleWPF
{
    internal sealed partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var repo = new CollectionRepository<Book>();

            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowPresenter(repo)
            };

            mainWindow.Show();
        }
    }

}