using AiurVersionControl.Models;

namespace AiurVersionControl.Tests.Models
{
    public class AddModification : IModification<NumberWorkSpace>
    {
        public int Amount { get; set; }

        public AddModification(int amount)
        {
            Amount = amount;
        }
        
        public AddModification(){}

        public void Apply(NumberWorkSpace workspace)
        {
            workspace.NumberStore += Amount;
        }
    }
}
