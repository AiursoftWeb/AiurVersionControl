using System;

namespace SampleWebApp.Models
{
    public class LogItem
    {
        private readonly Guid _hash;

        public int Id { get; set; }

        public string Message { get; set; }

        public LogItem()
        {
            this._hash = Guid.NewGuid();
        }

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
            return _hash.GetHashCode();
        }
    }
}
