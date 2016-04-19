using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.Interfaces
{
   /**
     * Callback handler for uncaught exceptions in the event processing cycle of the {@link BatchEventProcessor}
     */
    public interface IExceptionHandler<T>
    {
        /**
         * <p>Strategy for handling uncaught exceptions when processing an event.</p>
         *
         * <p>If the strategy wishes to terminate further processing by the {@link BatchEventProcessor}
         * then it should throw a {@link RuntimeException}.</p>
         *
         * @param ex the exception that propagated from the {@link EventHandler}.
         * @param sequence of the event which cause the exception.
         * @param event being processed when the exception occurred.  This can be null.
         */
        void handleEventException(Exception ex, long sequence, T eventToUse);

        /**
         * Callback to notify of an exception during {@link LifecycleAware#onStart()}
         *
         * @param ex throw during the starting process.
         */
        void handleOnStartException(Exception ex);

        /**
         * Callback to notify of an exception during {@link LifecycleAware#onShutdown()}
         *
         * @param ex throw during the shutdown process.
         */
        void handleOnShutdownException(Exception ex);
    }
}
