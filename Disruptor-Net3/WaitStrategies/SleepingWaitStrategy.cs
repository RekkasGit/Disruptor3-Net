using Disruptor_Net3.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disruptor_Net3.WaitStrategies
{
    /**
     * Sleeping strategy that initially spins, then uses a Thread.yield(), and
     * eventually sleep (<code>LockSupport.parkNanos(1)</code>) for the minimum
     * number of nanos the OS and JVM will allow while the
     * {@link com.lmax.disruptor.EventProcessor}s are waiting on a barrier.
     *
     * This strategy is a good compromise between performance and CPU resource.
     * Latency spikes can occur after quiet periods.
     */
    public class SleepingWaitStrategy:IWaitStrategy
    {
        private static int DEFAULT_RETRIES = 200;

        private int retries;

        public SleepingWaitStrategy():this(DEFAULT_RETRIES)
        {
         }

        public SleepingWaitStrategy(int retries)
        {
            this.retries = retries;
        }

       
        public long waitFor( long sequence, Sequence cursor, Sequence dependentSequence, ISequenceBarrier barrier)
        {
            long availableSequence;
            int counter = retries;

            while ((availableSequence = dependentSequence.get()) < sequence)
            {
                counter = applyWaitMethod(barrier, counter);
            }

            return availableSequence;
        }

      
        public void signalAllWhenBlocking()
        {
        }

        private int applyWaitMethod(ISequenceBarrier barrier, int counter)
        {
            barrier.checkAlert();

            if (counter > 100)
            {
                --counter;
            }
            else if (counter > 0)
            {
                --counter;
                Thread.Yield();
            }
            else
            {
                System.Threading.Thread.Sleep(1);
            }

            return counter;
        }
    }
}
