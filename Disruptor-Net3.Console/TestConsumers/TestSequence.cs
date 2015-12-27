using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disruptor_Net3.Console.TestConsumers
{
    public class TestMultiThreadedSequence : Disruptor_Net3.Interfaces.IEventHandler<TestEvent>
    {
       System.Diagnostics.Stopwatch stopwatch = null;

       PaddedLong previousSequence = new PaddedLong();
        private Int64 _totalExpected = 0;
        public Int64 totalExpected{
            get{  return _totalExpected; }
            set { _totalExpected = value - 1; } 
        }
        string _name;
        ManualResetEventSlim _resetEvent;
        Int32 _numberOfTherads;
        public TestMultiThreadedSequence(string name, ManualResetEventSlim resetEvent, Int32 numberOfThreads)
        {
            _numberOfTherads = numberOfThreads;
            previousSequence.value = -1;
            _name = name;
            _resetEvent = resetEvent;
        }
        public void onEvent(TestEvent eventToUse, long sequence, bool endOfBatch)
        {
            if (stopwatch == null)
            {
                stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
            }

            if(sequence==previousSequence.value+1)
            {
                previousSequence.value = sequence;
            }
            else
            {
                throw new ApplicationException("Sequence is messed up!");
            }
           
            if (sequence == _totalExpected)
            {
                stopwatch.Stop();
                System.Console.WriteLine("[" + _name + "] Consumer is done processing Events! Total Time in milliseconds:" + stopwatch.Elapsed.TotalMilliseconds + " " + String.Format("{0:###,###,###,###}op/sec", (_totalExpected / stopwatch.Elapsed.TotalSeconds)));
                _resetEvent.Set();

            }
        }
    }
}
