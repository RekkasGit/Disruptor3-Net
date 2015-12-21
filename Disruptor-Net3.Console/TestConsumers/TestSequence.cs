using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disruptor_Net3.Console.TestConsumers
{
    public class TestSequence : Disruptor_Net3.Interfaces.IEventHandler<TestEvent>
    {
        Int64 value = 0;
        System.Diagnostics.Stopwatch stopwatch = null;
        Int64 counter = 0;
        public Int64 totalExpected = 0;
        PaddedLong previousSequence = new PaddedLong();
        string _name;
        ManualResetEventSlim _resetEvent;
        public TestSequence(string name, ManualResetEventSlim resetEvent)
        {
            previousSequence.value = -1;
            _name = name;
            _resetEvent = resetEvent;
        }
        public void onEvent(TestEvent eventToUse, long sequence, bool endOfBatch)
        {
            if((sequence-1)==previousSequence.value)
            {
                previousSequence.value = sequence;
            }
            else
            {
                throw new ApplicationException("Sequence is messed up!");
            }
            counter++;
            if (stopwatch == null)
            {
                stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
            }
            if (counter == totalExpected)
            {
                stopwatch.Stop();
                System.Console.WriteLine("[" + _name + "] Consumer is done processing Events! Total Time in milliseconds:" + stopwatch.Elapsed.TotalMilliseconds + " " + String.Format("{0:###,###,###,###}op/sec", (totalExpected / stopwatch.Elapsed.TotalSeconds)));
                _resetEvent.Set();

            }
        }
    }
}
