using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor_Net3.Interfaces
{
    public interface IConsumerInfo
    {
        Sequence[] getSequences();

        ISequenceBarrier getBarrier();

        Boolean isEndOfChain();

        void start();

        void halt();

        void markAsUsedInBarrier();

        Boolean isRunning();
    }
}
