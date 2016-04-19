using Disruptor3_Net.Console.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.Console.Factories
{
    public class Test3P1CEventFactory : Disruptor3_Net.Interfaces.IEventFactory<Test3P1CEvent>
    {
        public Test3P1CEvent newInstance()
        {
            return new Test3P1CEvent();
        }
    }
}
