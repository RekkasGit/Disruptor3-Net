using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor3_Net.Tests.Events
{
    public sealed class FizzBuzzEvent
    {
        public long Value;
        public Int64 Fizz;
        public Int64 Buzz;
        public void Reset()
        {
            Value = 0L;
            Fizz = 0;
            Buzz = 0;
        }
    }
}
