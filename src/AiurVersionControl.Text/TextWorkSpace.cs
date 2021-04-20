using AiurVersionControl.Models;
using System.Collections.Generic;
using System.Linq;

namespace AiurVersionControl.Text
{
    /// <summary>
    /// A special workspace which contains a string.
    /// </summary>
    public class TextWorkSpace : WorkSpace
    {
        public List<string> Content { get; internal set; } = new();

        public TextWorkSpace()
        {

        }

        public TextWorkSpace(List<string> content) : this()
        {
            Content = content.ToList();
        }

        public override object Clone()
        {
            return new TextWorkSpace(Content);
        }
    }
}
