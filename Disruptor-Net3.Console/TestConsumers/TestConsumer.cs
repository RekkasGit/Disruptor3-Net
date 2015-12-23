using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disruptor_Net3.Console
{
    public class TestConsumer : Disruptor_Net3.Interfaces.IEventHandler<TestEvent>
    {
        Int64 value = 0;
        System.Diagnostics.Stopwatch stopwatch = null;
        PaddedLong counter = new PaddedLong();
        public Int64 totalExpected = 0;
        string _name;
        ManualResetEventSlim _resetEvent = null;
        public TestConsumer (string name,ManualResetEventSlim resetEvent= null)
        {
            _name = name;
            _resetEvent = resetEvent;
        }

        public void onEvent(TestEvent eventToUse, long sequence, bool endOfBatch)
        {
            counter.value++;
            if(stopwatch==null)
            {
                stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
            }
            if (counter.value == totalExpected)
            {
                stopwatch.Stop();
               
                System.Console.WriteLine("["+_name+ "] Consumer is done processing Events! Total Time in milliseconds:"+stopwatch.Elapsed.TotalMilliseconds +" " + String.Format("{0:###,###,###,###}op/sec", (totalExpected / stopwatch.Elapsed.TotalSeconds)));
                if(_resetEvent!=null)
                {
                    _resetEvent.Set();
                }
            }
        }
    }
}
