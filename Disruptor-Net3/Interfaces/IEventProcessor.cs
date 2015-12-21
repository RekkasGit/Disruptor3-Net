using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor_Net3.Interfaces
{
   /**
     * EventProcessors waitFor events to become available for consumption from the {@link RingBuffer}
     *
     * An EventProcessor will generally be associated with a Thread for execution.
     */
    public interface IEventProcessor:IRunnable
    {
        /**
         * Get a reference to the {@link Sequence} being used by this {@link EventProcessor}.
         *
         * @return reference to the {@link Sequence} for this {@link EventProcessor}
         */
        Sequence getSequence();

        /**
         * Signal that this EventProcessor should stop when it has finished consuming at the next clean break.
         * It will call {@link SequenceBarrier#alert()} to notify the thread to check status.
         */
        void halt();

        Boolean isRunning();
    }
}
