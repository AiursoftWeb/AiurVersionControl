using System.Numerics;

namespace AiurVersionControl.LSEQ.StrategiesComponents.Boundary
{
    public interface IBoundary
    {
        /// <summary>
        /// Return the boundary value at the specified depth. 
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        BigInteger GetBoundary(int depth);
    }
}