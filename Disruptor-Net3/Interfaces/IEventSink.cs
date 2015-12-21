using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor_Net3.Interfaces
{
    public interface IEventSink<E>
    {
        /**
         * Publishes an event to the ring buffer.  It handles
         * claiming the next sequence, getting the current (uninitialised)
         * event from the ring buffer and publishing the claimed sequence
         * after translation.
         *
         * @param translator The user specified translation for the event
         */
        void publishEvent(IEventTranslator<E> translator);

        /**
         * Attempts to publish an event to the ring buffer.  It handles
         * claiming the next sequence, getting the current (uninitialised)
         * event from the ring buffer and publishing the claimed sequence
         * after translation.  Will return false if specified capacity
         * was not available.
         *
         * @param translator The user specified translation for the event
         * @return true if the value was published, false if there was insufficient
         * capacity.
         */
        Boolean tryPublishEvent(IEventTranslator<E> translator);

        /**
         * Allows one user supplied argument.
         *
         * @see #publishEvent(EventTranslator)
         * @param translator The user specified translation for the event
         * @param arg0 A user supplied argument.
         */
         void publishEvent<A>(IEventTranslatorOneArg<E, A> translator, A arg0);

        /**
         * Allows one user supplied argument.
         *
         * @see #tryPublishEvent(EventTranslator)
         * @param translator The user specified translation for the event
         * @param arg0 A user supplied argument.
         * @return true if the value was published, false if there was insufficient
         * capacity.
         */
        Boolean tryPublishEvent<A>(IEventTranslatorOneArg<E, A> translator, A arg0);

        /**
         * Allows two user supplied arguments.
         *
         * @see #publishEvent(EventTranslator)
         * @param translator The user specified translation for the event
         * @param arg0 A user supplied argument.
         * @param arg1 A user supplied argument.
         */
         void publishEvent<A, B>(IEventTranslatorTwoArg<E, A, B> translator, A arg0, B arg1);

        /**
         * Allows two user supplied arguments.
         *
         * @see #tryPublishEvent(EventTranslator)
         * @param translator The user specified translation for the event
         * @param arg0 A user supplied argument.
         * @param arg1 A user supplied argument.
         * @return true if the value was published, false if there was insufficient
         * capacity.
         */
         Boolean tryPublishEvent <A, B> (IEventTranslatorTwoArg<E, A, B> translator, A arg0, B arg1);

        /**
         * Allows three user supplied arguments
         *
         * @see #publishEvent(EventTranslator)
         * @param translator The user specified translation for the event
         * @param arg0 A user supplied argument.
         * @param arg1 A user supplied argument.
         * @param arg2 A user supplied argument.
         */
        void publishEvent<A, B, C> (IEventTranslatorThreeArg<E, A, B, C> translator, A arg0, B arg1, C arg2);

        /**
         * Allows three user supplied arguments
         *
         * @see #publishEvent(EventTranslator)
         * @param translator The user specified translation for the event
         * @param arg0 A user supplied argument.
         * @param arg1 A user supplied argument.
         * @param arg2 A user supplied argument.
         * @return true if the value was published, false if there was insufficient
         * capacity.
         */
         Boolean tryPublishEvent<A, B, C>(IEventTranslatorThreeArg<E, A, B, C> translator, A arg0, B arg1, C arg2);

        /**
         * Allows a variable number of user supplied arguments
         *
         * @see #publishEvent(EventTranslator)
         * @param translator The user specified translation for the event
         * @param args User supplied arguments.
         */
        void publishEvent(IEventTranslatorVararg<E> translator, params Object[] args);

        /**
         * Allows a variable number of user supplied arguments
         *
         * @see #publishEvent(EventTranslator)
         * @param translator The user specified translation for the event
         * @param args User supplied arguments.
         * @return true if the value was published, false if there was insufficient
         * capacity.
         */
        Boolean tryPublishEvent(IEventTranslatorVararg<E> translator, params Object[] args);

        /**
         * Publishes multiple events to the ring buffer.  It handles
         * claiming the next sequence, getting the current (uninitialised)
         * event from the ring buffer and publishing the claimed sequence
         * after translation.
         * <p>
         * With this call the data that is to be inserted into the ring
         * buffer will be a field (either explicitly or captured anonymously),
         * therefore this call will require an instance of the translator
         * for each value that is to be inserted into the ring buffer.
         *
         * @param translators The user specified translation for each event
         */
        void publishEvents(IEventTranslator<E>[] translators);

        /**
         * Publishes multiple events to the ring buffer.  It handles
         * claiming the next sequence, getting the current (uninitialised)
         * event from the ring buffer and publishing the claimed sequence
         * after translation.
         * <p>
         * With this call the data that is to be inserted into the ring
         * buffer will be a field (either explicitly or captured anonymously),
         * therefore this call will require an instance of the translator
         * for each value that is to be inserted into the ring buffer.
         *
         * @param translators   The user specified translation for each event
         * @param batchStartsAt The first element of the array which is within the batch.
         * @param batchSize     The actual size of the batch
         */
        void publishEvents(IEventTranslator<E>[] translators, int batchStartsAt, int batchSize);

        /**
         * Attempts to publish multiple events to the ring buffer.  It handles
         * claiming the next sequence, getting the current (uninitialised)
         * event from the ring buffer and publishing the claimed sequence
         * after translation.  Will return false if specified capacity
         * was not available.
         *
         * @param translators The user specified translation for the event
         * @return true if the value was published, false if there was insufficient
         *         capacity.
         */
        Boolean tryPublishEvents(IEventTranslator<E>[] translators);

        /**
         * Attempts to publish multiple events to the ring buffer.  It handles
         * claiming the next sequence, getting the current (uninitialised)
         * event from the ring buffer and publishing the claimed sequence
         * after translation.  Will return false if specified capacity
         * was not available.
         *
         * @param translators   The user specified translation for the event
         * @param batchStartsAt The first element of the array which is within the batch.
         * @param batchSize     The actual size of the batch
         * @return true if all the values were published, false if there was insufficient
         *         capacity.
         */
        Boolean tryPublishEvents(IEventTranslator<E>[] translators, int batchStartsAt, int batchSize);

        /**
         * Allows one user supplied argument per event.
         *
         * @param translator The user specified translation for the event
         * @param arg0       A user supplied argument.
         * @see #publishEvents(com.lmax.disruptor.EventTranslator[])
         */
        void publishEvents<A> (IEventTranslatorOneArg<E, A> translator, A[] arg0);

        /**
         * Allows one user supplied argument per event.
         *
         * @param translator    The user specified translation for each event
         * @param batchStartsAt The first element of the array which is within the batch.
         * @param batchSize     The actual size of the batch
         * @param arg0          An array of user supplied arguments, one element per event.
         * @see #publishEvents(EventTranslator[])
         */
        void publishEvents<A>(IEventTranslatorOneArg<E, A> translator, int batchStartsAt, int batchSize, A[] arg0);

        /**
         * Allows one user supplied argument.
         *
         * @param translator The user specified translation for each event
         * @param arg0       An array of user supplied arguments, one element per event.
         * @return true if the value was published, false if there was insufficient
         *         capacity.
         * @see #tryPublishEvents(com.lmax.disruptor.EventTranslator[])
         */
         Boolean tryPublishEvents<A> (IEventTranslatorOneArg<E, A> translator, A[] arg0);

        /**
         * Allows one user supplied argument.
         *
         * @param translator    The user specified translation for each event
         * @param batchStartsAt The first element of the array which is within the batch.
         * @param batchSize     The actual size of the batch
         * @param arg0          An array of user supplied arguments, one element per event.
         * @return true if the value was published, false if there was insufficient
         *         capacity.
         * @see #tryPublishEvents(EventTranslator[])
         */
        Boolean tryPublishEvents<A>(IEventTranslatorOneArg<E, A> translator, int batchStartsAt, int batchSize, A[] arg0);

        /**
         * Allows two user supplied arguments per event.
         *
         * @param translator The user specified translation for the event
         * @param arg0       An array of user supplied arguments, one element per event.
         * @param arg1       An array of user supplied arguments, one element per event.
         * @see #publishEvents(com.lmax.disruptor.EventTranslator[])
         */
        void publishEvents<A, B> (IEventTranslatorTwoArg<E, A, B> translator, A[] arg0, B[] arg1);

        /**
         * Allows two user supplied arguments per event.
         *
         * @param translator    The user specified translation for the event
         * @param batchStartsAt The first element of the array which is within the batch.
         * @param batchSize     The actual size of the batch.
         * @param arg0          An array of user supplied arguments, one element per event.
         * @param arg1          An array of user supplied arguments, one element per event.
         * @see #publishEvents(EventTranslator[])
         */
        void publishEvents<A, B>(IEventTranslatorTwoArg<E, A, B> translator, int batchStartsAt, int batchSize, A[] arg0,
                                  B[] arg1);

        /**
         * Allows two user supplied arguments per event.
         *
         * @param translator The user specified translation for the event
         * @param arg0       An array of user supplied arguments, one element per event.
         * @param arg1       An array of user supplied arguments, one element per event.
         * @return true if the value was published, false if there was insufficient
         *         capacity.
         * @see #tryPublishEvents(com.lmax.disruptor.EventTranslator[])
         */
        Boolean tryPublishEvents<A, B>(IEventTranslatorTwoArg<E, A, B> translator, A[] arg0, B[] arg1);

        /**
         * Allows two user supplied arguments per event.
         *
         * @param translator    The user specified translation for the event
         * @param batchStartsAt The first element of the array which is within the batch.
         * @param batchSize     The actual size of the batch.
         * @param arg0          An array of user supplied arguments, one element per event.
         * @param arg1          An array of user supplied arguments, one element per event.
         * @return true if the value was published, false if there was insufficient
         *         capacity.
         * @see #tryPublishEvents(EventTranslator[])
         */
        Boolean tryPublishEvents<A, B>(IEventTranslatorTwoArg<E, A, B> translator, int batchStartsAt, int batchSize,
                                        A[] arg0, B[] arg1);

        /**
         * Allows three user supplied arguments per event.
         *
         * @param translator The user specified translation for the event
         * @param arg0       An array of user supplied arguments, one element per event.
         * @param arg1       An array of user supplied arguments, one element per event.
         * @param arg2       An array of user supplied arguments, one element per event.
         * @see #publishEvents(com.lmax.disruptor.EventTranslator[])
         */
        void publishEvents<A, B, C>(IEventTranslatorThreeArg<E, A, B, C> translator, A[] arg0, B[] arg1, C[] arg2);

        /**
         * Allows three user supplied arguments per event.
         *
         * @param translator    The user specified translation for the event
         * @param batchStartsAt The first element of the array which is within the batch.
         * @param batchSize     The number of elements in the batch.
         * @param arg0          An array of user supplied arguments, one element per event.
         * @param arg1          An array of user supplied arguments, one element per event.
         * @param arg2          An array of user supplied arguments, one element per event.
         * @see #publishEvents(EventTranslator[])
         */
        void publishEvents<A, B, C>(IEventTranslatorThreeArg<E, A, B, C> translator, int batchStartsAt, int batchSize,
                                     A[] arg0, B[] arg1, C[] arg2);

        /**
         * Allows three user supplied arguments per event.
         *
         * @param translator The user specified translation for the event
         * @param arg0       An array of user supplied arguments, one element per event.
         * @param arg1       An array of user supplied arguments, one element per event.
         * @param arg2       An array of user supplied arguments, one element per event.
         * @return true if the value was published, false if there was insufficient
         *         capacity.
         * @see #publishEvents(com.lmax.disruptor.EventTranslator[])
         */
        Boolean tryPublishEvents<A, B, C> (IEventTranslatorThreeArg<E, A, B, C> translator, A[] arg0, B[] arg1, C[] arg2);

        /**
         * Allows three user supplied arguments per event.
         *
         * @param translator    The user specified translation for the event
         * @param batchStartsAt The first element of the array which is within the batch.
         * @param batchSize     The actual size of the batch.
         * @param arg0          An array of user supplied arguments, one element per event.
         * @param arg1          An array of user supplied arguments, one element per event.
         * @param arg2          An array of user supplied arguments, one element per event.
         * @return true if the value was published, false if there was insufficient
         *         capacity.
         * @see #publishEvents(EventTranslator[])
         */
        Boolean tryPublishEvents<A, B, C>(IEventTranslatorThreeArg<E, A, B, C> translator, int batchStartsAt,
                                           int batchSize, A[] arg0, B[] arg1, C[] arg2);

        /**
         * Allows a variable number of user supplied arguments per event.
         *
         * @param translator The user specified translation for the event
         * @param args       User supplied arguments, one Object[] per event.
         * @see #publishEvents(com.lmax.disruptor.EventTranslator[])
         */
        void publishEvents(IEventTranslatorVararg<E> translator, params Object[][] args);

        /**
         * Allows a variable number of user supplied arguments per event.
         *
         * @param translator    The user specified translation for the event
         * @param batchStartsAt The first element of the array which is within the batch.
         * @param batchSize     The actual size of the batch
         * @param args          User supplied arguments, one Object[] per event.
         * @see #publishEvents(EventTranslator[])
         */
        void publishEvents(IEventTranslatorVararg<E> translator, int batchStartsAt, int batchSize, params Object[][] args);

        /**
         * Allows a variable number of user supplied arguments per event.
         *
         * @param translator The user specified translation for the event
         * @param args       User supplied arguments, one Object[] per event.
         * @return true if the value was published, false if there was insufficient
         *         capacity.
         * @see #publishEvents(com.lmax.disruptor.EventTranslator[])
         */
        Boolean tryPublishEvents(IEventTranslatorVararg<E> translator, params Object[][] args);

        /**
         * Allows a variable number of user supplied arguments per event.
         *
         * @param translator    The user specified translation for the event
         * @param batchStartsAt The first element of the array which is within the batch.
         * @param batchSize     The actual size of the batch.
         * @param args          User supplied arguments, one Object[] per event.
         * @return true if the value was published, false if there was insufficient
         *         capacity.
         * @see #publishEvents(EventTranslator[])
         */
        Boolean tryPublishEvents(IEventTranslatorVararg<E> translator, int batchStartsAt, int batchSize, Object[][] args);

    }
}
