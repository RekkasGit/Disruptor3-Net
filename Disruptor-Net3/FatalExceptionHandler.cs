using Disruptor_Net3.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor_Net3
{
    /* Convenience implementation of an exception handler that using standard JDK logging to log
     * the exception as {@link Level}.SEVERE and re-throw it wrapped in a {@link RuntimeException}
     */
    public class FatalExceptionHandler<T> : IExceptionHandler<T>
    {
      
        /// <summary>
        /// Strategy for handling uncaught exceptions when processing an event.
        /// </summary>
        /// <param name="ex">exception that propagated from the <see cref="IEventHandler{T}"/>.</param>
        /// <param name="sequence">sequence of the event which cause the exception.</param>
        /// <param name="evt">event being processed when the exception occurred.</param>
        public void handleEventException(Exception ex, long sequence, T evt)
        {
            var message = string.Format("Exception processing sequence {0} for event {1}: {2}", sequence, evt, ex);

            Console.WriteLine(message);

            throw new ApplicationException(message, ex);
        }

        /// <summary>
        /// Callback to notify of an exception during <see cref="ILifecycleAware.OnStart"/>
        /// </summary>
        /// <param name="ex">ex throw during the starting process.</param>
        public void handleOnStartException(Exception ex)
        {
            var message = string.Format("Exception during OnStart(): {0}", ex);

            Console.WriteLine(message);

            throw new ApplicationException(message, ex);
        }

        /// <summary>
        /// Callback to notify of an exception during <see cref="ILifecycleAware.OnShutdown"/>
        /// </summary>
        /// <param name="ex">ex throw during the shutdown process.</param>
        public void handleOnShutdownException(Exception ex)
        {
            var message = string.Format("Exception during OnShutdown(): {0}", ex);

            Console.WriteLine(message);

            throw new ApplicationException(message, ex);
        }


      
    }
}
