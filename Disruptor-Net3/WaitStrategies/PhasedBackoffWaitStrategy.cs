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
     * <p>Phased wait strategy for waiting {@link EventProcessor}s on a barrier.</p>
     *
     * <p>This strategy can be used when throughput and low-latency are not as important as CPU resource.
     * Spins, then yields, then waits using the configured fallback WaitStrategy.</p>
     */
    public class PhasedBackoffWaitStrategy:IWaitStrategy
    {
        private static int SPIN_TRIES = 10000;
        private long spinTimeoutNanos;
        private long yieldTimeoutNanos;
        private IWaitStrategy fallbackStrategy;
        System.Diagnostics.Stopwatch _stopWatch = new System.Diagnostics.Stopwatch();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spinTimeout">In NanoSeconds</param>
        /// <param name="yieldTimeout">In NanoSeconds</param>
        /// <param name="fallbackStrategy"></param>
        public PhasedBackoffWaitStrategy(long spinTimeout,
                                         long yieldTimeout,
                                         IWaitStrategy fallbackStrategy)
        {
            this.spinTimeoutNanos = spinTimeout;
            this.yieldTimeoutNanos = spinTimeoutNanos + yieldTimeout;
            this.fallbackStrategy = fallbackStrategy;
        }

        /**
         * Block with wait/notifyAll semantics
         */


        /// <summary>
        /// 
        /// </summary>
        /// <param name="spinTimeout">In NanoSeconds</param>
        /// <param name="yieldTimeout">In NanoSeconds</param>
        /// <param name="fallbackStrategy"></param>
    
        public static PhasedBackoffWaitStrategy withLock(long spinTimeout,
                                                         long yieldTimeout)
        {
            return new PhasedBackoffWaitStrategy(spinTimeout, yieldTimeout, new BlockingWaitStrategy());
        }


        /**
         * Block by sleeping in a loop
         */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spinTimeout">In NanoSeconds</param>
        /// <param name="yieldTimeout">In NanoSeconds</param>
        /// <param name="fallbackStrategy"></param>
        public static PhasedBackoffWaitStrategy withSleep(long spinTimeout,
                                                          long yieldTimeout)
        {
            return new PhasedBackoffWaitStrategy(spinTimeout, yieldTimeout,new SleepingWaitStrategy(0));
        }

        public long waitFor(long sequence, Sequence cursor, Sequence dependentSequence, ISequenceBarrier barrier)
        {
            long availableSequence;
            long startTime = 0;
            int counter = SPIN_TRIES;
          
            do
            {
                if ((availableSequence = dependentSequence.get()) >= sequence)
                {
                    _stopWatch.Stop();
                    return availableSequence;
                }

                if (0 == --counter)
                {
                    if (0 == startTime)
                    {
                        startTime = 1;
                        _stopWatch.Restart();
                    }
                    else
                    {
                        long timeDelta = ((Int64)(_stopWatch.Elapsed.TotalMilliseconds * 1000000)) - startTime;
                        if (timeDelta > yieldTimeoutNanos)
                        {
                            _stopWatch.Stop();
                            return fallbackStrategy.waitFor(sequence, cursor, dependentSequence, barrier);
                        }
                        else if (timeDelta > spinTimeoutNanos)
                        {
                            Thread.Yield();
                        }
                    }
                    counter = SPIN_TRIES;
                }
            }
            while (true);
        }

        public void signalAllWhenBlocking()
        {
            fallbackStrategy.signalAllWhenBlocking();
        }
    }
}
