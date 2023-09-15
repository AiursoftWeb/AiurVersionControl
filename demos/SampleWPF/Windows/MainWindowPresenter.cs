using Aiursoft.AiurVersionControl.CRUD;
using Aiursoft.AiurVersionControl.SampleWPF.Components;
using Aiursoft.AiurVersionControl.SampleWPF.Libraries;
using Aiursoft.AiurVersionControl.SampleWPF.Models;

namespace Aiursoft.AiurVersionControl.SampleWPF.Windows
{
    internal class MainWindowPresenter : Presenter
    {
        public MainWindowPresenter(CollectionRepository<Book> repo)
        {
            CrudPresenter = new BooksCRUDPresenter(repo);
            CommitsPresenter = new CommitsManagementPresenter(repo);
            RemotesPresenter = new RemoteManagementPresenter(repo);
        }

        public BooksCRUDPresenter CrudPresenter { get; set; }
        public CommitsManagementPresenter CommitsPresenter { get; set; }
        public RemoteManagementPresenter RemotesPresenter { get; set; }
    }
}
