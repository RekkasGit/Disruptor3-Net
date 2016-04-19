using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disruptor3_Net.Console
{
    public class TestConsumer : Disruptor3_Net.Interfaces.IEventHandler<TestEvent>
    {
        
        //PaddedLong counter = new PaddedLong();

        string _name;
        Int64 tempValue;
        public TestConsumer (string name)
        {
            _name = name;
        }

        public void onEvent(TestEvent eventToUse, long sequence, bool endOfBatch)
        {
           // tempValue = 1;
        }
    }
}
