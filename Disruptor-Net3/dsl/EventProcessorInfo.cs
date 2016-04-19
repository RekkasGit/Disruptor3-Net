using Disruptor3_Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.dsl
{
   /**
     * <p>Wrapper class to tie together a particular event processing stage</p>
     *
     * <p>Tracks the event processor instance, the event handler instance, and sequence barrier which the stage is attached to.</p>
     *
     * @param T the type of the configured {@link EventHandler}
     */
    class EventProcessorInfo<T>:IConsumerInfo
    {
        private IEventProcessor eventprocessor;
        private IEventHandler<T> handler;
        private ISequenceBarrier barrier;
        private Boolean endOfChain = true;

        public EventProcessorInfo(IEventProcessor eventprocessor, IEventHandler<T> handler, ISequenceBarrier barrier)
        {
            this.eventprocessor = eventprocessor;
            this.handler = handler;
            this.barrier = barrier;
        }

        public IEventProcessor getEventProcessor()
        {
            return eventprocessor;
        }

       
        public Sequence[] getSequences()
        {
            return new Sequence[] { eventprocessor.getSequence() };
        }

        public IEventHandler<T> getHandler()
        {
            return handler;
        }

       
        public ISequenceBarrier getBarrier()
        {
            return barrier;
        }

        
        public Boolean isEndOfChain()
        {
            return endOfChain;
        }

       
        public void start()
        {
            Task.Factory.StartNew(() => eventprocessor.run(), TaskCreationOptions.LongRunning);
        }

      
        public void halt()
        {
            eventprocessor.halt();
        }

        public void markAsUsedInBarrier()
        {
            endOfChain = false;
        }

        
        public Boolean isRunning()
        {
            return eventprocessor.isRunning();
        }
    }
}
