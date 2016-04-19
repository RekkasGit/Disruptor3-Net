using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.Exceptions
{
    /// <summary>
    /// Used to alert <see cref="IEventProcessor"/>s waiting at a <see cref="ISequenceBarrier"/> of status changes.
    /// </summary>
    public class InsufficientCapacityException : Exception
    {
        /// <summary>
        /// Pre-allocated exception to avoid garbage generation
        /// </summary>
        public static readonly InsufficientCapacityException INSTANCE = new InsufficientCapacityException();

        /// <summary>
        /// Private constructor so only a single instance exists.
        /// </summary>
        private InsufficientCapacityException()
        {
        }
    }
}
