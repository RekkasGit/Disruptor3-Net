using Disruptor3_Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net
{
   public class EventPoller<T>
    {
        private IDataProvider<T> dataProvider;
        private ISequencer sequencer;
        private Sequence sequence;
        private Sequence gatingSequence;

        public interface Handler<T>
        {
            Boolean onEvent(T eventToUse, long sequence, Boolean endOfBatch);
        }

        public enum PollState
        {
            PROCESSING, GATING, IDLE
        }

        public EventPoller(IDataProvider<T> dataProvider,ISequencer sequencer,Sequence sequence,Sequence gatingSequence)
        {
            this.dataProvider = dataProvider;
            this.sequencer = sequencer;
            this.sequence = sequence;
            this.gatingSequence = gatingSequence;
        }

        public PollState poll(Handler<T> eventHandler)
        {
            long currentSequence = sequence.get();
            long nextSequence = currentSequence + 1;
            long availableSequence = sequencer.getHighestPublishedSequence(nextSequence, gatingSequence.get());

            if (nextSequence <= availableSequence)
            {
                Boolean processNextEvent;
                long processedSequence = currentSequence;

                try
                {
                    do
                    {
                        T eventToUse = dataProvider.get(nextSequence);
                        processNextEvent = eventHandler.onEvent(eventToUse, nextSequence, nextSequence == availableSequence);
                        processedSequence = nextSequence;
                        nextSequence++;

                    }
                    while (nextSequence <= availableSequence & processNextEvent);
                }
                finally
                {
                    sequence.set(processedSequence);
                }

                return PollState.PROCESSING;
            }
            else if (sequencer.getCursor() >= nextSequence)
            {
                return PollState.GATING;
            }
            else
            {
                return PollState.IDLE;
            }
        }

        public static EventPoller<T> newInstance<T>(IDataProvider<T> dataProvider, ISequencer sequencer, Sequence sequence, Sequence cursorSequence, params Sequence[] gatingSequences)
        {
            Sequence gatingSequence;
            if (gatingSequences.Length == 0)
            {
                gatingSequence = cursorSequence;
            }
            else if (gatingSequences.Length == 1)
            {
                gatingSequence = gatingSequences[0];
            }
            else
            {
                gatingSequence = new FixedSequenceGroup(gatingSequences);
            }

            return new EventPoller<T>(dataProvider, sequencer, sequence, gatingSequence);
        }

        public Sequence getSequence()
        {
            return sequence;
        }
    }
}
