using AiurVersionControl.SampleWPF.Windows;
using AiurVersionControl.Text;
using System.Windows;

namespace AiurVersionControl.SampleWPF
{
    internal sealed partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var repo = new TextRepository();

            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowPresenter(repo)
            };

            mainWindow.Show();
        }
    }

}