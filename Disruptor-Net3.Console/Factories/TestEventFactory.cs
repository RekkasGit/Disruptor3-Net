using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor3_Net;

namespace Disruptor3_Net.Console
{
    public class TestEventFactory: Disruptor3_Net.Interfaces.IEventFactory<TestEvent>
    {
        public TestEvent newInstance()
        {
            return new TestEvent();
        }
    }
}
