﻿using AiurVersionControl.CRUD;
using AiurVersionControl.SampleWPF.Components;
using AiurVersionControl.SampleWPF.Libraries;
using AiurVersionControl.SampleWPF.Models;

namespace AiurVersionControl.SampleWPF.Windows
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
