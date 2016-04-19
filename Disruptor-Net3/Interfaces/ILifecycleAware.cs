using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.Interfaces
{
    /**
      * Implement this interface in your {@link EventHandler} to be notified when a thread for the
      * {@link BatchEventProcessor} starts and shuts down.
      */
    public interface ILifecycleAware
    {
        /**
         * Called once on thread start before first event is available.
         */
        void onStart();

        /**
         * <p>Called once just before the thread is shutdown.</p>
         *
         * Sequence event processing will already have stopped before this method is called. No events will
         * be processed after this message.
         */
        void onShutdown();
    }

}
