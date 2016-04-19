using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.Interfaces
{
    /**
     * Called by the {@link RingBuffer} to pre-populate all the events to fill the RingBuffer.
     *
     * @param <T> event implementation storing the data for sharing during exchange or parallel coordination of an event.
     */
    public interface IEventFactory<T>
    {
        /*
            * Implementations should instantiate an event object, with all memory already allocated where possible.
            */
        T newInstance();
    }
}
