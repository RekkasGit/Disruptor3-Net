using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor_Net3.Interfaces
{
    /**
     * Implementations translate another data representations into events claimed from the {@link RingBuffer}
     *
     * @param <T> event implementation storing the data for sharing during exchange or parallel coordination of an event.
     * @see EventTranslator
     */
    public interface IEventTranslatorVararg<T>
    {
        /**
         * Translate a data representation into fields set in given event
         *
         * @param event into which the data should be translated.
         * @param sequence that is assigned to event.
         * @param args The array of user arguments.
         */
        void translateTo(T eventToUse, long sequence, params Object[] args);
    }
}
