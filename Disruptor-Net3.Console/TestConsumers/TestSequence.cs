using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disruptor3_Net.Console.TestConsumers
{
    public class TestMultiThreadedSequence : Disruptor3_Net.Interfaces.IEventHandler<TestEvent>
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
