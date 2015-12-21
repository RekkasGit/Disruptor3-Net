using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor_Net3.Tests.Events
{
    public sealed class FizzBuzzEvent
    {
        public long Value { get; set; }
        public bool Fizz { get; set; }
        public bool Buzz { get; set; }

        public void Reset()
        {
            Value = 0L;
            Fizz = false;
            Buzz = false;
        }
    }
}
