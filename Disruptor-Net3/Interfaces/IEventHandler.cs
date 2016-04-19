using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.Interfaces
{
    /**
     * Callback interface to be implemented for processing events as they become available in the {@link RingBuffer}
     *
     * @see BatchEventProcessor#setExceptionHandler(ExceptionHandler) if you want to handle exceptions propagated out of the handler.
     *
     * @param <T> event implementation storing the data for sharing during exchange or parallel coordination of an event.
     */
    public interface IEventHandler<T>
    {
        /**
         * Called when a publisher has published an event to the {@link RingBuffer}
         *
         * @param event published to the {@link RingBuffer}
         * @param sequence of the event being processed
         * @param endOfBatch flag to indicate if this is the last event in a batch from the {@link RingBuffer}
         * @throws Exception if the EventHandler would like the exception handled further up the chain.
         */
        void onEvent(T eventToUse, long sequence, Boolean endOfBatch);
    }
}
