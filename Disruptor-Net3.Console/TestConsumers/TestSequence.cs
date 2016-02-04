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

        string _name;
        public TestMultiThreadedSequence(string name)
        {
            _name = name;
        }
        public void onEvent(TestEvent eventToUse, long sequence, bool endOfBatch)
        {
            
        }
    }
}
