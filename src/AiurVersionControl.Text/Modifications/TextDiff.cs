using System.Linq;
using AiurVersionControl.Models;
using NetDiff;

namespace AiurVersionControl.Text.Modifications
{
    public class TextDiff : IModification<TextWorkSpace>
    {
        public DiffResult<string>[] Diff { get; set; }

        public void Apply(TextWorkSpace workspace)
        {
            int i = 0;
            foreach(var diffItem in Diff)
            {
                switch (diffItem.Status)
                {
                    case DiffStatus.Equal:
                        i++;
                        break;
                    case DiffStatus.Inserted:
                        var tempList = workspace.Content.ToList();
                        tempList.Insert(i, diffItem.Obj2);
                        workspace.Content = tempList.ToArray();
                        i++;
                        break;
                    case DiffStatus.Deleted:
                        var tempList2 = workspace.Content.ToList();
                        tempList2.RemoveAt(i);
                        workspace.Content = tempList2.ToArray();
                        break;
                }
            }
        }
    }
}
