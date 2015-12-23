using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor_Net3;

namespace Disruptor_Net3.Console
{
    public class TestEventFactory: Disruptor_Net3.Interfaces.IEventFactory<TestEvent>
    {
        public TestEvent newInstance()
        {
            return new TestEvent();
        }
    }
}
