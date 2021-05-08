namespace AiurVersionControl.LSEQ.LogootEngine
{
    public class Replica
    {
        public int Id { get; set; } // Id of replica

        public int Clock { get; set; } // Clock of replica

        public Replica()
        {
            Id = 0;
            Clock = 0;
        }
    }
}