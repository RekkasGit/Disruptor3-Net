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
        
        //PaddedLong counter = new PaddedLong();

        string _name;
        Int64 tempValue;
        public TestConsumer (string name)
        {
            _name = name;
        }

        public void onEvent(TestEvent eventToUse, long sequence, bool endOfBatch)
        {
            tempValue = 1;
        }
    }
}
