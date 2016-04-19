using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.Interfaces
{
    
    /**
     * Coordination barrier for tracking the cursor for publishers and sequence of
     * dependent {@link EventProcessor}s for processing a data structure
     */
    public interface ISequenceBarrier
    {
        /**
         * Wait for the given sequence to be available for consumption.
         *
         * @param sequence to wait for
         * @return the sequence up to which is available
         * @throws AlertException if a status change has occurred for the Disruptor
         * @throws InterruptedException if the thread needs awaking on a condition variable.
         * @throws TimeoutException
         */
        long waitFor(long sequence); 

        /**
         * Get the current cursor value that can be read.
         *
         * @return value of the cursor for entries that have been published.
         */
        long getCursor();

        /**
         * The current alert status for the barrier.
         *
         * @return true if in alert otherwise false.
         */
        Boolean isAlerted();

        /**
         * Alert the {@link EventProcessor}s of a status change and stay in this status until cleared.
         */
        void alert();

        /**
         * Clear the current alert status.
         */
        void clearAlert();

        /**
         * Check if an alert has been raised and throw an {@link AlertException} if it has.
         *
         * @throws AlertException if alert has been raised.
         */
        void checkAlert();
    }

}
