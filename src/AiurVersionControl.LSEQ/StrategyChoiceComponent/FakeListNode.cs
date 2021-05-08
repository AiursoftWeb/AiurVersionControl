using AiurVersionControl.LSEQ.Data;

namespace AiurVersionControl.LSEQ.StrategyChoiceComponent
{
    public class FakeListNode
    {
        public Positions Prev { get; set; }

        public int Date { get; set; }

        public Positions Next { get; set; }

        public FakeListNode(Positions prev, int date, Positions next)
        {
            Prev = prev;
            Date = date;
            Next = next;
        }
    }
}