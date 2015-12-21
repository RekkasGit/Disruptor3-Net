using Disruptor_Net3.Tests.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor_Net3.Tests.Factories
{
    public class FizzBuzzEventFactory : Disruptor_Net3.Interfaces.IEventFactory<FizzBuzzEvent>
    {
        public FizzBuzzEvent newInstance()
        {
            return new FizzBuzzEvent();
        }
    }
}
