using System.Numerics;

namespace AiurVersionControl.LSEQ.StrategiesComponents.Boundary
{
    public class ConstantBoundary : IBoundary
    {
        private readonly BigInteger _value;

        public ConstantBoundary(BigInteger value)
        {
            _value = value;
        }
            
        public BigInteger GetBoundary(int depth)
        {
            return _value;
        }
    }
}