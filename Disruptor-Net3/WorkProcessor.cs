using Disruptor_Net3.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Disruptor_Net3.Exceptions;
namespace Disruptor_Net3
{
    
    /**
     * <p>A {@link WorkProcessor} wraps a single {@link WorkHandler}, effectively consuming the sequence
     * and ensuring appropriate barriers.</p>
     *
     * <p>Generally, this will be used as part of a {@link WorkerPool}.</p>
     *
     * @param <T> event implementation storing the details for the work to processed.
     */
    public class WorkProcessor<T>:IEventProcessor
    {
        private Boolean running = false;
        private Sequence sequence = new Sequence();
        private RingBuffer<T> ringBuffer;
        private ISequenceBarrier sequenceBarrier;
        private IWorkHandler<T> workHandler;
        private IExceptionHandler<T> exceptionHandler;
        private Sequence workSequence;
        private EventReleaser _eventRelease;
        private class EventReleaser:IEventReleaser
        {
            Sequence _sequenceToRelease;
            public EventReleaser(Sequence sequenceToRelease)
            {
                _sequenceToRelease = sequenceToRelease;
            }
            public void release()
            {
 	            _sequenceToRelease.set(Int64.MaxValue);
            }
        };


        /**
         * Construct a {@link WorkProcessor}.
         *
         * @param ringBuffer to which events are published.
         * @param sequenceBarrier on which it is waiting.
         * @param workHandler is the delegate to which events are dispatched.
         * @param exceptionHandler to be called back when an error occurs
         * @param workSequence from which to claim the next event to be worked on.  It should always be initialised
         * as {@link Sequencer#INITIAL_CURSOR_VALUE}
         */
        public WorkProcessor(RingBuffer<T> ringBuffer,
                             ISequenceBarrier sequenceBarrier,
                             IWorkHandler<T> workHandler,
                             IExceptionHandler<T> exceptionHandler,
                             Sequence workSequence)
        {
            _eventRelease = new EventReleaser(sequence);
            this.ringBuffer = ringBuffer;
            this.sequenceBarrier = sequenceBarrier;
            this.workHandler = workHandler;
            this.exceptionHandler = exceptionHandler;
            this.workSequence = workSequence;

            if (this.workHandler is IEventReleaseAware)
            {
                ((IEventReleaseAware)this.workHandler).setEventReleaser(_eventRelease);
            }
        }

        
        public Sequence getSequence()
        {
            return sequence;
        }

        
        public void halt()
        {
            Volatile.Write(ref running,false);
            sequenceBarrier.alert();
        }

       
        public Boolean isRunning()
        {
            return Volatile.Read(ref running);
        }

        /**
         * It is ok to have another thread re-run this method after a halt().
         *
         * @throws IllegalStateException if this processor is already running
         */
        
        public void run()
        {
            
            if (!Volatile.Read(ref running))
            {
                throw new System.InvalidOperationException("Thread is already running");
            }
            sequenceBarrier.clearAlert();

            notifyStart();

            Boolean processedSequence = true;
            long cachedAvailableSequence = Int64.MinValue;
            long nextSequence = sequence.get();
            T eventToUse = default(T);
            while (true)
            {
                try
                {
                    // if previous sequence was processed - fetch the next sequence and set
                    // that we have successfully processed the previous sequence
                    // typically, this will be true
                    // this prevents the sequence getting too far forward if an exception
                    // is thrown from the WorkHandler
                    if (processedSequence)
                    {
                        processedSequence = false;
                        do
                        {
                            nextSequence = workSequence.get() + 1L;
                            sequence.set(nextSequence - 1L);
                        }
                        while (!workSequence.compareAndSet(nextSequence - 1L, nextSequence));
                    }

                    if (cachedAvailableSequence >= nextSequence)
                    {
                        eventToUse = ringBuffer.get(nextSequence);
                        workHandler.onEvent(eventToUse);
                        processedSequence = true;
                    }
                    else
                    {
                        cachedAvailableSequence = sequenceBarrier.waitFor(nextSequence);
                    }
                }
                catch (AlertException)
                {
                    if (!Volatile.Read(ref running))
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    // handle, mark as processed, unless the exception handler threw an exception
                    exceptionHandler.handleEventException(ex, nextSequence, eventToUse);
                    processedSequence = true;
                }
            }

            notifyShutdown();
            Volatile.Write(ref running, false);
        }

        private void notifyStart()
        {
            if (workHandler is ILifecycleAware)
            {
                try
                {
                    ((ILifecycleAware)workHandler).onStart();
                }
                catch (Exception ex)
                {
                    exceptionHandler.handleOnStartException(ex);
                }
            }
        }

        private void notifyShutdown()
        {
            if (workHandler is ILifecycleAware)
            {
                try
                {
                    ((ILifecycleAware)workHandler).onShutdown();
                }
                catch (Exception ex)
                {
                    exceptionHandler.handleOnShutdownException(ex);
                }
            }
        }
    }
}
