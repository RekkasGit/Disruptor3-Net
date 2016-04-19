using Disruptor3_Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.dsl
{
   
    /**
     * A group of {@link EventProcessor}s used as part of the {@link Disruptor}.
     *
     * @param <T> the type of entry used by the event processors.
     */
    public class EventHandlerGroup<T>
    {
        private Disruptor<T> disruptor;
        private ConsumerRepository<T> consumerRepository;
        private Sequence[] sequences;

        public EventHandlerGroup(Disruptor<T> disruptor,
                          ConsumerRepository<T> consumerRepository,
                          Sequence[] sequences)
        {
            this.disruptor = disruptor;
            this.consumerRepository = consumerRepository;
            this.sequences = sequences.ToArray();
        }

        /**
         * Create a new event handler group that combines the consumers in this group with <tt>otherHandlerGroup</tt>.
         *
         * @param otherHandlerGroup the event handler group to combine.
         * @return a new EventHandlerGroup combining the existing and new consumers into a single dependency group.
         */
        public EventHandlerGroup<T> and(EventHandlerGroup<T> otherHandlerGroup)
        {
            Sequence[] combinedSequences = new Sequence[this.sequences.Length + otherHandlerGroup.sequences.Length];
            Array.Copy(this.sequences, 0, combinedSequences, 0, this.sequences.Length);
            Array.Copy(otherHandlerGroup.sequences, 0, combinedSequences, this.sequences.Length, otherHandlerGroup.sequences.Length);

            return new EventHandlerGroup<T>(disruptor, consumerRepository, combinedSequences);
        }

        /**
         * Create a new event handler group that combines the handlers in this group with <tt>processors</tt>.
         *
         * @param processors the processors to combine.
         * @return a new EventHandlerGroup combining the existing and new processors into a single dependency group.
         */
        public EventHandlerGroup<T> and(params IEventProcessor[] processors)
        {
            Sequence[] combinedSequences = new Sequence[sequences.Length + processors.Length];

            for (int i = 0; i < processors.Length; i++)
            {
                consumerRepository.add(processors[i]);
                combinedSequences[i] = processors[i].getSequence();
            }
            Array.Copy(sequences, 0, combinedSequences, processors.Length, sequences.Length);

            return new EventHandlerGroup<T>(disruptor, consumerRepository, combinedSequences);
        }

        /**
         * Set up batch handlers to consume events from the ring buffer. These handlers will only process events
         * after every {@link EventProcessor} in this group has processed the event.
         *
         * <p>This method is generally used as part of a chain. For example if the handler <code>A</code> must
         * process events before handler <code>B</code>:</p>
         *
         * <pre><code>dw.handleEventsWith(A).then(B);</code></pre>
         *
         * @param handlers the batch handlers that will process events.
         * @return a {@link EventHandlerGroup} that can be used to set up a event processor barrier over the created event processors.
         */
        public EventHandlerGroup<T> then(params IEventHandler<T>[] handlers)
        {
            return handleEventsWith(handlers);
        }

        /**
         * <p>Set up custom event processors to handle events from the ring buffer. The Disruptor will
         * automatically start these processors when {@link Disruptor#start()} is called.</p>
         *
         * <p>This method is generally used as part of a chain. For example if the handler <code>A</code> must
         * process events before handler <code>B</code>:</p>
         *
         * @param eventProcessorFactories the event processor factories to use to create the event processors that will process events.
         * @return a {@link EventHandlerGroup} that can be used to chain dependencies.
         */
        public EventHandlerGroup<T> then(params EventProcessorFactory<T>[] eventProcessorFactories)
        {
            return handleEventsWith(eventProcessorFactories);
        }

        /**
         * Set up a worker pool to handle events from the ring buffer. The worker pool will only process events
         * after every {@link EventProcessor} in this group has processed the event. Each event will be processed
         * by one of the work handler instances.
         *
         * <p>This method is generally used as part of a chain. For example if the handler <code>A</code> must
         * process events before the worker pool with handlers <code>B, C</code>:</p>
         *
         * <pre><code>dw.handleEventsWith(A).thenHandleEventsWithWorkerPool(B, C);</code></pre>
         *
         * @param handlers the work handlers that will process events. Each work handler instance will provide an extra thread in the worker pool.
         * @return a {@link EventHandlerGroup} that can be used to set up a event processor barrier over the created event processors.
         */
        public EventHandlerGroup<T> thenHandleEventsWithWorkerPool(params IWorkHandler<T>[] handlers)
        {
            return handleEventsWithWorkerPool(handlers);
        }

        /**
         * Set up batch handlers to handle events from the ring buffer. These handlers will only process events
         * after every {@link EventProcessor} in this group has processed the event.
         *
         * <p>This method is generally used as part of a chain. For example if <code>A</code> must
         * process events before <code>B</code>:</p>
         *
         * <pre><code>dw.after(A).handleEventsWith(B);</code></pre>
         *
         * @param handlers the batch handlers that will process events.
         * @return a {@link EventHandlerGroup} that can be used to set up a event processor barrier over the created event processors.
         */
        public EventHandlerGroup<T> handleEventsWith(IEventHandler<T>[] handlers)
        {
            return disruptor.createEventProcessors(sequences, handlers);
        }

        /**
         * <p>Set up custom event processors to handle events from the ring buffer. The Disruptor will
         * automatically start these processors when {@link Disruptor#start()} is called.</p>
         *
         * <p>This method is generally used as part of a chain. For example if <code>A</code> must
         * process events before <code>B</code>:</p>
         *
         * <pre><code>dw.after(A).handleEventsWith(B);</code></pre>
         *
         * @param eventProcessorFactories the event processor factories to use to create the event processors that will process events.
         * @return a {@link EventHandlerGroup} that can be used to chain dependencies.
         */
        public EventHandlerGroup<T> handleEventsWith(params EventProcessorFactory<T>[] eventProcessorFactories)
        {
            return disruptor.createEventProcessors(sequences, eventProcessorFactories);
        }

        /**
         * Set up a worker pool to handle events from the ring buffer. The worker pool will only process events
         * after every {@link EventProcessor} in this group has processed the event. Each event will be processed
         * by one of the work handler instances.
         *
         * <p>This method is generally used as part of a chain. For example if the handler <code>A</code> must
         * process events before the worker pool with handlers <code>B, C</code>:</p>
         *
         * <pre><code>dw.after(A).handleEventsWithWorkerPool(B, C);</code></pre>
         *
         * @param handlers the work handlers that will process events. Each work handler instance will provide an extra thread in the worker pool.
         * @return a {@link EventHandlerGroup} that can be used to set up a event processor barrier over the created event processors.
         */
        public EventHandlerGroup<T> handleEventsWithWorkerPool(params IWorkHandler<T>[] handlers)
        {
            return disruptor.createWorkerPool(sequences, handlers);
        }

        /**
         * Create a dependency barrier for the processors in this group.
         * This allows custom event processors to have dependencies on
         * {@link com.lmax.disruptor.BatchEventProcessor}s created by the disruptor.
         *
         * @return a {@link SequenceBarrier} including all the processors in this group.
         */
        public ISequenceBarrier asSequenceBarrier()
        {
            return disruptor.getRingBuffer().newBarrier(sequences);
        }
    }

}
