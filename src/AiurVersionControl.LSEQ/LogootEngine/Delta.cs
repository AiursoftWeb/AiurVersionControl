using AiurVersionControl.LSEQ.Data;

namespace AiurVersionControl.LSEQ.LogootEngine
{
    public class Delta
    {
        private Operation Type { get; set; }
        private Positions Id { get; set; }
        private string Content { get; set; }

        public Delta(Operation type, Positions id, string content) {
            Type = type;
            Id = id;
            Content = content;
        }
        
        public override string ToString() {
            return "<" + Type + "," + Id + ">";
        }
    }
}