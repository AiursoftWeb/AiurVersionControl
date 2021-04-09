using AiurVersionControl.Models;
using AiurVersionControl.Text.Modifications;
using NetDiff;
using System.Linq;

namespace AiurVersionControl.Text
{
    /// <summary>
    /// A special controlled repository that contains a text workspace which you can do modification to.
    /// </summary>
    public class TextRepository : ControlledRepository<TextWorkSpace>
    {
        public void Update(string newContent)
        {
            var diff = DiffUtil.Diff(WorkSpace.Content, newContent).ToArray();
            ApplyChange(new TextDiff
            {
                Diff = diff
            });
        }
    }
}
