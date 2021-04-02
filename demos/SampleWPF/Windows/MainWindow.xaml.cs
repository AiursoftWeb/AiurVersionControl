using System.ComponentModel;
using System.Windows;

namespace AiurVersionControl.SampleWPF.Windows
{
    internal sealed partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += OnClosing;
        }

        void OnClosing(object sender, CancelEventArgs e)
        {
            if ((DataContext as MainWindowPresenter)?.CommitsPresenter.ServerHosting ?? false)
            {
                MessageBox.Show(
                    "There is still a server hosting!",
                    "Quit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                e.Cancel = true;
            }
        }
    }
}