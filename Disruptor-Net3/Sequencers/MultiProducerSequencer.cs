using Disruptor_Net3.Exceptions;
using Disruptor_Net3.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disruptor_Net3.Sequencers
{
    public class MultiProducerSequencer: AbstractSequencer
    {
        private Sequence gatingSequenceCache = new Sequence();
        private int[] availableBuffer;
        private int indexMask;
        private int indexShift;
        /**
         * Construct a Sequencer with the selected wait strategy and buffer size.
         *
         * @param bufferSize the size of the buffer that this will sequence over.
         * @param waitStrategy for those waiting on sequences.
         */
        public MultiProducerSequencer(int bufferSize, IWaitStrategy waitStrategy):base(bufferSize,waitStrategy)
        {
            availableBuffer = new int[bufferSize];
            indexMask = bufferSize - 1;
            indexShift = Util.Util.log2(bufferSize);
            initialiseAvailableBuffer();
        }
            public override void claim(long sequence)
        {
            cursor.set(sequence);
        }
        public override long remainingCapacity()
        {
            long consumed = Util.Util.getMinimumSequence(gatingSequences, cursor.get());
            long produced = cursor.get();
            return getBufferSize() - (produced - consumed);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long next()
        {
            return next(1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long next(int n)
        {
            if (n < 1)
            {
                throw new ArgumentException("n must be > 0");
            }

            long current;
            long next;
            Int64 counter = 0;
            do
            {
                current = cursor.get();
                next = current + n;

                long wrapPoint = next - bufferSize;
                long cachedGatingSequence = gatingSequenceCache.get();
                
                if (wrapPoint > cachedGatingSequence || cachedGatingSequence > current)
                {
                    long gatingSequence = Util.Util.getMinimumSequence(gatingSequences, current);

                    if (wrapPoint > gatingSequence)
                    {
                        counter++;
                        if(counter%100==0)
                        {
                            Thread.Yield();
                        }
                        else if(counter%1001==0)
                        {
                            Thread.Sleep(1);
                        }
                       // spinWait.SpinOnce(); ; // TODO, should we spin based on the wait strategy?
                        continue;
                    }

                    gatingSequenceCache.set(gatingSequence);
                }
                else if (cursor.compareAndSet(current, next))
                {
                    break;
                }
            }
            while (true);

            return next;
        }
        /// <summary>
        /// Returns -1 if not successful
        /// </summary>
        /// <returns></returns>
        public override long tryNext()
        {
            return tryNext(1);
        }
        /// <summary>
        /// returns -1 if not successful
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public override long tryNext(int n)
        {
            if (n < 1)
            {
                throw new ArgumentException("n must be > 0");
            }

            long current;
            long next;

            do
            {
                current = cursor.get();
                next = current + n;

                if (!hasAvailableCapacity(gatingSequences, n, current))
                {
                  
                    throw InsufficientCapacityException.INSTANCE;
                }
            }
            while (!cursor.compareAndSet(current, next));
            return next;
        }
        public override bool hasAvailableCapacity(int requiredCapacity)
        {
            return hasAvailableCapacity(gatingSequences, requiredCapacity, cursor.get());
        }
        private Boolean hasAvailableCapacity(Sequence[] gatingSequences, Int32 requiredCapacity, long cursorValue)
        {
            long wrapPoint = (cursorValue + requiredCapacity) - bufferSize;
            long cachedGatingSequence = gatingSequenceCache.get();

            if (wrapPoint > cachedGatingSequence || cachedGatingSequence > cursorValue)
            {
                long minSequence = Util.Util.getMinimumSequence(gatingSequences, cursorValue);
                gatingSequenceCache.set(minSequence);

                if (wrapPoint > minSequence)
                {
                    return false;
                }
            }
            return true;
        }
     
        private void initialiseAvailableBuffer()
        {
            for (int i = availableBuffer.Length - 1; i != 0; i--)
            {
                setAvailableBufferValue(i, -1);
            }

            setAvailableBufferValue(0, -1);
        }
         /**
         * The below methods work on the availableBuffer flag.
         *
         * The prime reason is to avoid a shared sequence object between publisher threads.
         * (Keeping single pointers tracking start and end would require coordination
         * between the threads).
         *
         * --  Firstly we have the constraint that the delta between the cursor and minimum
         * gating sequence will never be larger than the buffer size (the code in
         * next/tryNext in the Sequence takes care of that).
         * -- Given that; take the sequence value and mask off the lower portion of the
         * sequence as the index into the buffer (indexMask). (aka modulo operator)
         * -- The upper portion of the sequence becomes the value to check for availability.
         * ie: it tells us how many times around the ring buffer we've been (aka division)
         * -- Because we can't wrap without the gating sequences moving forward (i.e. the
         * minimum gating sequence is effectively our last available position in the
         * buffer), when we have new data and successfully claimed a slot we can simply
         * write over the top.
         */
        private void setAvailable(long sequence)
        {
            setAvailableBufferValue(calculateIndex(sequence), calculateAvailabilityFlag(sequence));
        }
        unsafe void putOrderedInt(int* obj, Int32 offset, int value)
        {
            unsafe
            {
                
                Thread.MemoryBarrier();
                obj[offset] = value;
            }
        }
      
        unsafe private void setAvailableBufferValue(int index, int flag)
        {
           Interlocked.Exchange(ref availableBuffer[index], flag);
        
        }
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int calculateAvailabilityFlag(long sequence)
        {
            return (int) (((UInt64)sequence) >> indexShift);
        }
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int calculateIndex(long sequence)
        {
            return ((int) sequence) & indexMask;
        }
        public override bool isAvailable(long sequence)
        {
            int index = calculateIndex(sequence);
            int flag = calculateAvailabilityFlag(sequence);
            return (Volatile.Read(ref availableBuffer[index]) == flag);
        }
        public override long getHighestPublishedSequence(long lowerBound, long availableSequence)
        {
            for (long sequence = lowerBound; sequence <= availableSequence; sequence++)
            {
                if (!isAvailable(sequence))
                {
                    return sequence - 1;
                }
            }

            return availableSequence;
        }
        
        /**
         * @see Sequencer#publish(long)
         */
   
        public override void publish(long sequence)
        {
            setAvailable(sequence);
            waitStrategy.signalAllWhenBlocking();
        }

        /**
         * @see Sequencer#publish(long, long)
         */
      
        public override void publish(long lo, long hi)
        {
            for (long l = lo; l <= hi; l++)
            {
                setAvailable(l);
            }
            waitStrategy.signalAllWhenBlocking();
        }


    }
}
