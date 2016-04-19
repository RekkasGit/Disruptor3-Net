using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.Interfaces
{
   
    /**
     * Coordinates claiming sequences for access to a data structure while tracking dependent {@link Sequence}s
     */
    public interface ISequencer:ICursored,ISequenced
    {
   

        /**
         * Claim a specific sequence.  Only used if initialising the ring buffer to
         * a specific value.
         *
         * @param sequence The sequence to initialise too.
         */
        void claim(long sequence);

        /**
         * Confirms if a sequence is published and the event is available for use; non-blocking.
         *
         * @param sequence of the buffer to check
         * @return true if the sequence is available for use, false if not
         */
        Boolean isAvailable(long sequence);

        /**
         * Add the specified gating sequences to this instance of the Disruptor.  They will
         * safely and atomically added to the list of gating sequences.
         *
         * @param gatingSequences The sequences to add.
         */
        void addGatingSequences(params Sequence[] gatingSequences);

        /**
         * Remove the specified sequence from this sequencer.
         *
         * @param sequence to be removed.
         * @return <tt>true</tt> if this sequence was found, <tt>false</tt> otherwise.
         */
        Boolean removeGatingSequence(Sequence sequence);

        /**
         * Create a new SequenceBarrier to be used by an EventProcessor to track which messages
         * are available to be read from the ring buffer given a list of sequences to track.
         *
         * @see SequenceBarrier
         * @param sequencesToTrack
         * @return A sequence barrier that will track the specified sequences.
         */
        ISequenceBarrier newBarrier(params Sequence[] sequencesToTrack);

        /**
         * Get the minimum sequence value from all of the gating sequences
         * added to this ringBuffer.
         *
         * @return The minimum gating sequence or the cursor sequence if
         * no sequences have been added.
         */
        long getMinimumSequence();

        /**
         * Get the highest sequence number that can be safely read from the ring buffer.  Depending
         * on the implementation of the Sequencer this call may need to scan a number of values
         * in the Sequencer.  The scan will range from nextSequence to availableSequence.  If
         * there are no available values <code>&gt;= nextSequence</code> the return value will be
         * <code>nextSequence - 1</code>.  To work correctly a consumer should pass a value that
         * it 1 higher than the last sequence that was successfully processed.
         *
         * @param nextSequence The sequence to start scanning from.
         * @param availableSequence The sequence to scan to.
         * @return The highest value that can be safely read, will be at least <code>nextSequence - 1</code>.
         */
        long getHighestPublishedSequence(long nextSequence, long availableSequence);

        EventPoller<T> newPoller<T>(IDataProvider<T> provider, params Sequence[] gatingSequences);
    }
}
