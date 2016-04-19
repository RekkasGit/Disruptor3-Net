using Disruptor3_Net.Tests.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.Tests.Factories
{
    public class FizzBuzzEventFactory : Disruptor3_Net.Interfaces.IEventFactory<FizzBuzzEvent>
    {
        public FizzBuzzEvent newInstance()
        {
            return new FizzBuzzEvent();
        }
    }
}
