using AiurVersionControl.SampleWPF.Libraries;
using AiurVersionControl.Text;

namespace AiurVersionControl.SampleWPF.Components
{
    internal sealed class TextEditorPresenter : Presenter
    {
        private TextRepository Repository { get; set; }

        public TextEditorPresenter(TextRepository repo)
        {
            Repository = repo;
            Repository.PropertyChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(NewTitle));
            };
        }

        public string NewTitle
        {
            get => string.Join('\n', Repository.WorkSpace.Content);
            set => Repository.Update(value.Split('\n'));
        }
    }
}