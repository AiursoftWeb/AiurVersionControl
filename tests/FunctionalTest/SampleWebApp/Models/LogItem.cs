namespace SampleWebApp.Models
{
    public class LogItem
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return Message;
        }

        public override bool Equals(object obj)
        {
            return (obj as LogItem)?.Message == Message;
        }

        public override int GetHashCode()
        {
            return Message.GetHashCode();
        }
    }
}
