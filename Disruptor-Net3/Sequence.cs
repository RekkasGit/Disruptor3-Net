using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Disruptor_Net3
{

  
    [StructLayout(LayoutKind.Explicit, Size = 128)]
    public struct PaddedLong
    {
        [FieldOffset(0)]Int64 p1;
        [FieldOffset(8)]Int64 p2;
        [FieldOffset(16)]Int64 p3;
        [FieldOffset(24)]Int64 p4;
        [FieldOffset(32)]Int64 p5;
        [FieldOffset(40)]Int64 p6;
        [FieldOffset(48)]Int64 p7;
        [FieldOffset(56)]Int64 p8;
        [FieldOffset(64)]public Int64 value;
        [FieldOffset(72)]Int64 p9;
        [FieldOffset(80)]Int64 p10;
        [FieldOffset(88)]Int64 p11;
        [FieldOffset(96)]Int64 p12;
        [FieldOffset(104)]Int64 p13;
        [FieldOffset(112)]Int64 p14;
        [FieldOffset(120)]Int64 p15;
    }

     /**
     * <p>Concurrent sequence class used for tracking the progress of
     * the ring buffer and event processors.  Support a number
     * of concurrent operations including CAS and order writes.
     *
     * <p>Also attempts to be more efficient with regards to false
     * sharing by adding padding around the volatile field.
     */
    public class Sequence
    {
        public PaddedLong paddedValue = new PaddedLong();
        public static Int64 INITIAL_VALUE = -1L;
        
        /**
         * Create a sequence initialised to -1.
         */
        public Sequence():this(INITIAL_VALUE)
        {

        }
       
        /**
         * Create a sequence with a specified initial value.
         *
         * @param initialValue The initial value for this sequence.
         */
        public Sequence(long initialValue)
        {
            paddedValue.value = initialValue;
        }

        /**
         * Perform a volatile read of this sequence's value.
         *
         * @return The current value of the sequence.
         */
        
        public virtual long get()
        {
            return Volatile.Read(ref  paddedValue.value);
        }

        /**
         * Perform an ordered write of this sequence.  The intent is
         * a Store/Store barrier between this write and any previous
         * store.
         *
         * @param value The new value for the sequence.
         */
        //http://psy-lob-saw.blogspot.com/2012/12/atomiclazyset-is-performance-win-for.html
        /*
         * Martin Thompson15 August 2014 at 18:02
        My understanding of the .Net Memory Model is that it is stronger than the Java Memory Model, particularly for field access - writes to a field have StoreStore ordering.
        http://msdn.microsoft.com/en-us/magazine/jj863136.aspx //memory modle 1
        https://msdn.microsoft.com/en-us/magazine/jj883956.aspx // memory modle 2
        */
        public virtual void set(long value)
        {
            paddedValue.value = value;
            //Interlocked.Exchange(ref paddedValue.value,value);
        }

        /**
         * Performs a volatile write of this sequence.  The intent is
         * a Store/Store barrier between this write and any previous
         * write and a Store/Load barrier between this write and any
         * subsequent volatile read.
         *
         * @param value The new value for the sequence.
         */
        public virtual void setVolatile(long value)
        {
            Volatile.Write(ref paddedValue.value, value);
        }

        /**
         * Perform a compare and set operation on the sequence.
         *
         * @param expectedValue The expected current value.
         * @param newValue The value to update to.
         * @return true if the operation succeeds, false otherwise.
         */
        public virtual Boolean compareAndSet(long expectedValue, long newValue)
        {
            return  Interlocked.CompareExchange(ref paddedValue.value,newValue,expectedValue)==expectedValue;
        }

        /**
         * Atomically increment the sequence by one.
         *
         * @return The value after the increment
         */
        public virtual long incrementAndGet()
        {

            return addAndGet(1L);
        }

        /**
         * Atomically add the supplied value.
         *
         * @param increment The value to add to the sequence.
         * @return The value after the increment.
         */
        public virtual long addAndGet(long increment)
        {
            long currentValue;
            long newValue;

            do
            {
                currentValue = get();
                newValue = currentValue + increment;
            }
            while (!compareAndSet(currentValue, newValue));

            return newValue;
        }


        public virtual String toString()
        {
            return get().ToString();
        }
    }
}
