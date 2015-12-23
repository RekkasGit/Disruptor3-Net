using Disruptor_Net3.Console.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor_Net3.Console.Factories
{
    public class Test3P1CEventFactory : Disruptor_Net3.Interfaces.IEventFactory<Test3P1CEvent>
    {
        public Test3P1CEvent newInstance()
        {
            return new Test3P1CEvent();
        }
    }
}
