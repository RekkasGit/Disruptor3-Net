using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor_Net3
{
    public class FixedSequenceGroup:Sequence
    {
        private Sequence[] sequences;

        /**
         * Constructor
         *
         * @param sequences the list of sequences to be tracked under this sequence group
         */
        public FixedSequenceGroup(Sequence[] sequences)
        {
            this.sequences = sequences.ToArray();
        }

        /**
         * Get the minimum sequence value for the group.
         *
         * @return the minimum sequence value for the group.
         */
      
        public override long get()
        {
            return Util.Util.getMinimumSequence(sequences);
        }

      
        public override String toString()
        {
            //this even useful?
            return sequences.ToString();
        }

        /**
         * Not supported.
         */
        public override void set(long value)
        {
            throw new NotImplementedException();
        }

        /**
         * Not supported.
         */
      
        public override Boolean compareAndSet(long expectedValue, long newValue)
        {
            throw new NotImplementedException();
        }

        /**
         * Not supported.
         */
        public override long incrementAndGet()
        {
            throw new NotImplementedException();
        }

        /**
         * Not supported.
         */
      
        public override long addAndGet(long increment)
        {
            throw new NotImplementedException();
        }
    }
}
