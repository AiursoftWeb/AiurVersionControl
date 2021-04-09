using AiurVersionControl.Models;
using NetDiff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurVersionControl.Text.Modifications
{
    public class TextDiff : IModification<TextWorkSpace>
    {
        public DiffResult<char>[] Diff { get; internal set; }

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
                        workspace.Content = workspace.Content.Insert(i, diffItem.Obj2.ToString());
                        i++;
                        break;
                    case DiffStatus.Deleted:
                        workspace.Content = workspace.Content.Remove(i, 1);
                        break;
                }
            }
        }
    }
}
