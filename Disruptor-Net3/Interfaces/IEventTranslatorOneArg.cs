using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.Interfaces
{
   public interface IEventTranslatorOneArg<T, A>
    {
        /**
         * Translate a data representation into fields set in given event
         *
         * @param event into which the data should be translated.
         * @param sequence that is assigned to event.
         * @param arg0 The first user specified argument to the translator
         */
        void translateTo(T eventToUse, long sequence,A arg0);
    }

}
