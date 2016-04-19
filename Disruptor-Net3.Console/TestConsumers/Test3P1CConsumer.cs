using Disruptor3_Net.Console.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disruptor3_Net.Console
{
    class TestXP1CConsumer : Disruptor3_Net.Interfaces.IEventHandler<Test3P1CEvent>
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
