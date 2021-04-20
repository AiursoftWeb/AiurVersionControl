using NetDiff;

namespace AiurVersionControl.Text.Modifications
{
    public class LineDiff
    {
        public int LineNumber { get; set; }
        public DiffStatus Status { get; set; }
        public string NewLine { get; set; }
    }
}
