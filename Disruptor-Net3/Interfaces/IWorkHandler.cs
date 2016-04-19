using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.Interfaces
{
    /**
     * Callback interface to be implemented for processing units of work as they become available in the {@link RingBuffer}.
     *
     * @param <T> event implementation storing the data for sharing during exchange or parallel coordination of an event.
     * @see WorkerPool
     */
    public interface IWorkHandler<T>
    {
        /**
         * Callback to indicate a unit of work needs to be processed.
         *
         * @param event published to the {@link RingBuffer}
         * @throws Exception if the {@link WorkHandler} would like the exception handled further up the chain.
         */
        void onEvent(T eventToUse) ;
    }
}
