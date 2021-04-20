using AiurVersionControl.SampleWPF.Components;
using AiurVersionControl.SampleWPF.Libraries;
using AiurVersionControl.Text;

namespace AiurVersionControl.SampleWPF.Windows
{
    internal class MainWindowPresenter : Presenter
    {
        public MainWindowPresenter(TextRepository repo)
        {
            CrudPresenter = new TextEditorPresenter(repo);
            CommitsPresenter = new CommitsManagementPresenter(repo);
            RemotesPresenter = new RemoteManagementPresenter(repo);
        }

        public TextEditorPresenter CrudPresenter { get; set; }
        public CommitsManagementPresenter CommitsPresenter { get; set; }
        public RemoteManagementPresenter RemotesPresenter { get; set; }
    }
}
