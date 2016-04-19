using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.dsl
{
    /**
     * Defines producer types to support creation of RingBuffer with correct sequencer and publisher.
     */
    public enum ProducerType
    {
        /** Create a RingBuffer with a single event publisher to the RingBuffer */
        SINGLE,

        /** Create a RingBuffer supporting multiple event publishers to the one RingBuffer */
        MULTI
    }

}
