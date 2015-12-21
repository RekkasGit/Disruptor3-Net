using Disruptor_Net3.dsl;
using Disruptor_Net3.Exceptions;
using Disruptor_Net3.Interfaces;
using Disruptor_Net3.Sequencers;
using Disruptor_Net3.WaitStrategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor_Net3
{
    
    [StructLayout(LayoutKind.Sequential)]
    public abstract class RingBufferFields<E>
    {
        protected Int64 p0;
        protected Int64 p1;
        protected Int64 p2;
        protected Int64 p3;
        protected Int64 p4;
        protected Int64 p5;
        protected Int64 p6;
        protected Int64 p7;
        private static int BUFFER_PAD = 128 / IntPtr.Size; 
        private long indexMask;
        private E[] entries;
        protected int bufferSize;
        protected ISequencer sequencer;
        public static long INITIAL_CURSOR_VALUE = Sequence.INITIAL_VALUE;
        protected long p8, p9, p10, p11, p12, p13, p14;
        protected Int64 p15;
    
        public RingBufferFields(IEventFactory<E> eventFactory,
                         ISequencer       sequencer)
        {
            this.sequencer  = sequencer;
            this.bufferSize = sequencer.getBufferSize();

            if (bufferSize < 1)
            {
                throw new ArgumentException("bufferSize must not be less than 1");
            }
            if (!Util.Util.IsPowerOf2(bufferSize))
            {
                throw new ArgumentException("bufferSize must be a power of 2");
            }

            this.indexMask = bufferSize - 1;
            this.entries = new E[sequencer.getBufferSize() + (2 * BUFFER_PAD)];
            fill(eventFactory);
        }

        private void fill(IEventFactory<E> eventFactory)
        {
            for (int i = 0; i < bufferSize; i++)
            {
                entries[i] = eventFactory.newInstance();
            }
        }

        
        protected E elementAt(long sequence)
        {
            return entries[(Int32)((sequence & indexMask))];
        }
    }

    /**
     * Ring based store of reusable entries containing the data representing
     * an event being exchanged between event producer and {@link EventProcessor}s.
     *
     * @param <E> implementation storing the data for sharing during exchange or parallel coordination of an event.
     */
    public class RingBuffer<E> : RingBufferFields<E>, IEventSequencer<E>, IEventSink<E>, ICursored 
    {
      

        /**
         * Construct a RingBuffer with the full option set.
         *
         * @param eventFactory to newInstance entries for filling the RingBuffer
         * @param sequencer sequencer to handle the ordering of events moving through the RingBuffer.
         * @throws IllegalArgumentException if bufferSize is less than 1 or not a power of 2
         */
        public RingBuffer(IEventFactory<E> eventFactory,ISequencer sequencer):base(eventFactory,sequencer)
        {   
        }

        /**
         * Create a new multiple producer RingBuffer with the specified wait strategy.
         *
         * @see MultiProducerSequencer
         * @param factory used to create the events within the ring buffer.
         * @param bufferSize number of elements to create within the ring buffer.
         * @param waitStrategy used to determine how to wait for new elements to become available.
         * @throws IllegalArgumentException if bufferSize is less than 1 or not a power of 2
         */
        public static RingBuffer<E> createMultiProducer<E>(IEventFactory<E> factory,
                                                            int             bufferSize,
                                                            IWaitStrategy    waitStrategy)
        {
            MultiProducerSequencer sequencer = new MultiProducerSequencer(bufferSize, waitStrategy);

            return new RingBuffer<E>(factory, sequencer);
        }

        /**
         * Create a new multiple producer RingBuffer using the default wait strategy  {@link BlockingWaitStrategy}.
         *
         * @see MultiProducerSequencer
         * @param factory used to create the events within the ring buffer.
         * @param bufferSize number of elements to create within the ring buffer.
         * @throws IllegalArgumentException if <tt>bufferSize</tt> is less than 1 or not a power of 2
         */
        public static RingBuffer<E> createMultiProducer<E>(IEventFactory<E> factory, int bufferSize)
        {
            return createMultiProducer(factory, bufferSize, new BlockingWaitStrategy());
        }

        /**
         * Create a new single producer RingBuffer with the specified wait strategy.
         *
         * @see SingleProducerSequencer
         * @param factory used to create the events within the ring buffer.
         * @param bufferSize number of elements to create within the ring buffer.
         * @param waitStrategy used to determine how to wait for new elements to become available.
         * @throws IllegalArgumentException if bufferSize is less than 1 or not a power of 2
         */
        public static RingBuffer<E> createSingleProducer<E>(IEventFactory<E> factory,
                                                             int             bufferSize,
                                                             IWaitStrategy    waitStrategy)
        {
            SingleProducerSequencer sequencer = new SingleProducerSequencer(bufferSize, waitStrategy);

            return new RingBuffer<E>(factory, sequencer);
        }

        /**
         * Create a new single producer RingBuffer using the default wait strategy  {@link BlockingWaitStrategy}.
         *
         * @see MultiProducerSequencer
         * @param factory used to create the events within the ring buffer.
         * @param bufferSize number of elements to create within the ring buffer.
         * @throws IllegalArgumentException if <tt>bufferSize</tt> is less than 1 or not a power of 2
         */
        public static RingBuffer<E> createSingleProducer<E>(IEventFactory<E> factory, int bufferSize)
        {
            return createSingleProducer(factory, bufferSize, new BlockingWaitStrategy());
        }

        /**
         * Create a new Ring Buffer with the specified producer type (SINGLE or MULTI)
         *
         * @param producerType producer type to use {@link ProducerType}.
         * @param factory used to create events within the ring buffer.
         * @param bufferSize number of elements to create within the ring buffer.
         * @param waitStrategy used to determine how to wait for new elements to become available.
         * @throws IllegalArgumentException if bufferSize is less than 1 or not a power of 2
         */
        public static RingBuffer<E> create<E>(ProducerType    producerType,
                                               IEventFactory<E> factory,
                                               int             bufferSize,
                                               IWaitStrategy    waitStrategy)
        {
            switch (producerType)
            {
                case  ProducerType.SINGLE:
                return createSingleProducer(factory, bufferSize, waitStrategy);
                case ProducerType.MULTI:
                    return createMultiProducer(factory, bufferSize, waitStrategy);
                default:
                    throw new ArgumentOutOfRangeException(producerType.ToString());
            }
        }

        /**
         * <p>Get the event for a given sequence in the RingBuffer.</p>
         *
         * <p>This call has 2 uses.  Firstly use this call when publishing to a ring buffer.
         * After calling {@link RingBuffer#next()} use this call to get hold of the
         * preallocated event to fill with data before calling {@link RingBuffer#publish(long)}.</p>
         *
         * <p>Secondly use this call when consuming data from the ring buffer.  After calling
         * {@link SequenceBarrier#waitFor(long)} call this method with any value greater than
         * that your current consumer sequence and less than or equal to the value returned from
         * the {@link SequenceBarrier#waitFor(long)} method.</p>
         *
         * @param sequence for the event
         * @return the event for the given sequence
         */
      
        public E get(long sequence)
        {
            return elementAt(sequence);
        }

        /**
         * Increment and return the next sequence for the ring buffer.  Calls of this
         * method should ensure that they always publish the sequence afterward.  E.g.
         * <pre>
         * long sequence = ringBuffer.next();
         * try {
         *     Event e = ringBuffer.get(sequence);
         *     // Do some work with the event.
         * } finally {
         *     ringBuffer.publish(sequence);
         * }
         * </pre>
         * @see RingBuffer#publish(long)
         * @see RingBuffer#get(long)
         * @return The next sequence to publish to.
         */
        
        public long next()
        {
            return sequencer.next();
        }

        /**
         * The same functionality as {@link RingBuffer#next()}, but allows the caller to claim
         * the next n sequences.
         *
         * @see Sequencer#next(int)
         * @param n number of slots to claim
         * @return sequence number of the highest slot claimed
         */
        
        public long next(int n)
        {
            return sequencer.next(n);
        }

        /**
         * <p>Increment and return the next sequence for the ring buffer.  Calls of this
         * method should ensure that they always publish the sequence afterward.  E.g.
         * <pre>
         * long sequence = ringBuffer.next();
         * try {
         *     Event e = ringBuffer.get(sequence);
         *     // Do some work with the event.
         * } finally {
         *     ringBuffer.publish(sequence);
         * }
         * </pre>
         * <p>This method will not block if there is not space available in the ring
         * buffer, instead it will throw an {@link InsufficientCapacityException}.
         *
         *
         * @see RingBuffer#publish(long)
         * @see RingBuffer#get(long)
         * @return The next sequence to publish to.
         * @throws InsufficientCapacityException if the necessary space in the ring buffer is not available
         */
       
        public long tryNext()
        {
            return sequencer.tryNext();
        }

        /**
         * The same functionality as {@link RingBuffer#tryNext()}, but allows the caller to attempt
         * to claim the next n sequences.
         *
         * @param n number of slots to claim
         * @return sequence number of the highest slot claimed
         * @throws InsufficientCapacityException if the necessary space in the ring buffer is not available
         */
       
        public long tryNext(int n)
        {
            return sequencer.tryNext(n);
        }

        /**
         * Resets the cursor to a specific value.  This can be applied at any time, but it is worth noting
         * that it can cause a data race and should only be used in controlled circumstances.  E.g. during
         * initialisation.
         *
         * @param sequence The sequence to reset too.
         * @throws IllegalStateException If any gating sequences have already been specified.
         */
       
        public void resetTo(long sequence)
        {
            sequencer.claim(sequence);
            sequencer.publish(sequence);
        }

        /**
         * Sets the cursor to a specific sequence and returns the preallocated entry that is stored there.  This
         * can cause a data race and should only be done in controlled circumstances, e.g. during initialisation.
         *
         * @param sequence The sequence to claim.
         * @return The preallocated event.
         */
        public E claimAndGetPreallocated(long sequence)
        {
            sequencer.claim(sequence);
            return get(sequence);
        }

        /**
         * Determines if a particular entry has been published.
         *
         * @param sequence The sequence to identify the entry.
         * @return If the value has been published or not.
         */
        public Boolean isPublished(long sequence)
        {
            return sequencer.isAvailable(sequence);
        }

        /**
         * Add the specified gating sequences to this instance of the Disruptor.  They will
         * safely and atomically added to the list of gating sequences.
         *
         * @param gatingSequences The sequences to add.
         */
        public void addGatingSequences(params Sequence[] gatingSequences)
        {
            sequencer.addGatingSequences(gatingSequences);
        }

        /**
         * Get the minimum sequence value from all of the gating sequences
         * added to this ringBuffer.
         *
         * @return The minimum gating sequence or the cursor sequence if
         * no sequences have been added.
         */
        public long getMinimumGatingSequence()
        {
            return sequencer.getMinimumSequence();
        }

        /**
         * Remove the specified sequence from this ringBuffer.
         *
         * @param sequence to be removed.
         * @return <tt>true</tt> if this sequence was found, <tt>false</tt> otherwise.
         */
        public Boolean removeGatingSequence(Sequence sequence)
        {
            return sequencer.removeGatingSequence(sequence);
        }

        /**
         * Create a new SequenceBarrier to be used by an EventProcessor to track which messages
         * are available to be read from the ring buffer given a list of sequences to track.
         *
         * @see SequenceBarrier
         * @param sequencesToTrack the additional sequences to track
         * @return A sequence barrier that will track the specified sequences.
         */
        public ISequenceBarrier newBarrier(params Sequence[] sequencesToTrack)
        {
            return sequencer.newBarrier(sequencesToTrack);
        }

        /**
         * Creates an event poller for this ring buffer gated on the supplied sequences.
         *
         * @param gatingSequences
         * @return A poller that will gate on this ring buffer and the supplied sequences.
         */
        public EventPoller<E> newPoller(params Sequence[] gatingSequences)
        {
            return sequencer.newPoller(this, gatingSequences);
        }

        /**
         * Get the current cursor value for the ring buffer.  The actual value recieved
         * will depend on the type of {@link Sequencer} that is being used.
         *
         * @see MultiProducerSequencer
         * @see SingleProducerSequencer
         */
        
        public long getCursor()
        {
            return sequencer.getCursor();
        }

        /**
         * The size of the buffer.
         */
        public int getBufferSize()
        {
            return bufferSize;
        }

        /**
         * Given specified <tt>requiredCapacity</tt> determines if that amount of space
         * is available.  Note, you can not assume that if this method returns <tt>true</tt>
         * that a call to {@link RingBuffer#next()} will not block.  Especially true if this
         * ring buffer is set up to handle multiple producers.
         *
         * @param requiredCapacity The capacity to check for.
         * @return <tt>true</tt> If the specified <tt>requiredCapacity</tt> is available
         * <tt>false</tt> if now.
         */
        public Boolean hasAvailableCapacity(int requiredCapacity)
        {
            return sequencer.hasAvailableCapacity(requiredCapacity);
        }


        /**
         * @see com.lmax.disruptor.EventSink#publishEvent(com.lmax.disruptor.EventTranslator)
         */
     
        public void publishEvent(IEventTranslator<E> translator)
        {
            long sequence = sequencer.next();
            translateAndPublish(translator, sequence);
        }

        /**
         * @see com.lmax.disruptor.EventSink#tryPublishEvent(com.lmax.disruptor.EventTranslator)
         */
        
        public Boolean tryPublishEvent(IEventTranslator<E> translator)
        {
            try
            {
                long sequence = sequencer.tryNext();
                translateAndPublish(translator, sequence);
                return true;
            }
            catch (InsufficientCapacityException)
            {
                return false;
            }
        }

        /**
         * @see com.lmax.disruptor.EventSink#publishEvent(com.lmax.disruptor.EventTranslatorOneArg, Object)
         *      com.lmax.disruptor.EventSink#publishEvent(com.lmax.disruptor.EventTranslatorOneArg, A)
         */
       
        public  void publishEvent<A>(IEventTranslatorOneArg<E, A> translator, A arg0)
        {
            long sequence = sequencer.next();
            translateAndPublish(translator, sequence, arg0);
        }

        /**
         * @see com.lmax.disruptor.EventSink#tryPublishEvent(com.lmax.disruptor.EventTranslatorOneArg, Object)
         *      com.lmax.disruptor.EventSink#tryPublishEvent(com.lmax.disruptor.EventTranslatorOneArg, A)
         */
       
        public Boolean tryPublishEvent<A>(IEventTranslatorOneArg<E, A> translator, A arg0)
        {
            try
            {
                long sequence = sequencer.tryNext();
                translateAndPublish(translator, sequence, arg0);
                return true;
            }
            catch (InsufficientCapacityException)
            {
                return false;
            }
        }

        /**
         * @see com.lmax.disruptor.EventSink#publishEvent(com.lmax.disruptor.EventTranslatorTwoArg, Object, Object)
         *      com.lmax.disruptor.EventSink#publishEvent(com.lmax.disruptor.EventTranslatorTwoArg, A, B)
         */
       
        public void publishEvent<A, B>(IEventTranslatorTwoArg<E, A, B> translator, A arg0, B arg1)
        {
            long sequence = sequencer.next();
            translateAndPublish(translator, sequence, arg0, arg1);
        }

        /**
         * @see com.lmax.disruptor.EventSink#tryPublishEvent(com.lmax.disruptor.EventTranslatorTwoArg, Object, Object)
         *      com.lmax.disruptor.EventSink#tryPublishEvent(com.lmax.disruptor.EventTranslatorTwoArg, A, B)
         */
       
        public Boolean tryPublishEvent<A, B>(IEventTranslatorTwoArg<E, A, B> translator, A arg0, B arg1)
        {
            try
            {
                long sequence = sequencer.tryNext();
                translateAndPublish(translator, sequence, arg0, arg1);
                return true;
            }
            catch (InsufficientCapacityException)
            {
                return false;
            }
        }

        /**
         * @see com.lmax.disruptor.EventSink#publishEvent(com.lmax.disruptor.EventTranslatorThreeArg, Object, Object, Object)
         *      com.lmax.disruptor.EventSink#publishEvent(com.lmax.disruptor.EventTranslatorThreeArg, A, B, C)
         */
        
        public void publishEvent<A, B, C>(IEventTranslatorThreeArg<E, A, B, C> translator, A arg0, B arg1, C arg2)
        {
            long sequence = sequencer.next();
            translateAndPublish(translator, sequence, arg0, arg1, arg2);
        }

        /**
         * @see com.lmax.disruptor.EventSink#tryPublishEvent(com.lmax.disruptor.EventTranslatorThreeArg, Object, Object, Object)
         *      com.lmax.disruptor.EventSink#tryPublishEvent(com.lmax.disruptor.EventTranslatorThreeArg, A, B, C)
         */
        
        public Boolean tryPublishEvent<A, B, C>(IEventTranslatorThreeArg<E, A, B, C> translator, A arg0, B arg1, C arg2)
        {
            try
            {
                long sequence = sequencer.tryNext();
                translateAndPublish(translator, sequence, arg0, arg1, arg2);
                return true;
            }
            catch (InsufficientCapacityException)
            {
                return false;
            }
        }

        /**
         * @see com.lmax.disruptor.EventSink#publishEvent(com.lmax.disruptor.EventTranslatorVararg, java.lang.Object...)
         */
       
        public void publishEvent(IEventTranslatorVararg<E> translator, params Object[] args)
        {
             long sequence = sequencer.next();
            translateAndPublish(translator, sequence, args);
        }

        /**
         * @see com.lmax.disruptor.EventSink#tryPublishEvent(com.lmax.disruptor.EventTranslatorVararg, java.lang.Object...)
         */
       
        public Boolean tryPublishEvent(IEventTranslatorVararg<E> translator, params Object[] args)
        {
            try
            {
                long sequence = sequencer.tryNext();
                translateAndPublish(translator, sequence, args);
                return true;
            }
            catch (InsufficientCapacityException)
            {
                return false;
            }
        }


        /**
         * @see com.lmax.disruptor.EventSink#publishEvents(com.lmax.disruptor.EventTranslator[])
         */
       
        public void publishEvents(IEventTranslator<E>[] translators)
        {
            publishEvents(translators, 0, translators.Length);
        }

        /**
         * @see com.lmax.disruptor.EventSink#publishEvents(com.lmax.disruptor.EventTranslator[], int, int)
         */
       
        public void publishEvents(IEventTranslator<E>[] translators, int batchStartsAt, int batchSize)
        {
            checkBounds(translators, batchStartsAt, batchSize);
            long finalSequence = sequencer.next(batchSize);
            translateAndPublishBatch(translators, batchStartsAt, batchSize, finalSequence);
        }

        /**
         * @see com.lmax.disruptor.EventSink#tryPublishEvents(com.lmax.disruptor.EventTranslator[])
         */
        
        public Boolean tryPublishEvents(IEventTranslator<E>[] translators)
        {
            return tryPublishEvents(translators, 0, translators.Length);
        }

        /**
         * @see com.lmax.disruptor.EventSink#tryPublishEvents(com.lmax.disruptor.EventTranslator[], int, int)
         */
        
        public Boolean tryPublishEvents(IEventTranslator<E>[] translators, int batchStartsAt, int batchSize)
        {
            checkBounds(translators, batchStartsAt, batchSize);
            try
            {
                long finalSequence = sequencer.tryNext(batchSize);
                translateAndPublishBatch(translators, batchStartsAt, batchSize, finalSequence);
                return true;
            }
            catch (InsufficientCapacityException)
            {
                return false;
            }
        }

        /**
         * @see com.lmax.disruptor.EventSink#publishEvents(com.lmax.disruptor.EventTranslatorOneArg, Object[])
         *      com.lmax.disruptor.EventSink#publishEvents(com.lmax.disruptor.EventTranslatorOneArg, A[])
         */
      
        public void publishEvents<A>(IEventTranslatorOneArg<E, A> translator, A[] arg0)
        {
            publishEvents(translator, 0, arg0.Length, arg0);
        }

        /**
         * @see com.lmax.disruptor.EventSink#publishEvents(com.lmax.disruptor.EventTranslatorOneArg, int, int, Object[])
         *      com.lmax.disruptor.EventSink#publishEvents(com.lmax.disruptor.EventTranslatorOneArg, int, int, A[])
         */
       
        public void publishEvents<A>(IEventTranslatorOneArg<E, A> translator, int batchStartsAt, int batchSize, A[] arg0)
        {
            checkBounds(arg0, batchStartsAt, batchSize);
            long finalSequence = sequencer.next(batchSize);
            translateAndPublishBatch(translator, arg0, batchStartsAt, batchSize, finalSequence);
        }

        /**
         * @see com.lmax.disruptor.EventSink#tryPublishEvents(com.lmax.disruptor.EventTranslatorOneArg, Object[])
         *      com.lmax.disruptor.EventSink#tryPublishEvents(com.lmax.disruptor.EventTranslatorOneArg, A[])
         */
       
        public Boolean tryPublishEvents<A>(IEventTranslatorOneArg<E, A> translator, A[] arg0)
        {
            return tryPublishEvents(translator, 0, arg0.Length, arg0);
        }

        /**
         * @see com.lmax.disruptor.EventSink#tryPublishEvents(com.lmax.disruptor.EventTranslatorOneArg, int, int, Object[])
         *      com.lmax.disruptor.EventSink#tryPublishEvents(com.lmax.disruptor.EventTranslatorOneArg, int, int, A[])
         */
       
        public Boolean tryPublishEvents<A>(IEventTranslatorOneArg<E, A> translator, int batchStartsAt, int batchSize, A[] arg0)
        {
            checkBounds(arg0, batchStartsAt, batchSize);
            try
            {
                long finalSequence = sequencer.tryNext(batchSize);
                translateAndPublishBatch(translator, arg0, batchStartsAt, batchSize, finalSequence);
                return true;
            }
            catch (InsufficientCapacityException)
            {
                return false;
            }
        }

        /**
         * @see com.lmax.disruptor.EventSink#publishEvents(com.lmax.disruptor.EventTranslatorTwoArg, Object[], Object[])
         *      com.lmax.disruptor.EventSink#publishEvents(com.lmax.disruptor.EventTranslatorTwoArg, A[], B[])
         */
      
        public void publishEvents<A, B>(IEventTranslatorTwoArg<E, A, B> translator, A[] arg0, B[] arg1)
        {
            publishEvents(translator, 0, arg0.Length, arg0, arg1);
        }

        /**
         * @see com.lmax.disruptor.EventSink#publishEvents(com.lmax.disruptor.EventTranslatorTwoArg, int, int, Object[], Object[])
         *      com.lmax.disruptor.EventSink#publishEvents(com.lmax.disruptor.EventTranslatorTwoArg, int, int, A[], B[])
         */
        
        public void publishEvents<A, B>(IEventTranslatorTwoArg<E, A, B> translator, int batchStartsAt, int batchSize, A[] arg0, B[] arg1)
        {
            checkBounds(arg0, arg1, batchStartsAt, batchSize);
            long finalSequence = sequencer.next(batchSize);
            translateAndPublishBatch(translator, arg0, arg1, batchStartsAt, batchSize, finalSequence);
        }

        /**
         * @see com.lmax.disruptor.EventSink#tryPublishEvents(com.lmax.disruptor.EventTranslatorTwoArg, Object[], Object[])
         *      com.lmax.disruptor.EventSink#tryPublishEvents(com.lmax.disruptor.EventTranslatorTwoArg, A[], B[])
         */
       
        public Boolean tryPublishEvents<A, B> (IEventTranslatorTwoArg<E, A, B> translator, A[] arg0, B[] arg1)
        {
            return tryPublishEvents(translator, 0, arg0.Length, arg0, arg1);
        }

        /**
         * @see com.lmax.disruptor.EventSink#tryPublishEvents(com.lmax.disruptor.EventTranslatorTwoArg, int, int, Object[], Object[])
         *      com.lmax.disruptor.EventSink#tryPublishEvents(com.lmax.disruptor.EventTranslatorTwoArg, int, int, A[], B[])
         */
     
        public Boolean tryPublishEvents<A, B>(IEventTranslatorTwoArg<E, A, B> translator, int batchStartsAt, int batchSize, A[] arg0, B[] arg1)
        {
            checkBounds(arg0, arg1, batchStartsAt, batchSize);
            try
            {
                long finalSequence = sequencer.tryNext(batchSize);
                translateAndPublishBatch(translator, arg0, arg1, batchStartsAt, batchSize, finalSequence);
                return true;
            }
            catch (InsufficientCapacityException)
            {
                return false;
            }
        }

        /**
         * @see com.lmax.disruptor.EventSink#publishEvents(com.lmax.disruptor.EventTranslatorThreeArg, Object[], Object[], Object[])
         *      com.lmax.disruptor.EventSink#publishEvents(com.lmax.disruptor.EventTranslatorThreeArg, A[], B[], C[])
         */
        public void publishEvents<A, B, C> (IEventTranslatorThreeArg<E, A, B, C> translator, A[] arg0, B[] arg1, C[] arg2)
        {
            publishEvents(translator, 0, arg0.Length, arg0, arg1, arg2);
        }

        /**
         * @see com.lmax.disruptor.EventSink#publishEvents(com.lmax.disruptor.EventTranslatorThreeArg, int, int, Object[], Object[], Object[])
         *      com.lmax.disruptor.EventSink#publishEvents(com.lmax.disruptor.EventTranslatorThreeArg, int, int, A[], B[], C[])
         */
       
        public void publishEvents<A, B, C> (IEventTranslatorThreeArg<E, A, B, C> translator, int batchStartsAt, int batchSize, A[] arg0, B[] arg1, C[] arg2)
        {
            checkBounds(arg0, arg1, arg2, batchStartsAt, batchSize);
            long finalSequence = sequencer.next(batchSize);
            translateAndPublishBatch(translator, arg0, arg1, arg2, batchStartsAt, batchSize, finalSequence);
        }

        /**
         * @see com.lmax.disruptor.EventSink#tryPublishEvents(com.lmax.disruptor.EventTranslatorThreeArg, Object[], Object[], Object[])
         *      com.lmax.disruptor.EventSink#tryPublishEvents(com.lmax.disruptor.EventTranslatorThreeArg, A[], B[], C[])
         */
       
        public Boolean tryPublishEvents<A, B, C>(IEventTranslatorThreeArg<E, A, B, C> translator, A[] arg0, B[] arg1, C[] arg2)
        {
            return tryPublishEvents(translator, 0, arg0.Length, arg0, arg1, arg2);
        }

        /**
         * @see com.lmax.disruptor.EventSink#tryPublishEvents(com.lmax.disruptor.EventTranslatorThreeArg, int, int, Object[], Object[], Object[])
         *      com.lmax.disruptor.EventSink#tryPublishEvents(com.lmax.disruptor.EventTranslatorThreeArg, int, int, A[], B[], C[])
         */
       
        public Boolean tryPublishEvents<A, B, C>(IEventTranslatorThreeArg<E, A, B, C> translator, int batchStartsAt, int batchSize, A[] arg0, B[] arg1, C[] arg2)
        {
            checkBounds(arg0, arg1, arg2, batchStartsAt, batchSize);
            try
            {
                long finalSequence = sequencer.tryNext(batchSize);
                translateAndPublishBatch(translator, arg0, arg1, arg2, batchStartsAt, batchSize, finalSequence);
                return true;
            }
            catch (InsufficientCapacityException)
            {
                return false;
            }
        }

        /**
         * @see com.lmax.disruptor.EventSink#publishEvents(com.lmax.disruptor.EventTranslatorVararg, java.lang.Object[][])
         */
       
        public void publishEvents(IEventTranslatorVararg<E> translator, params Object[][] args)
        {
            publishEvents(translator, 0, args.Length, args);
        }

        /**
         * @see com.lmax.disruptor.EventSink#publishEvents(com.lmax.disruptor.EventTranslatorVararg, int, int, java.lang.Object[][])
         */
       
        public void publishEvents(IEventTranslatorVararg<E> translator, int batchStartsAt, int batchSize, params Object[][] args)
        {
            checkBounds(batchStartsAt, batchSize, args);
            long finalSequence = sequencer.next(batchSize);
            translateAndPublishBatch(translator, batchStartsAt, batchSize, finalSequence, args);
        }

        /**
         * @see com.lmax.disruptor.EventSink#tryPublishEvents(com.lmax.disruptor.EventTranslatorVararg, java.lang.Object[][])
         */
       
        public Boolean tryPublishEvents(IEventTranslatorVararg<E> translator, params Object[][] args)
        {
            return tryPublishEvents(translator, 0, args.Length, args);
        }

        /**
         * @see com.lmax.disruptor.EventSink#tryPublishEvents(com.lmax.disruptor.EventTranslatorVararg, int, int, java.lang.Object[][])
         */
       
        public Boolean tryPublishEvents(IEventTranslatorVararg<E> translator, int batchStartsAt, int batchSize, params Object[][] args)
        {
            checkBounds(args, batchStartsAt, batchSize);
            try
            {
                long finalSequence = sequencer.tryNext(batchSize);
                translateAndPublishBatch(translator, batchStartsAt, batchSize, finalSequence, args);
                return true;
            }
            catch (InsufficientCapacityException)
            {
                return false;
            }
        }

        /**
         * Publish the specified sequence.  This action marks this particular
         * message as being available to be read.
         *
         * @param sequence the sequence to publish.
         */
        public void publish(long sequence)
        {
            sequencer.publish(sequence);
        }

        /**
         * Publish the specified sequences.  This action marks these particular
         * messages as being available to be read.
         *
         * @see Sequencer#next(int)
         * @param lo the lowest sequence number to be published
         * @param hi the highest sequence number to be published
         */
       
        public void publish(long lo, long hi)
        {
            sequencer.publish(lo, hi);
        }

        /**
         * Get the remaining capacity for this ringBuffer.
         * @return The number of slots remaining.
         */
        public long remainingCapacity()
        {
            return sequencer.remainingCapacity();
        }

        private void checkBounds(IEventTranslator<E>[] translators, int batchStartsAt, int batchSize)
        {
            checkBatchSizing(batchStartsAt, batchSize);
            batchOverRuns(translators, batchStartsAt, batchSize);
        }

        private void checkBatchSizing(int batchStartsAt, int batchSize)
        {
            if(batchStartsAt < 0 || batchSize < 0)
            {
                throw new  ArgumentOutOfRangeException("Both batchStartsAt and batchSize must be positive but got: batchStartsAt " + batchStartsAt + " and batchSize " + batchSize);
            }
            else if(batchSize > bufferSize)
            {
                throw new ArgumentOutOfRangeException("The ring buffer cannot accommodate " + batchSize + " it only has space for " + bufferSize + " entities.");
            }
        }

        private  void checkBounds<A>(A[] arg0, int batchStartsAt, int batchSize)
        {
            checkBatchSizing(batchStartsAt, batchSize);
            batchOverRuns(arg0, batchStartsAt, batchSize);
        }

        private void checkBounds<A, B>(A[] arg0, B[] arg1, int batchStartsAt, int batchSize)
        {
            checkBatchSizing(batchStartsAt, batchSize);
            batchOverRuns(arg0, batchStartsAt, batchSize);
            batchOverRuns(arg1, batchStartsAt, batchSize);
        }

        private  void checkBounds<A, B, C>(A[] arg0, B[] arg1, C[] arg2, int batchStartsAt, int batchSize)
        {
            checkBatchSizing(batchStartsAt, batchSize);
            batchOverRuns(arg0, batchStartsAt, batchSize);
            batchOverRuns(arg1, batchStartsAt, batchSize);
            batchOverRuns(arg2, batchStartsAt, batchSize);
        }

        private void checkBounds(int batchStartsAt, int batchSize, Object[][] args)
        {
            checkBatchSizing(batchStartsAt, batchSize);
            batchOverRuns(args, batchStartsAt, batchSize);
        }

        private  void batchOverRuns<A>(A[] arg0, int batchStartsAt, int batchSize)
        {
            if(batchStartsAt + batchSize > arg0.Length)
            {
                throw new ArgumentOutOfRangeException("A batchSize of: " + batchSize +
                                                   " with batchStatsAt of: " + batchStartsAt +
                                                   " will overrun the available number of arguments: " + (arg0.Length - batchStartsAt));
            }
        }

        private void translateAndPublish(IEventTranslator<E> translator, long sequence)
        {
            try
            {
                translator.translateTo(get(sequence), sequence);
            }
            finally
            {
                sequencer.publish(sequence);
            }
        }

        private  void translateAndPublish<A>(IEventTranslatorOneArg<E, A> translator, long sequence, A arg0)
        {
            try
            {
                translator.translateTo(get(sequence), sequence, arg0);
            }
            finally
            {
                sequencer.publish(sequence);
            }
        }

        private  void translateAndPublish<A, B>(IEventTranslatorTwoArg<E, A, B> translator, long sequence, A arg0, B arg1)
        {
            try
            {
                translator.translateTo(get(sequence), sequence, arg0, arg1);
            }
            finally
            {
                sequencer.publish(sequence);
            }
        }

        private void translateAndPublish<A, B, C>(IEventTranslatorThreeArg<E, A, B, C> translator, long sequence,
                                                   A arg0, B arg1, C arg2)
        {
            try
            {
                translator.translateTo(get(sequence), sequence, arg0, arg1, arg2);
            }
            finally
            {
                sequencer.publish(sequence);
            }
        }

        private void translateAndPublish(IEventTranslatorVararg<E> translator, long sequence, params Object[] args)
        {
            try
            {
                translator.translateTo(get(sequence), sequence, args);
            }
            finally
            {
                sequencer.publish(sequence);
            }
        }

        private void translateAndPublishBatch(IEventTranslator<E>[] translators, int batchStartsAt,
                                              int batchSize, long finalSequence)
        {
            long initialSequence = finalSequence - (batchSize - 1);
            try
            {
                long sequence = initialSequence;
                int batchEndsAt = batchStartsAt + batchSize;
                for (int i = batchStartsAt; i < batchEndsAt; i++)
                {
                    IEventTranslator<E> translator = translators[i];
                    translator.translateTo(get(sequence), sequence++);
                }
            }
            finally
            {
                sequencer.publish(initialSequence, finalSequence);
            }
        }

        private  void translateAndPublishBatch<A>(IEventTranslatorOneArg<E, A> translator, A[] arg0,
                                                  int batchStartsAt, int batchSize, long finalSequence)
        {
            long initialSequence = finalSequence - (batchSize - 1);
            try
            {
                long sequence = initialSequence;
                int batchEndsAt = batchStartsAt + batchSize;
                for (int i = batchStartsAt; i < batchEndsAt; i++)
                {
                    translator.translateTo(get(sequence), sequence++, arg0[i]);
                }
            }
            finally
            {
                sequencer.publish(initialSequence, finalSequence);
            }
        }

        private void translateAndPublishBatch<A, B> (IEventTranslatorTwoArg<E, A, B> translator, A[] arg0,
                                                     B[] arg1, int batchStartsAt, int batchSize,
                                                     long finalSequence)
        {
            long initialSequence = finalSequence - (batchSize - 1);
            try
            {
                long sequence = initialSequence;
                int batchEndsAt = batchStartsAt + batchSize;
                for (int i = batchStartsAt; i < batchEndsAt; i++)
                {
                    translator.translateTo(get(sequence), sequence++, arg0[i], arg1[i]);
                }
            }
            finally
            {
                sequencer.publish(initialSequence, finalSequence);
            }
        }

        private  void translateAndPublishBatch<A, B, C>(IEventTranslatorThreeArg<E, A, B, C> translator,
                                                        A[] arg0, B[] arg1, C[] arg2, int batchStartsAt,
                                                        int batchSize, long finalSequence)
        {
            long initialSequence = finalSequence - (batchSize - 1);
            try
            {
                long sequence = initialSequence;
                int batchEndsAt = batchStartsAt + batchSize;
                for (int i = batchStartsAt; i < batchEndsAt; i++)
                {
                    translator.translateTo(get(sequence), sequence++, arg0[i], arg1[i], arg2[i]);
                }
            }
            finally
            {
                sequencer.publish(initialSequence, finalSequence);
            }
        }

        private void translateAndPublishBatch(IEventTranslatorVararg<E> translator, int batchStartsAt,
                                              int batchSize, long finalSequence, Object[][] args)
        {
            long initialSequence = finalSequence - (batchSize - 1);
            try
            {
                long sequence = initialSequence;
                int batchEndsAt = batchStartsAt + batchSize;
                for (int i = batchStartsAt; i < batchEndsAt; i++)
                {
                    translator.translateTo(get(sequence), sequence++, args[i]);
                }
            }
            finally
            {
                sequencer.publish(initialSequence, finalSequence);
            }
        }
    }
}
