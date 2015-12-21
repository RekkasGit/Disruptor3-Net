using Disruptor_Net3.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor_Net3.dsl
{

    /**
     * Provides a repository mechanism to associate {@link EventHandler}s with {@link EventProcessor}s
     *
     * @param <T> the type of the {@link EventHandler}
     */
    public class ConsumerRepository<T> : IEnumerable<IConsumerInfo>
    {
        private Dictionary<IEventHandler<T>, EventProcessorInfo<T>> eventProcessorInfoByEventHandler = new Dictionary<IEventHandler<T>, EventProcessorInfo<T>>();
        private Dictionary<Sequence, IConsumerInfo> eventProcessorInfoBySequence = new Dictionary<Sequence, IConsumerInfo>();
        private List<IConsumerInfo> consumerInfos = new List<IConsumerInfo>();

        public void add(IEventProcessor eventprocessor,
                        IEventHandler<T> handler,
                        ISequenceBarrier barrier)
        {
            EventProcessorInfo<T> consumerInfo = new EventProcessorInfo<T>(eventprocessor, handler, barrier);
            eventProcessorInfoByEventHandler.Add(handler, consumerInfo);
            eventProcessorInfoBySequence.Add(eventprocessor.getSequence(), consumerInfo);
            consumerInfos.Add(consumerInfo);
        }

        public void add(IEventProcessor processor)
        {
            EventProcessorInfo<T> consumerInfo = new EventProcessorInfo<T>(processor, null, null);
            eventProcessorInfoBySequence.Add(processor.getSequence(), consumerInfo);
            consumerInfos.Add(consumerInfo);
        }

        public void add(WorkerPool<T> workerPool, ISequenceBarrier sequenceBarrier)
        {
            WorkerPoolInfo<T> workerPoolInfo = new WorkerPoolInfo<T>(workerPool, sequenceBarrier);
            consumerInfos.Add(workerPoolInfo);
            foreach (Sequence sequence in workerPool.getWorkerSequences())
            {
                eventProcessorInfoBySequence.Add(sequence, workerPoolInfo);
            }
        }

        public Sequence[] getLastSequenceInChain(Boolean includeStopped)
        {
            List<Sequence> lastSequence = new List<Sequence>();
            foreach (IConsumerInfo consumerInfo in consumerInfos)
            {
                if ((includeStopped || consumerInfo.isRunning()) && consumerInfo.isEndOfChain())
                {
                    Sequence[] sequences = consumerInfo.getSequences();
                    lastSequence.AddRange(sequences);
                }
            }

            return lastSequence.ToArray();
        }

        public IEventProcessor getEventProcessorFor(IEventHandler<T> handler)
        {
            EventProcessorInfo<T> eventprocessorInfo = getEventProcessorInfo(handler);
            if (eventprocessorInfo == null)
            {
                throw new ArgumentOutOfRangeException("The event handler " + handler + " is not processing events.");
            }

            return eventprocessorInfo.getEventProcessor();
        }

        public Sequence getSequenceFor(IEventHandler<T> handler)
        {
            return getEventProcessorFor(handler).getSequence();
        }

        public void unMarkEventProcessorsAsEndOfChain(params Sequence[] barrierEventProcessors)
        {
            foreach (Sequence barrierEventProcessor in barrierEventProcessors)
            {
                getEventProcessorInfo(barrierEventProcessor).markAsUsedInBarrier();
            }
        }
        public IEnumerator<IConsumerInfo> GetEnumerator()
        {
            return consumerInfos.GetEnumerator();
        }

        public IEnumerator<IConsumerInfo> iterator()
        {
            return consumerInfos.GetEnumerator();
        }

        public ISequenceBarrier getBarrierFor(IEventHandler<T> handler)
        {
            IConsumerInfo consumerInfo = getEventProcessorInfo(handler);
            return consumerInfo != null ? consumerInfo.getBarrier() : null;
        }
        private EventProcessorInfo<T> getEventProcessorInfo(IEventHandler<T> handler)
        {
            return eventProcessorInfoByEventHandler[handler];
        }

        private IConsumerInfo getEventProcessorInfo(Sequence barrierEventProcessor)
        {
            return eventProcessorInfoBySequence[barrierEventProcessor];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return consumerInfos.GetEnumerator();
        }
    }
}