using Disruptor3_Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Disruptor3_Net.Exceptions;

namespace Disruptor3_Net
{
   /**
     * Convenience class for handling the batching semantics of consuming entries from a {@link RingBuffer}
     * and delegating the available events to an {@link EventHandler}.
     *
     * If the {@link EventHandler} also implements {@link LifecycleAware} it will be notified just after the thread
     * is started and just before the thread is shutdown.
     *
     * @param <T> event implementation storing the data for sharing during exchange or parallel coordination of an event.
     */
    public class BatchEventProcessor<T>:IEventProcessor
    {
        private Boolean running = false;
        private IExceptionHandler<T> exceptionHandler = new FatalExceptionHandler<T>();
        private IDataProvider<T> dataProvider;
        private ISequenceBarrier sequenceBarrier;
        private IEventHandler<T> eventHandler;
        private Sequence sequence = new Sequence();
        private ITimeoutHandler timeoutHandler;

        /**
         * Construct a {@link EventProcessor} that will automatically track the progress by updating its sequence when
         * the {@link EventHandler#onEvent(Object, long, boolean)} method returns.
         *
         * @param dataProvider to which events are published.
         * @param sequenceBarrier on which it is waiting.
         * @param eventHandler is the delegate to which events are dispatched.
         */
        public BatchEventProcessor(IDataProvider<T> dataProvider,
                                   ISequenceBarrier sequenceBarrier,
                                   IEventHandler<T> eventHandler)
        {
            this.dataProvider = dataProvider;
            this.sequenceBarrier = sequenceBarrier;
            this.eventHandler = eventHandler;

            if (eventHandler is ISequenceReportingEventHandler<T>)
            {
                ((ISequenceReportingEventHandler<T>)eventHandler).setSequenceCallback(sequence);
            }

            timeoutHandler = (eventHandler is ITimeoutHandler) ? (ITimeoutHandler) eventHandler : null;
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
         * Set a new {@link ExceptionHandler} for handling exceptions propagated out of the {@link BatchEventProcessor}
         *
         * @param exceptionHandler to replace the existing exceptionHandler.
         */
        public void setExceptionHandler(IExceptionHandler<T> exceptionHandler)
        {
            if (exceptionHandler==null)
            {
                throw new NullReferenceException();
            }

            this.exceptionHandler = exceptionHandler;
        }

        /**
         * It is ok to have another thread rerun this method after a halt().
         *
         * @throws IllegalStateException if this object instance is already running in a thread
         */
        
        public void run()
        {
        

            if (Volatile.Read(ref running))
            {
                throw new ApplicationException ("Thread is already running");
            }
            sequenceBarrier.clearAlert();

            notifyStart();

            T eventToUse = default(T);
            long nextSequence = sequence.get() + 1L;
            try
            {
                while (true)
                {
                    try
                    {
                        long availableSequence = sequenceBarrier.waitFor(nextSequence);

                        while (nextSequence <= availableSequence)
                        {
                            eventToUse = dataProvider.get(nextSequence);
                            eventHandler.onEvent(eventToUse, nextSequence, nextSequence == availableSequence);
                            nextSequence++;
                        }

                        sequence.set(availableSequence);
                    }
                    catch (Disruptor3_Net.Exceptions.TimeoutException)
                    {
                        notifyTimeout(sequence.get());
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
                        exceptionHandler.handleEventException(ex, nextSequence, eventToUse);
                        sequence.set(nextSequence);
                        nextSequence++;
                    }
                }
            }
            finally
            {
                notifyShutdown();
                Volatile.Write(ref running,false);
            }
        }

        private void notifyTimeout(long availableSequence)
        {
            try
            {
                if (timeoutHandler != null)
                {
                    timeoutHandler.onTimeout(availableSequence);
                }
            }
            catch (Exception e)
            {
                exceptionHandler.handleEventException(e, availableSequence, default(T));
            }
        }

        /**
         * Notifies the EventHandler when this processor is starting up
         */
        private void notifyStart()
        {
            if (eventHandler is ILifecycleAware)
            {
                try
                {
                    ((ILifecycleAware)eventHandler).onStart();
                }
                catch (Exception ex)
                {
                    exceptionHandler.handleOnStartException(ex);
                }
            }
        }

        /**
         * Notifies the EventHandler immediately prior to this processor shutting down
         */
        private void notifyShutdown()
        {
            if (eventHandler is ILifecycleAware)
            {
                try
                {
                    ((ILifecycleAware)eventHandler).onShutdown();
                }
                catch (Exception ex)
                {
                    exceptionHandler.handleOnShutdownException(ex);
                }
            }
        }
    }
}
