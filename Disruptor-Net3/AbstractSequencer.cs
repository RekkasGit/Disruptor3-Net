using Disruptor3_Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net
{
    public abstract class AbstractSequencer : ISequencer
    {
        /** Set to -1 as sequence starting point */
        static long INITIAL_CURSOR_VALUE = -1L;
        public static Int32 numberToPad = IntPtr.Size; // want to padd the gate sequence
        public AbstractSequencer(Int32 bufferSize,IWaitStrategy waitStrat)
        {
            this.bufferSize = bufferSize;
            this.waitStrategy = waitStrat;
            
        }

        protected int bufferSize;
        protected IWaitStrategy waitStrategy;
        protected Sequence cursor = new Sequence(INITIAL_CURSOR_VALUE);
        protected Sequence[] gatingSequences = new Sequence[(numberToPad*2)];

        public abstract void claim(long sequence);
        public abstract bool isAvailable(long sequence);
        public void addGatingSequences(params Sequence[] newGatingSequences)
        {
            long cursorSequence;
            Sequence[] updatedSequences;
            Sequence[] currentSequences;

            do
            {
                currentSequences = gatingSequences;
                updatedSequences = new Sequence[currentSequences.Length + newGatingSequences.Length];
                Array.Copy(currentSequences, 0, updatedSequences,0,currentSequences.Length);
                Array.Copy(newGatingSequences, 0, updatedSequences,currentSequences.Length-numberToPad, newGatingSequences.Length);
                cursorSequence = this.getCursor();
                int index = currentSequences.Length-numberToPad;
                foreach (Sequence sequence in newGatingSequences)
                {
                    sequence.set(cursorSequence);
                    updatedSequences[index++] = sequence;
                }
            }
            while (System.Threading.Interlocked.CompareExchange(ref this.gatingSequences, updatedSequences, currentSequences) != currentSequences);

            cursorSequence = this.getCursor();
            foreach (Sequence sequence in  newGatingSequences)
            {
                sequence.set(cursorSequence);
            }
            
        }

        public bool removeGatingSequence(Sequence sequence)
        {
            //TODO: haven't really tested the remove gate sequence with the padding... 
            Sequence[] oldSequences;
            Sequence[] newSequences;
            do
            {
                oldSequences = gatingSequences;
                int oldSize = oldSequences.Length;


                Int32 numberToremove = 0;
                for (Int32 i = numberToPad; i < oldSequences.Length -numberToPad; i++)
                {
                    if (oldSequences[i].get() != sequence.get())
                    {
                        numberToremove++;
                    }
                }
                if (numberToremove == 0)
                {
                    return false;
                }

                newSequences = new Sequence[oldSize - numberToremove];
                Int32 newCounter = numberToPad;
                for (Int32 i = numberToPad; i < oldSequences.Length - numberToPad; i++)
                {
                    if (oldSequences[i].get() != sequence.get())
                    {
                        newSequences[newCounter] = sequence;
                        newCounter++;
                    }
                }
            }
            while (System.Threading.Interlocked.CompareExchange(ref this.gatingSequences, newSequences, oldSequences) != oldSequences);
            return true;

        }
        public ISequenceBarrier newBarrier(params Sequence[] sequencesToTrack)
        {
            return new ProcessingSequenceBarrier(this, waitStrategy, cursor, sequencesToTrack);
        }
        public long getMinimumSequence()
        {
            return Util.Util.getMinimumSequence(gatingSequences, cursor.get());
        }
        public abstract long getHighestPublishedSequence(long nextSequence, long availableSequence);
        public long getCursor()
        {
            return cursor.get();
        }
        public int getBufferSize()
        {
            return bufferSize;
        }

        public abstract bool hasAvailableCapacity(int requiredCapacity);
        public abstract long remainingCapacity();
        public abstract long next();
        public abstract long next(int n);
        public abstract long tryNext();
        public abstract long tryNext(int n);
        public abstract void publish(long sequence);
        public abstract void publish(long lo, long hi);

        public EventPoller<T> newPoller<T>(IDataProvider<T> dataProvider, params Sequence[] gatingSequences)
        {
            return EventPoller<T>.newInstance(dataProvider, this, new Sequence(), cursor, gatingSequences);
        }

    }
}
