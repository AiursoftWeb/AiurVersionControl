using AiurVersionControl.CRUD;
using AiurVersionControl.Remotes;
using AiurVersionControl.SampleWPF.Models;

namespace AiurVersionControl.SampleWPF.Components
{
    /// <summary>
    /// Interaction logic for RemoteManagement.xaml
    /// </summary>
    public sealed partial class RemoteControl
    {
        public RemoteControl(WebSocketRemoteWithWorkSpace<CollectionWorkSpace<Book>> remote)
        {
            InitializeComponent();
            RemoteObject = remote;
        }

        public WebSocketRemoteWithWorkSpace<CollectionWorkSpace<Book>> RemoteObject { get; }
    }
}
