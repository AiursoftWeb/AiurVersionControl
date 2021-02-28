using System.Collections.ObjectModel;
using System.Windows;
using AiurVersionControl.SampleWPF.ViewModels;
using AiurVersionControl.SampleWPF.Views;

namespace AiurVersionControl.SampleWPF
{
    internal sealed partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var converterPresenter = new ConverterPresenter();
            var mainWindow = new ConvertWindow {DataContext = converterPresenter};

            mainWindow.Show();
        }
    }
}