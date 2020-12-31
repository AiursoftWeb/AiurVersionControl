using AiurVersionControl.Models;

namespace AiurVersionControl.Tests.Models
{
    public class AddModification : IModification<NumberWorkSpace>
    {
        private readonly int _amount;

        public AddModification(int amount)
        {
            _amount = amount;
        }

        public void Apply(NumberWorkSpace workspace)
        {
            workspace.NumberStore += _amount;
        }
    }
}
