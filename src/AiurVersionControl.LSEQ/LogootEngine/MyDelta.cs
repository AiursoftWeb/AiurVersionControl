using AiurVersionControl.LSEQ.Data;

namespace AiurVersionControl.LSEQ.LogootEngine
{
    public class MyDelta
    {
        public Operation Type { get; set; }
        public Positions Id { get; set; }
        public string Content { get; set; }

        public MyDelta(Operation type, Positions id, string content) {
            Type = type;
            Id = id;
            Content = content;
        }
        
        public override string ToString() {
            return "<" + Type + "," + Id + ">";
        }
    }
}