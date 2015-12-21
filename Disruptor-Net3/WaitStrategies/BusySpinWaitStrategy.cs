using Disruptor_Net3.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor_Net3.WaitStrategies
{
   public class BusySpinWaitStrategy:IWaitStrategy
    {
        public long waitFor(long sequence, Sequence cursor, Sequence dependentSequence, ISequenceBarrier barrier)
        {
            long availableSequence;

            while ((availableSequence = dependentSequence.get()) < sequence)
            {
                barrier.checkAlert();
            }

            return availableSequence;
        }
         public void signalAllWhenBlocking()
        {
        }
    }
}
