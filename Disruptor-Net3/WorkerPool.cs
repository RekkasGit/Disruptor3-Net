using Disruptor_Net3.Interfaces;
using Disruptor_Net3.WaitStrategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Disruptor_Net3
{
   /**
     * WorkerPool contains a pool of {@link WorkProcessor}s that will consume sequences so jobs can be farmed out across a pool of workers.
     * Each of the {@link WorkProcessor}s manage and calls a {@link WorkHandler} to process the events.
     *
     * @param <T> event to be processed by a pool of workers
     */
    public class WorkerPool<T>
    {
        private Boolean started = false;
        private Sequence workSequence = new Sequence();
        private RingBuffer<T> ringBuffer;
        // WorkProcessors are created to wrap each of the provided WorkHandlers
        private WorkProcessor<T>[] workProcessors;

        /**
         * Create a worker pool to enable an array of {@link WorkHandler}s to consume published sequences.
         *
         * This option requires a pre-configured {@link RingBuffer} which must have {@link RingBuffer#addGatingSequences(Sequence...)}
         * called before the work pool is started.
         *
         * @param ringBuffer of events to be consumed.
         * @param sequenceBarrier on which the workers will depend.
         * @param exceptionHandler to callback when an error occurs which is not handled by the {@link WorkHandler}s.
         * @param workHandlers to distribute the work load across.
         */
        public WorkerPool(RingBuffer<T> ringBuffer,
                          ISequenceBarrier sequenceBarrier,
                          IExceptionHandler<T> exceptionHandler,
                          params IWorkHandler<T>[] workHandlers)
        {
            this.ringBuffer = ringBuffer;
            int numWorkers = workHandlers.Length;
            workProcessors = new WorkProcessor<T>[numWorkers];

            for (int i = 0; i < numWorkers; i++)
            {
                workProcessors[i] = new WorkProcessor<T>(ringBuffer,
                                                         sequenceBarrier,
                                                         workHandlers[i],
                                                         exceptionHandler,
                                                         workSequence);
            }
        }

        /**
         * Construct a work pool with an internal {@link RingBuffer} for convenience.
         *
         * This option does not require {@link RingBuffer#addGatingSequences(Sequence...)} to be called before the work pool is started.
         *
         * @param eventFactory for filling the {@link RingBuffer}
         * @param exceptionHandler to callback when an error occurs which is not handled by the {@link WorkHandler}s.
         * @param workHandlers to distribute the work load across.
         */
        public WorkerPool(IEventFactory<T> eventFactory,
                          IExceptionHandler<T> exceptionHandler,
                          params IWorkHandler<T>[] workHandlers)
        {
            ringBuffer = RingBuffer<T>.createMultiProducer(eventFactory, 1024, new BlockingWaitStrategy());
            ISequenceBarrier barrier = ringBuffer.newBarrier();
            int numWorkers = workHandlers.Length;
            workProcessors = new WorkProcessor<T>[numWorkers];

            for (int i = 0; i < numWorkers; i++)
            {
                workProcessors[i] = new WorkProcessor<T>(ringBuffer,
                                                         barrier,
                                                         workHandlers[i],
                                                         exceptionHandler,
                                                         workSequence);
            }

            ringBuffer.addGatingSequences(getWorkerSequences());
        }

        /**
         * Get an array of {@link Sequence}s representing the progress of the workers.
         *
         * @return an array of {@link Sequence}s representing the progress of the workers.
         */
        public Sequence[] getWorkerSequences()
        {
            Sequence[] sequences = new Sequence[workProcessors.Length + 1];
            for (int i = 0, size = workProcessors.Length; i < size; i++)
            {
                sequences[i] = workProcessors[i].getSequence();
            }
            sequences[sequences.Length - 1] = workSequence;

            return sequences;
        }

        /**
         * Start the worker pool processing events in sequence.
         *
         * @param executor providing threads for running the workers.
         * @return the {@link RingBuffer} used for the work queue.
         * @throws IllegalStateException if the pool has already been started and not halted yet
         */
        public RingBuffer<T> start()
        {
            
            if (Volatile.Read(ref started))
            {
                throw new System.InvalidOperationException("WorkerPool has already been started and cannot be restarted until halted.");
            }

            long cursor = ringBuffer.getCursor();
            workSequence.set(cursor);

            foreach (WorkProcessor<T> processor in workProcessors)
            {
                processor.getSequence().set(cursor);
                Task.Factory.StartNew(() => processor.run(), TaskCreationOptions.LongRunning);
            }

            return ringBuffer;
        }

        /**
         * Wait for the {@link RingBuffer} to drain of published events then halt the workers.
         */
        public void drainAndHalt()
        {
            Sequence[] workerSequences = getWorkerSequences();
            while (ringBuffer.getCursor() > Util.Util.getMinimumSequence(workerSequences))
            {
                Thread.Yield();
            }

            foreach (WorkProcessor<T> processor in workProcessors)
            {
                processor.halt();
            }
            Volatile.Write(ref started, false);
        }

        /**
         * Halt all workers immediately at the end of their current cycle.
         */
        public void halt()
        {
            foreach (WorkProcessor<T> processor in workProcessors)
            {
                processor.halt();
            }

            Volatile.Write(ref started, false);
        }

        public Boolean isRunning()
        {
            return Volatile.Read(ref started);
        }
    }
}
