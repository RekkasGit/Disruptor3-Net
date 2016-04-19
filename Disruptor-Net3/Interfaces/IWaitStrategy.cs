using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.Interfaces
{
  
    /**
     * Strategy employed for making {@link EventProcessor}s wait on a cursor {@link Sequence}.
     */
    public interface IWaitStrategy
    {
        /**
         * Wait for the given sequence to be available.  It is possible for this method to return a value
         * less than the sequence number supplied depending on the implementation of the WaitStrategy.  A common
         * use for this is to signal a timeout.  Any EventProcessor that is using a WaitStragegy to get notifications
         * about message becoming available should remember to handle this case.  The {@link BatchEventProcessor} explicitly
         * handles this case and will signal a timeout if required.
         *
         * @param sequence to be waited on.
         * @param cursor the main sequence from ringbuffer. Wait/notify strategies will
         *    need this as it's the only sequence that is also notified upon update.
         * @param dependentSequence on which to wait.
         * @param barrier the processor is waiting on.
         * @return the sequence that is available which may be greater than the requested sequence.
         * @throws AlertException if the status of the Disruptor has changed.
         * @throws InterruptedException if the thread is interrupted.
         * @throws TimeoutException
         */
        long waitFor(long sequence, Sequence cursor, Sequence dependentSequence, ISequenceBarrier barrier);

        /**
         * Implementations should signal the waiting {@link EventProcessor}s that the cursor has advanced.
         */
        void signalAllWhenBlocking();
    }
}
