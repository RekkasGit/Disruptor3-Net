using Disruptor_Net3.Exceptions;
using Disruptor_Net3.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disruptor_Net3.Sequencers
{
    public abstract class SingleProducerSequencerPad : AbstractSequencer
    {
        protected long p1, p2, p3, p4, p5, p6, p7;
        public SingleProducerSequencerPad(int bufferSize, IWaitStrategy waitStrategy):base(bufferSize,waitStrategy)
        {
           
        }
    }

    public abstract class SingleProducerSequencerFields:SingleProducerSequencerPad
    {
        public SingleProducerSequencerFields(int bufferSize, IWaitStrategy waitStrategy):base(bufferSize,waitStrategy)
        {
           
        }

        /** Set to -1 as sequence starting point */
        protected long nextValue = Sequence.INITIAL_VALUE;
        protected long cachedValue = Sequence.INITIAL_VALUE;
    }

    /**
     * <p>Coordinator for claiming sequences for access to a data structure while tracking dependent {@link Sequence}s.
     * Not safe for use from multiple threads as it does not implement any barriers.</p>
     *
     * <p>Note on {@link Sequencer#getCursor()}:  With this sequencer the cursor value is updated after the call
     * to {@link Sequencer#publish(long)} is made.
     */

    public class SingleProducerSequencer:SingleProducerSequencerFields
    {
        protected long p8, p9, p10, p11, p12, p13, p14;

        /**
         * Construct a Sequencer with the selected wait strategy and buffer size.
         *
         * @param bufferSize the size of the buffer that this will sequence over.
         * @param waitStrategy for those waiting on sequences.
         */
        public SingleProducerSequencer(int bufferSize, IWaitStrategy waitStrategy):base(bufferSize,waitStrategy)
        {
         
        }

        /**
         * @see Sequencer#hasAvailableCapacity(int)
         */
        
        public override Boolean hasAvailableCapacity(int requiredCapacity)
        {
            long nextValue = this.nextValue;

            long wrapPoint = (nextValue + requiredCapacity) - bufferSize;
            long cachedGatingSequence = this.cachedValue;

            if (wrapPoint > cachedGatingSequence || cachedGatingSequence > nextValue)
            {
                long minSequence = Util.Util.getMinimumSequence(gatingSequences, nextValue);
                this.cachedValue = minSequence;

                if (wrapPoint > minSequence)
                {
                    return false;
                }
            }

            return true;
        }

        /**
         * @see Sequencer#next()
         */
       
        public override long next()
        {
            return next(1);
        }

        /**
         * @see Sequencer#next(int)
         */
      
        public override long next(int n)
        {
            if (n < 1)
            {
                throw new ArgumentOutOfRangeException("n must be > 0");
            }

            long nextValue = this.nextValue;

            long nextSequence = nextValue + n;
            long wrapPoint = nextSequence - bufferSize;
            long cachedGatingSequence = this.cachedValue;

            if (wrapPoint > cachedGatingSequence || cachedGatingSequence > nextValue)
            {
                long minSequence;
                var spinlock = default(SpinWait);
                while (wrapPoint > (minSequence = Util.Util.getMinimumSequence(gatingSequences, nextValue)))
                {
                    spinlock.SpinOnce();
                    //LockSupport.parkNanos(1L); // TODO: Use waitStrategy to spin?
                }

                this.cachedValue = minSequence;
            }

            this.nextValue = nextSequence;

            return nextSequence;
        }

        /**
         * @see Sequencer#tryNext()
         */
        
        public override long tryNext()
        {
            return tryNext(1);
        }

        /**
         * @see Sequencer#tryNext(int)
         */
        public override long tryNext(int n)
        {
            if (n < 1)
            {
                throw new ArgumentOutOfRangeException("n must be > 0");
            }

            if (!hasAvailableCapacity(n))
            {
                throw InsufficientCapacityException.INSTANCE;
            }

            long nextSequence = this.nextValue += n;

            return nextSequence;
        }

        /**
         * @see Sequencer#remainingCapacity()
         */
       
        public override long remainingCapacity()
        {
            long nextValue = this.nextValue;

            long consumed = Util.Util.getMinimumSequence(gatingSequences, nextValue);
            long produced = nextValue;
            return getBufferSize() - (produced - consumed);
        }

        /**
         * @see Sequencer#claim(long)
         */
       
        public override void claim(long sequence)
        {
            this.nextValue = sequence;
        }

        /**
         * @see Sequencer#publish(long)
         */
        
        public override void publish(long sequence)
        {
            cursor.set(sequence);
            waitStrategy.signalAllWhenBlocking();
        }

        /**
         * @see Sequencer#publish(long, long)
         */
        
        public override void publish(long lo, long hi)
        {
            publish(hi);
        }

        /**
         * @see Sequencer#isAvailable(long)
         */
       
        public override Boolean isAvailable(long sequence)
        {
            return sequence <= cursor.get();
        }
        
        public override long getHighestPublishedSequence(long lowerBound, long availableSequence)
        {
            return availableSequence;
        }
    }
}
