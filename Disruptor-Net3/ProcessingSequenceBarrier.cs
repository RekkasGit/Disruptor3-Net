using Disruptor_Net3.Exceptions;
using Disruptor_Net3.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor_Net3
{
   class ProcessingSequenceBarrier:ISequenceBarrier
    {
        private IWaitStrategy waitStrategy;
        private Sequence dependentSequence;
        private volatile Boolean alerted = false;
        private Sequence cursorSequence;
        private ISequencer sequencer;

        public ProcessingSequenceBarrier(ISequencer sequencer,IWaitStrategy waitStrategy,Sequence cursorSequence,Sequence[] dependentSequences)
        {
            this.sequencer = sequencer;
            this.waitStrategy = waitStrategy;
            this.cursorSequence = cursorSequence;
            if (0 == dependentSequences.Length)
            {
                dependentSequence = cursorSequence;
            }
            else
            {
                dependentSequence = new FixedSequenceGroup(dependentSequences);
            }
        }

       
        public long waitFor(long sequence)
        {
            checkAlert();

            long availableSequence = waitStrategy.waitFor(sequence, cursorSequence, dependentSequence, this);

            if (availableSequence < sequence)
            {
                return availableSequence;
            }

            return sequencer.getHighestPublishedSequence(sequence, availableSequence);
        }

       
        public long getCursor()
        {
            return dependentSequence.get();
        }

       
        public Boolean isAlerted()
        {
            return alerted;
        }

       
        public void alert()
        {
            alerted = true;
            waitStrategy.signalAllWhenBlocking();
        }

       
        public void clearAlert()
        {
            alerted = false;
        }

       
        public void checkAlert()
        {
            if (alerted)
            {
                throw AlertException.INSTANCE;
            }
        }
    }
}
