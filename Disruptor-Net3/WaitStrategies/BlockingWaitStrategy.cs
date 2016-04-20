using Disruptor3_Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disruptor3_Net.WaitStrategies
{
    /**
     * Blocking strategy that uses a lock and condition variable for {@link EventProcessor}s waiting on a barrier.
     *
     * This strategy can be used when throughput and low-latency are not as important as CPU resource.
     */
    public sealed class BlockingWaitStrategy : IWaitStrategy
    {
        private readonly object _gate = new object();
        private volatile int _numWaiters;

        /// <summary>
        /// Wait for the given sequence to be available
        /// </summary>
        /// <param name="sequence">sequence to be waited on.</param>
        /// <param name="cursor">Ring buffer cursor on which to wait.</param>
        /// <param name="dependents">dependents further back the chain that must advance first</param>
        /// <param name="barrier">barrier the <see cref="IEventProcessor"/> is waiting on.</param>
        /// <returns>the sequence that is available which may be greater than the requested sequence.</returns>
        public long waitFor(long sequence, Sequence cursor, Sequence dependentSequence, ISequenceBarrier barrier)
        {
            var availableSequence = cursor.get(); // volatile read
            if (availableSequence < sequence)
            {
                Monitor.Enter(_gate);

                //try/catch is faster than try finally by a fair bit.
                try
                {
                    ++_numWaiters;
                    while ((availableSequence = cursor.get()) < sequence) // volatile read
                    {
                        barrier.checkAlert();
                        Monitor.Wait(_gate);
                    }
                }
                catch(Exception ex)
                {
                    --_numWaiters;
                    Monitor.Exit(_gate);
                    throw;
                }
                --_numWaiters;
                Monitor.Exit(_gate);

            }
            while ((availableSequence = dependentSequence.get()) < sequence)
            {
                barrier.checkAlert();
            }

            return availableSequence;
        }
        /// <summary>
        /// Signal those <see cref="IEventProcessor"/> waiting that the cursor has advanced.
        /// </summary>
        public void signalAllWhenBlocking()
        {
            if (_numWaiters != 0)
            {
                lock (_gate)
                {
                    Monitor.PulseAll(_gate);
                }
            }
        }
    }
}
