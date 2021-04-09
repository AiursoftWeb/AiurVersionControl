using AiurVersionControl.Models;

namespace AiurVersionControl.Text
{
    /// <summary>
    /// A special workspace which contains a string.
    /// </summary>
    public class TextWorkSpace : WorkSpace
    {
        public string Content { get; internal set; } = string.Empty;

        public TextWorkSpace()
        {

        }

        public TextWorkSpace(string content) : this()
        {
            Content = content;
        }

        public override object Clone()
        {
            return new TextWorkSpace(Content);
        }
    }
}
