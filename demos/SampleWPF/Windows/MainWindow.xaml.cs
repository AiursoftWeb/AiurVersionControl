using System.ComponentModel;

namespace AiurVersionControl.SampleWPF.Windows
{
    internal sealed partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += OnClosing;
        }

        async void OnClosing(object sender, CancelEventArgs e)
        {
            await (DataContext as MainWindowModel).CommitsPresenter.StopServer();
        }
    }
}