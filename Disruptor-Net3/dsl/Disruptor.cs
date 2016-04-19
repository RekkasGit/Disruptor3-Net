using Disruptor3_Net.Interfaces;
using Disruptor3_Net.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace Disruptor3_Net.dsl
{
   /**
 * A DSL-style API for setting up the disruptor pattern around a ring buffer
 * (aka the Builder pattern).
 *
 * <p>
 * A simple example of setting up the disruptor with two event handlers that
 * must process events in order:
 * </p>
 *
 * <pre>
 * <code>Disruptor&lt;MyEvent&gt; disruptor = new Disruptor&lt;MyEvent&gt;(MyEvent.FACTORY, 32, Executors.newCachedThreadPool());
 * EventHandler&lt;MyEvent&gt; handler1 = new EventHandler&lt;MyEvent&gt;() { ... };
 * EventHandler&lt;MyEvent&gt; handler2 = new EventHandler&lt;MyEvent&gt;() { ... };
 * disruptor.handleEventsWith(handler1);
 * disruptor.after(handler1).handleEventsWith(handler2);
 *
 * RingBuffer ringBuffer = disruptor.start();</code>
 * </pre>
 *
 * @param <T> the type of event used.
 */
    public class Disruptor<T>
    {
        private RingBuffer<T> ringBuffer;
        private ConsumerRepository<T> consumerRepository = new ConsumerRepository<T>();
        private Int32 started = 0; // 0 false, 1 true
        private IExceptionHandler<T> exceptionHandler;

        /**
         * Create a new Disruptor. Will default to {@link com.lmax.disruptor.BlockingWaitStrategy} and
         * {@link ProducerType}.MULTI
         *
         * @param eventFactory
         *            the factory to create events in the ring buffer.
         * @param ringBufferSize
         *            the size of the ring buffer.
         * @param executor
         *            an {@link Executor} to execute event processors.
         */
        public Disruptor(IEventFactory<T> eventFactory, int ringBufferSize):this( RingBuffer<T>.createMultiProducer<T>(eventFactory, ringBufferSize))
        {
           
        }

        /**
         * Create a new Disruptor.
         *
         * @param eventFactory   the factory to create events in the ring buffer.
         * @param ringBufferSize the size of the ring buffer, must be power of 2.
         * @param executor       an {@link Executor} to execute event processors.
         * @param producerType   the claim strategy to use for the ring buffer.
         * @param waitStrategy   the wait strategy to use for the ring buffer.
         */
        public Disruptor(IEventFactory<T> eventFactory,
                         int ringBufferSize,
                         ProducerType producerType,
                         IWaitStrategy waitStrategy)
            : this(RingBuffer<T>.create<T>(producerType, eventFactory, ringBufferSize, waitStrategy))
        {
           
        }

        /**
         * Private constructor helper
         */
        private Disruptor(RingBuffer<T> ringBuffer)
        {
            this.ringBuffer = ringBuffer;
            
        }

        /**
         * <p>Set up event handlers to handle events from the ring buffer. These handlers will process events
         * as soon as they become available, in parallel.</p>
         *
         * <p>This method can be used as the start of a chain. For example if the handler <code>A</code> must
         * process events before handler <code>B</code>:</p>
         * <pre><code>dw.handleEventsWith(A).then(B);</code></pre>
         *
         * @param handlers the event handlers that will process events.
         * @return a {@link EventHandlerGroup} that can be used to chain dependencies.
         */
        public EventHandlerGroup<T> handleEventsWith(params IEventHandler<T>[] handlers)
        {
            return createEventProcessors(new Sequence[0], handlers);
        }

        /**
         * <p>Set up custom event processors to handle events from the ring buffer. The Disruptor will
         * automatically start these processors when {@link #start()} is called.</p>
         *
         * <p>This method can be used as the start of a chain. For example if the handler <code>A</code> must
         * process events before handler <code>B</code>:</p>
         * <pre><code>dw.handleEventsWith(A).then(B);</code></pre>
         *
         * <p>Since this is the start of the chain, the processor factories will always be passed an empty <code>Sequence</code>
         * array, so the factory isn't necessary in this case. This method is provided for consistency with
         * {@link EventHandlerGroup#handleEventsWith(EventProcessorFactory...)} and {@link EventHandlerGroup#then(EventProcessorFactory...)}
         * which do have barrier sequences to provide.</p>
         *
         * @param eventProcessorFactories the event processor factories to use to create the event processors that will process events.
         * @return a {@link EventHandlerGroup} that can be used to chain dependencies.
         */
        public EventHandlerGroup<T> handleEventsWith(params EventProcessorFactory<T>[] eventProcessorFactories)
        {
            Sequence[] barrierSequences = new Sequence[0];
            return createEventProcessors(barrierSequences, eventProcessorFactories);
        }

        /**
         * <p>Set up custom event processors to handle events from the ring buffer. The Disruptor will
         * automatically start this processors when {@link #start()} is called.</p>
         *
         * <p>This method can be used as the start of a chain. For example if the processor <code>A</code> must
         * process events before handler <code>B</code>:</p>
         * <pre><code>dw.handleEventsWith(A).then(B);</code></pre>
         *
         * @param processors the event processors that will process events.
         * @return a {@link EventHandlerGroup} that can be used to chain dependencies.
         */
        public EventHandlerGroup<T> handleEventsWith(params IEventProcessor[] processors)
        {
            foreach (IEventProcessor processor in processors)
            {
                consumerRepository.add(processor);
            }
            return new EventHandlerGroup<T>(this, consumerRepository, Util.Util.getSequencesFor(processors));
        }


        /**
         * Set up a {@link WorkerPool} to distribute an event to one of a pool of work handler threads.
         * Each event will only be processed by one of the work handlers.
         * The Disruptor will automatically start this processors when {@link #start()} is called.
         *
         * @param workHandlers the work handlers that will process events.
         * @return a {@link EventHandlerGroup} that can be used to chain dependencies.
         */
        public EventHandlerGroup<T> handleEventsWithWorkerPool(params IWorkHandler<T>[] workHandlers)
        {
            return createWorkerPool(new Sequence[0], workHandlers);
        }

        /**
         * <p>Specify an exception handler to be used for any future event handlers.</p>
         *
         * <p>Note that only event handlers set up after calling this method will use the exception handler.</p>
         *
         * @param exceptionHandler the exception handler to use for any future {@link EventProcessor}.
         */
        public void handleExceptionsWith(IExceptionHandler<T> exceptionHandler)
        {
            this.exceptionHandler = exceptionHandler;
        }

        /**
         * Override the default exception handler for a specific handler.
         * <pre>disruptorWizard.handleExceptionsIn(eventHandler).with(exceptionHandler);</pre>
         *
         * @param eventHandler the event handler to set a different exception handler for.
         * @return an ExceptionHandlerSetting dsl object - intended to be used by chaining the with method call.
         */
        public ExceptionHandlerSetting<T> handleExceptionsFor(IEventHandler<T> eventHandler)
        {
            return new ExceptionHandlerSetting<T>(eventHandler, consumerRepository);
        }

        /**
         * <p>Create a group of event handlers to be used as a dependency.
         * For example if the handler <code>A</code> must process events before handler <code>B</code>:</p>
         *
         * <pre><code>dw.after(A).handleEventsWith(B);</code></pre>
         *
         * @param handlers the event handlers, previously set up with {@link #handleEventsWith(com.lmax.disruptor.EventHandler[])},
         *                 that will form the barrier for subsequent handlers or processors.
         * @return an {@link EventHandlerGroup} that can be used to setup a dependency barrier over the specified event handlers.
         */
        public EventHandlerGroup<T> after(params IEventHandler<T>[] handlers)
        {
            Sequence[] sequences = new Sequence[handlers.Length];
            for (int i = 0, handlersLength = handlers.Length; i < handlersLength; i++)
            {
                sequences[i] = consumerRepository.getSequenceFor(handlers[i]);
            }

            return new EventHandlerGroup<T>(this, consumerRepository, sequences);
        }

        /**
         * Create a group of event processors to be used as a dependency.
         *
         * @param processors the event processors, previously set up with {@link #handleEventsWith(com.lmax.disruptor.EventProcessor...)},
         *                   that will form the barrier for subsequent handlers or processors.
         * @return an {@link EventHandlerGroup} that can be used to setup a {@link SequenceBarrier} over the specified event processors.
         * @see #after(com.lmax.disruptor.EventHandler[])
         */
        public EventHandlerGroup<T> after(params IEventProcessor[] processors)
        {
            foreach (IEventProcessor processor in processors)
            {
                consumerRepository.add(processor);
            }

            return new EventHandlerGroup<T>(this, consumerRepository, Util.Util.getSequencesFor(processors));
        }

        /**
         * Publish an event to the ring buffer.
         *
         * @param eventTranslator the translator that will load data into the event.
         */
        public void publishEvent(IEventTranslator<T> eventTranslator)
        {
            ringBuffer.publishEvent(eventTranslator);
        }

        /**
         * Publish an event to the ring buffer.
         *
         * @param eventTranslator the translator that will load data into the event.
         * @param arg A single argument to load into the event
         */
        public void publishEvent<A>(IEventTranslatorOneArg<T, A> eventTranslator, A arg)
        {
            ringBuffer.publishEvent(eventTranslator, arg);
        }

        /**
         * Publish a batch of events to the ring buffer.
         *
         * @param eventTranslator the translator that will load data into the event.
         * @param arg An array single arguments to load into the events. One Per event.
         */
        public void publishEvents<A>(IEventTranslatorOneArg<T, A> eventTranslator, A[] arg)
        {
            ringBuffer.publishEvents(eventTranslator, arg);
        }

        /**
         * <p>Starts the event processors and returns the fully configured ring buffer.</p>
         *
         * <p>The ring buffer is set up to prevent overwriting any entry that is yet to
         * be processed by the slowest event processor.</p>
         *
         * <p>This method must only be called once after all event processors have been added.</p>
         *
         * @return the configured ring buffer.
         */
        public RingBuffer<T> start()
        {
            Sequence[] gatingSequences = consumerRepository.getLastSequenceInChain(true);
            ringBuffer.addGatingSequences(gatingSequences);

            checkOnlyStartedOnce();
            foreach(IConsumerInfo consumerInfo in consumerRepository)
            {
                consumerInfo.start();
            }

            return ringBuffer;
        }

        /**
         * Calls {@link com.lmax.disruptor.EventProcessor#halt()} on all of the event processors created via this disruptor.
         */
        public void halt()
        {
            foreach(IConsumerInfo consumerInfo in consumerRepository)
            {
                consumerInfo.halt();
            }
        }

        /**
         * Waits until all events currently in the disruptor have been processed by all event processors
         * and then halts the processors.  It is critical that publishing to the ring buffer has stopped
         * before calling this method, otherwise it may never return.
         *
         * <p>This method will not shutdown the executor, nor will it await the termination of the
         * processor threads.</p>
         */
        public void shutdown()
        {
            try
            {
                shutdown(-1);
            }
            catch (Disruptor3_Net.Exceptions.TimeoutException e)
            {
                exceptionHandler.handleOnShutdownException(e);
            }
        }

        /**
         * <p>Waits until all events currently in the disruptor have been processed by all event processors
         * and then halts the processors.</p>
         *
         * <p>This method will not shutdown the executor, nor will it await the termination of the
         * processor threads.</p>
         *
         * @param timeout  the amount of time to wait for all events to be processed. <code>-1</code> will give an infinite timeout
         * @param timeUnit the unit the timeOut is specified in
         */
        public void shutdown(long timeout)
        {
            long timeOutAt = System.DateTime.UtcNow.Millisecond + timeout;
            while (hasBacklog())
            {
                if (timeout >= 0 &&  System.DateTime.UtcNow.Millisecond > timeOutAt)
                {
                    throw Disruptor3_Net.Exceptions.TimeoutException.INSTANCE;
                }
                System.Threading.Thread.Sleep(1);
                // Busy spin <= Why?
            }
            halt();
        }

        /**
         * The {@link RingBuffer} used by this Disruptor.  This is useful for creating custom
         * event processors if the behaviour of {@link BatchEventProcessor} is not suitable.
         *
         * @return the ring buffer used by this Disruptor.
         */
        public RingBuffer<T> getRingBuffer()
        {
            return ringBuffer;
        }

        /**
         * Get the value of the cursor indicating the published sequence.
         *
         * @return value of the cursor for events that have been published.
         */
        public long getCursor()
        {
            return ringBuffer.getCursor();
        }

        /**
         * The capacity of the data structure to hold entries.
         *
         * @return the size of the RingBuffer.
         * @see com.lmax.disruptor.Sequencer#getBufferSize()
         */
        public long getBufferSize()
        {
            return ringBuffer.getBufferSize();
        }

        /**
         * Get the event for a given sequence in the RingBuffer.
         *
         * @param sequence for the event.
         * @return event for the sequence.
         * @see RingBuffer#get(long)
         */
        public T get(long sequence)
        {
            return ringBuffer.get(sequence);
        }

        /**
         * Get the {@link SequenceBarrier} used by a specific handler. Note that the {@link SequenceBarrier}
         * may be shared by multiple event handlers.
         *
         * @param handler the handler to get the barrier for.
         * @return the SequenceBarrier used by <i>handler</i>.
         */
        public ISequenceBarrier getBarrierFor(IEventHandler<T> handler)
        {
            return consumerRepository.getBarrierFor(handler);
        }

        /**
         * Confirms if all messages have been consumed by all event processors
         */
        private Boolean hasBacklog()
        {
            long cursor = ringBuffer.getCursor();
            foreach (Sequence consumer in consumerRepository.getLastSequenceInChain(false))
            {
                if (cursor > consumer.get())
                {
                    return true;
                }
            }
            return false;
        }

        public EventHandlerGroup<T> createEventProcessors(Sequence[] barrierSequences,
                                                   IEventHandler<T>[] eventHandlers)
        {
            checkNotStarted();

            Sequence[] processorSequences = new Sequence[eventHandlers.Length];
            ISequenceBarrier barrier = ringBuffer.newBarrier(barrierSequences);

            for (int i = 0, eventHandlersLength = eventHandlers.Length; i < eventHandlersLength; i++)
            {
                IEventHandler<T> eventHandler = eventHandlers[i];

                BatchEventProcessor<T> batchEventProcessor = new BatchEventProcessor<T>(ringBuffer, barrier, eventHandler);

                if (exceptionHandler != null)
                {
                    batchEventProcessor.setExceptionHandler(exceptionHandler);
                }

                consumerRepository.add(batchEventProcessor, eventHandler, barrier);
                processorSequences[i] = batchEventProcessor.getSequence();
            }

            if (processorSequences.Length > 0)
            {
                consumerRepository.unMarkEventProcessorsAsEndOfChain(barrierSequences);
            }

            return new EventHandlerGroup<T>(this, consumerRepository, processorSequences);
        }

        public EventHandlerGroup<T> createEventProcessors(Sequence[] barrierSequences, EventProcessorFactory<T>[] processorFactories)
        {
            IEventProcessor[] eventProcessors = new IEventProcessor[processorFactories.Length];
            for (int i = 0; i < processorFactories.Length; i++)
            {
                eventProcessors[i] = processorFactories[i].createEventProcessor(ringBuffer, barrierSequences);
            }
            return handleEventsWith(eventProcessors);
        }

        public EventHandlerGroup<T> createWorkerPool(Sequence[] barrierSequences, IWorkHandler<T>[] workHandlers)
        {
            ISequenceBarrier sequenceBarrier = ringBuffer.newBarrier(barrierSequences);
            WorkerPool<T> workerPool = new WorkerPool<T>(ringBuffer, sequenceBarrier, exceptionHandler, workHandlers);
            consumerRepository.add(workerPool, sequenceBarrier);
            return new EventHandlerGroup<T>(this, consumerRepository, workerPool.getWorkerSequences());
        }

        private void checkNotStarted()
        {
            if (Volatile.Read(ref started)==1)
            {
                throw new ThreadStateException("All event handlers must be added before calling starts.");
            }
        }

        private void checkOnlyStartedOnce()
        {
           
            if (Interlocked.CompareExchange(ref started, 1, 0)!=0)
            {
                throw new ThreadStateException("Disruptor.start() must only be called once.");
            }
        }
    }
}
