using AiurVersionControl.CRUD;
using AiurVersionControl.Text.Modifications;
using NetDiff;
using System.Linq;

namespace AiurVersionControl.Text
{
    /// <summary>
    /// A special controlled repository that contains a text workspace which you can do modification to.
    /// </summary>
    public class TextRepository : CollectionRepository<string>
    {
        public void UpdateText(string[] newContent)
        {
            var diff = DiffUtil.Diff(WorkSpace.List, newContent).ToArray();
            ApplyChange(new LineDiffsCommit(diff));
        }
    }
}
