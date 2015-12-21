using Disruptor_Net3.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor_Net3.dsl
{
    /**
     * A support class used as part of setting an exception handler for a specific event handler.
     * For example:
     * <pre><code>disruptorWizard.handleExceptionsIn(eventHandler).with(exceptionHandler);</code></pre>
     *
     * @param <T> the type of event being handled.
     */
    public class ExceptionHandlerSetting<T>
    {
        private IEventHandler<T> eventHandler;
        private ConsumerRepository<T> consumerRepository;

        public ExceptionHandlerSetting(IEventHandler<T> eventHandler,
                                ConsumerRepository<T> consumerRepository)
        {
            this.eventHandler = eventHandler;
            this.consumerRepository = consumerRepository;
        }

        /**
         * Specify the {@link ExceptionHandler} to use with the event handler.
         *
         * @param exceptionHandler the exception handler to use.
         */
        public void with(IExceptionHandler<T> exceptionHandler)
        {
            ((BatchEventProcessor<T>) consumerRepository.getEventProcessorFor(eventHandler)).setExceptionHandler(exceptionHandler);
            consumerRepository.getBarrierFor(eventHandler).alert();
        }
    }

}
