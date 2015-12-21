using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor_Net3.Util;
using System.Runtime.CompilerServices;
using Disruptor_Net3.Interfaces;

namespace Disruptor_Net3
{
   /**
 * A {@link Sequence} group that can dynamically have {@link Sequence}s added and removed while being
 * thread safe.
 * <p>
 * The {@link SequenceGroup#get()} and {@link SequenceGroup#set(long)} methods are lock free and can be
 * concurrently be called with the {@link SequenceGroup#add(Sequence)} and {@link SequenceGroup#remove(Sequence)}.
 */
    public class SequenceGroup:Sequence
    {
        private Sequence[] sequences = new Sequence[0];

        /**
         * Default Constructor
         */
        public SequenceGroup():base(-1)
        {
       
        }

        /**
         * Get the minimum sequence value for the group.
         *
         * @return the minimum sequence value for the group.
         */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long get()
        {
            return Util.Util.getMinimumSequence(sequences);
        }

        /**
         * Set all {@link Sequence}s in the group to a given value.
         *
         * @param value to set the group of sequences to.
         */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void set(long value)
        {
            Sequence[] sequences = this.sequences;
            for (int i = 0, size = sequences.Length; i < size; i++)
            {
                sequences[i].set(value);
            }
        }

        /**
         * Add a {@link Sequence} into this aggregate.  This should only be used during
         * initialisation.  Use {@link SequenceGroup#addWhileRunning(Cursored, Sequence)}
         *
         * @see SequenceGroup#addWhileRunning(Cursored, Sequence)
         * @param sequence to be added to the aggregate.
         */
        public void add(Sequence sequence)
        {
            Sequence[] oldSequences;
            Sequence[] newSequences;
            do
            {
                oldSequences = sequences;
                int oldSize = oldSequences.Length;
                newSequences = new Sequence[oldSize + 1];
                System.Array.Copy(oldSequences, 0, newSequences, 0, oldSize);
                newSequences[oldSize] = sequence;
           
            }
            while (System.Threading.Interlocked.CompareExchange(ref this.sequences, newSequences, oldSequences) != oldSequences);
        }

        /**
         * Remove the first occurrence of the {@link Sequence} from this aggregate.
         *
         * @param sequence to be removed from this aggregate.
         * @return true if the sequence was removed otherwise false.
         */
        public Boolean remove(Sequence sequence)
        {
            Sequence[] oldSequences;
            Sequence[] newSequences;
            do
            {
                oldSequences = sequences;
                int oldSize = oldSequences.Length;
           

                Int32 numberToremove = 0;
                for (Int32 i = 0; i < oldSequences.Length; i++)
                {
                    if (oldSequences[i].get() != sequence.get())
                    {
                        numberToremove++;
                    }
                }
                if(numberToremove==0)
                {
                    return false;
                }

                newSequences = new Sequence[oldSize - numberToremove];
                Int32 newCounter = 0;
                for (Int32 i = 0; i < oldSequences.Length;i++)
                {
                    if(oldSequences[i].get()!=sequence.get())
                    {
                        newSequences[newCounter] = sequence;
                        newCounter++;
                    }
                }
            }
            while (System.Threading.Interlocked.CompareExchange(ref this.sequences, newSequences, oldSequences) != oldSequences);
            return true;
        }

        /**
         * Get the size of the group.
         *
         * @return the size of the group.
         */
        public int size()
        {
            return sequences.Length;
        }

        /**
         * Adds a sequence to the sequence group after threads have started to publish to
         * the Disruptor.  It will set the sequences to cursor value of the ringBuffer
         * just after adding them.  This should prevent any nasty rewind/wrapping effects.
         *
         * @param cursored The data structure that the owner of this sequence group will
         * be pulling it's events from.
         * @param sequence The sequence to add.
         */
        public void addWhileRunning(ICursored cursored, Sequence sequence)
        {
            long cursorSequence;
            Sequence[] updatedSequences;
            Sequence[] currentSequences;

            do
            {
                currentSequences = sequences;
                updatedSequences = new Sequence[currentSequences.Length + 1];
                Array.Copy(currentSequences, updatedSequences, currentSequences.Length);
                updatedSequences[currentSequences.Length] = sequence;
                cursorSequence = cursored.getCursor();
                int index = currentSequences.Length;
                sequence.set(cursorSequence);
                updatedSequences[index++] = sequence;
            }
            while (System.Threading.Interlocked.CompareExchange(ref this.sequences, updatedSequences, currentSequences)!=currentSequences);

            cursorSequence = cursored.getCursor();
            sequence.set(cursorSequence);
        }
    }
}
