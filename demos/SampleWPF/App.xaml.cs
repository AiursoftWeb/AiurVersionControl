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

            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowModel
                {
                    CrudPresenter = new BooksCRUDPresenter()
                }
            };

            mainWindow.Show();
        }
    }

}