using Disruptor3_Net.Exceptions;
using Disruptor3_Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disruptor3_Net.Sequencers
{




    [StructLayout(LayoutKind.Explicit, Size = 136)]
    public struct PaddedSingleSequence
    {
        [FieldOffset(0)]
        Int64 p1;
        [FieldOffset(8)]
        Int64 p2;
        [FieldOffset(16)]
        Int64 p3;
        [FieldOffset(24)]
        Int64 p4;
        [FieldOffset(32)]
        Int64 p5;
        [FieldOffset(40)]
        Int64 p6;
        [FieldOffset(48)]
        Int64 p7;
        [FieldOffset(56)]
        Int64 p8;
        [FieldOffset(64)]
        public Int64 nextValue;
        [FieldOffset(72)]
        public Int64 cachedValue;
        [FieldOffset(80)]
        Int64 p10;
        [FieldOffset(88)]
        Int64 p11;
        [FieldOffset(96)]
        Int64 p12;
        [FieldOffset(104)]
        Int64 p13;
        [FieldOffset(112)]
        Int64 p14;
        [FieldOffset(120)]
        Int64 p15;
        [FieldOffset(128)]
        Int64 p16;
    }
   

    /**
     * <p>Coordinator for claiming sequences for access to a data structure while tracking dependent {@link Sequence}s.
     * Not safe for use from multiple threads as it does not implement any barriers.</p>
     *
     * <p>Note on {@link Sequencer#getCursor()}:  With this sequencer the cursor value is updated after the call
     * to {@link Sequencer#publish(long)} is made.
     */

    public class SingleProducerSequencer:AbstractSequencer
    {

        PaddedSingleSequence padSequence = new PaddedSingleSequence();
        /**
         * Construct a Sequencer with the selected wait strategy and buffer size.
         *
         * @param bufferSize the size of the buffer that this will sequence over.
         * @param waitStrategy for those waiting on sequences.
         */
        public SingleProducerSequencer(int bufferSize, IWaitStrategy waitStrategy):base(bufferSize,waitStrategy)
        {

            padSequence.nextValue = Sequence.INITIAL_VALUE;
            padSequence.cachedValue = Sequence.INITIAL_VALUE;

        }

        /**
         * @see Sequencer#hasAvailableCapacity(int)
         */
        
        public override Boolean hasAvailableCapacity(int requiredCapacity)
        {
            long nextValue = padSequence.nextValue;

            long wrapPoint = (nextValue + requiredCapacity) - bufferSize;
            long cachedGatingSequence = padSequence.cachedValue;

            if (wrapPoint > cachedGatingSequence || cachedGatingSequence > nextValue)
            {
                long minSequence = Util.Util.getMinimumSequence(gatingSequences, nextValue);
                padSequence.cachedValue = minSequence;

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

            long nextValue = padSequence.nextValue;

            long nextSequence = nextValue + n;
            long wrapPoint = nextSequence - bufferSize;
            long cachedGatingSequence = padSequence.cachedValue;

            if (wrapPoint > cachedGatingSequence || cachedGatingSequence > nextValue)
            {
                long minSequence;
                Int64 counter = 0;
                while (wrapPoint > (minSequence = Util.Util.getMinimumSequence(gatingSequences, nextValue)))
                {
                    counter++;
                    if (counter % 100 == 0)
                    {
                        Thread.Yield();
                    }
                    else if (counter % 10001 == 0)
                    {
                        Thread.Sleep(1);
                    }
                    //LockSupport.parkNanos(1L); // TODO: Use waitStrategy to spin?
                }
                padSequence.cachedValue = minSequence;
            }

            padSequence.nextValue = nextSequence;

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

            long nextSequence = padSequence.nextValue += n;

            return nextSequence;
        }

        /**
         * @see Sequencer#remainingCapacity()
         */
       
        public override long remainingCapacity()
        {
            long nextValue = padSequence.nextValue;

            long consumed = Util.Util.getMinimumSequence(gatingSequences, nextValue);
            long produced = nextValue;
            return getBufferSize() - (produced - consumed);
        }

        /**
         * @see Sequencer#claim(long)
         */
       
        public override void claim(long sequence)
        {
            padSequence.nextValue = sequence;
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
