using Disruptor3_Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.Util
{
    public static class Util
    {
        /**
        * Calculate the next power of 2, greater than or equal to x.<p>
        * From Hacker's Delight, Chapter 3, Harry S. Warren Jr.
        *
        * @param x Value to round up
        * @return The next power of 2 from x inclusive
        */
        public static Int32 ceilingNextPowerOfTwo(int x)
        {
            return 1 << (32 - LeadingZeros(x - 1));
        }
        //http://stackoverflow.com/questions/10439242/count-leading-zeroes-in-an-int32
        public static int LeadingZeros(int x)
        {
            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);
            return (sizeof(int) * 8 - Ones(x));
        }
        public static int Ones(int x)
        {
            x -= ((x >> 1) & 0x55555555);
            x = (((x >> 2) & 0x33333333) + (x & 0x33333333));
            x = (((x >> 4) + x) & 0x0f0f0f0f);
            x += (x >> 8);
            x += (x >> 16);
            return (x & 0x0000003f);
        }

        /**
        * Get the minimum sequence from an array of {@link com.lmax.disruptor.Sequence}s.
        *
        * @param sequences to compare.
        * @return the minimum sequence found or Long.MAX_VALUE if the array is empty.
        */
        public static long getMinimumSequence(Sequence[] sequences)
        {
            return getMinimumSequence(sequences, Int64.MaxValue);
        }

        /**
         * Get the minimum sequence from an array of {@link com.lmax.disruptor.Sequence}s.
         *
         * @param sequences to compare.
         * @param minimum an initial default minimum.  If the array is empty this value will be
         * returned.
         * @return the minimum sequence found or Long.MAX_VALUE if the array is empty.
         */
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long getMinimumSequence(Sequence[] sequences, Int64 minimum)
        {

            Int32 maxLength = sequences.Length - AbstractSequencer.numberToPad;
            for (int i = AbstractSequencer.numberToPad; i <maxLength && i < sequences.Length; i++)
            {
                Int64 value = sequences[i].get();
                if (value < minimum)
                {
                    minimum = value;
                }
                //minimum = Math.Min(minimum, value);
            }
            return minimum;
        }

        /**
         * Get an array of {@link Sequence}s for the passed {@link EventProcessor}s
         *
         * @param processors for which to get the sequences
         * @return the array of {@link Sequence}s
         */
        public static Sequence[] getSequencesFor(params IEventProcessor[] processors)
        {
            Sequence[] sequences = new Sequence[processors.Length];
            for (int i = 0; i < sequences.Length && i < processors.Length; i++)
            {
                sequences[i] = processors[i].getSequence();
            }

            return sequences;
        }
        /**
         * Calculate the log base 2 of the supplied integer, essentially reports the location
         * of the highest bit.
         *
         * @param i Value to calculate log2 for.
         * @return The log2 value
         */
        public static int log2(int i)
        {
            int r = 0;
            while ((i >>= 1) != 0)
            {
                ++r;
            }
            return r;
        }
        public unsafe static Int32 GetInt32FromArray(byte[] source, int index)
        {
            fixed (byte* p = &source[0])
            {
                return *(Int32*)(p + index);
            }
        }
        public unsafe static void SetInt32IntoArray(byte[] target, int index, Int32 value)
        {
            fixed (byte* p = &target[index])
            {
                *((Int32*)p) = value;
            }
        }



        public unsafe static Int64 GetInt64FromArray(byte[] source, int index)
        {
            fixed (byte* p = &source[0])
            {
                return *(Int64*)(p + index);
            }
        }
        public unsafe static void SetInt64IntoArray(byte[] target, int index, Int64 value)
        {
            fixed (byte* p = &target[index])
            {
                *((Int64*)p) = value;
            }
        }
        /// <summary>
        /// Test whether a given integer is a power of 2 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static bool IsPowerOf2(this int x)
        {
            return x > 0 && (x & (x - 1)) == 0;
        }

    }
}
