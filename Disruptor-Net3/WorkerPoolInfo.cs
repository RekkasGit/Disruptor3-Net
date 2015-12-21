using Disruptor_Net3.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor_Net3
{
   class WorkerPoolInfo<T>:IConsumerInfo
    {
        private WorkerPool<T> workerPool;
        private ISequenceBarrier sequenceBarrier;
        private Boolean endOfChain = true;

        public WorkerPoolInfo(WorkerPool<T> workerPool, ISequenceBarrier sequenceBarrier)
        {
            this.workerPool = workerPool;
            this.sequenceBarrier = sequenceBarrier;
        }
        public Sequence[] getSequences()
        {
            return workerPool.getWorkerSequences();
        }
        public ISequenceBarrier getBarrier()
        {
            return sequenceBarrier;
        }
        public Boolean isEndOfChain()
        {
            return endOfChain;
        }
        public void start()
        {
            workerPool.start();
        }
        public void halt()
        {
            workerPool.halt();
        }
 
        public void markAsUsedInBarrier()
        {
            endOfChain = false;
        }

        public Boolean isRunning()
        {
            return workerPool.isRunning();
        }
    }
}
