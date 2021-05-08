using System.Numerics;
using Microsoft.VisualBasic.CompilerServices;

namespace AiurVersionControl.LSEQ.BaseComponents
{
    public interface IBase
    {
        /// <summary>
        /// Return the number of bit to a depth
        /// </summary>
        /// <param name="depth"></param>
        /// <returns>bit number</returns>
        int GetSumBit(int depth);

        /// <summary>
        /// The number of bit used at a given depth
        /// </summary>
        /// <param name="depth"></param>
        /// <returns>bit number</returns>
        int GetBitBase(int depth);

        /// <summary>
        /// the number of bit at depth 1
        /// </summary>
        /// <returns>bit number</returns>
        int GetBaseBase();

        /// <summary>
        /// Process the interval (i.e. number of id possible) between p and q, p <_id < q
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        BigInteger Interval(BigInteger p, BigInteger q, int index);
    }
}