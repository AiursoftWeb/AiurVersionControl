using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AiurVersionControl.SampleWPF.Services
{
    public class Counter
    {
        private int _current;

        /// <summary>
        /// Get a new scope unique number which is one larger than current.
        /// </summary>
        /// <returns></returns>
        public int GetUniqueNo()
        {
            return Interlocked.Increment(ref this._current);
        }

        /// <summary>
        /// Last returned counter value. If a initial counter, will be -1.
        /// </summary>
        public int GetCurrent
        {
            get
            {
                return this._current;
            }
        }
    }
}
