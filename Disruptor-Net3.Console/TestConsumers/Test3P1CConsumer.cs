using Disruptor_Net3.Console.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disruptor_Net3.Console
{
    class TestXP1CConsumer : Disruptor_Net3.Interfaces.IEventHandler<Test3P1CEvent>
    {

        string _name;
        public TestXP1CConsumer(string name)
        {
            _name = name;
        }

        public void onEvent(Test3P1CEvent eventToUse, long sequence, bool endOfBatch)
        {
            
        }
    }
}
