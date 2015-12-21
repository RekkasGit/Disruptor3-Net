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
     * Yielding strategy that uses a Thread.yield() for {@link com.lmax.disruptor.EventProcessor}s waiting on a barrier
     * after an initially spinning.
     *
     * This strategy is a good compromise between performance and CPU resource without incurring significant latency spikes.
     */
    public class YieldingWaitStrategy:IWaitStrategy
    {
        private static int SPIN_TRIES = 100;

        public long waitFor(long sequence, Sequence cursor, Sequence dependentSequence,ISequenceBarrier barrier)
        {
            long availableSequence;
            int counter = SPIN_TRIES;

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

            if (0 == counter)
            {
                Thread.Yield();
            }
            else
            {
                --counter;
            }

            return counter;
        }
    }
}
