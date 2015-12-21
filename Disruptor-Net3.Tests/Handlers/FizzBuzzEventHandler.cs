using Disruptor_Net3.Interfaces;
using Disruptor_Net3.Tests.Enums;
using Disruptor_Net3.Tests.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disruptor_Net3.Tests.Handlers
{
    public class FizzBuzzEventHandler : IEventHandler<FizzBuzzEvent>
    {
        private readonly FizzBuzzStep _fizzBuzzStep;
        private readonly long _iterations;
        private readonly ManualResetEvent _mru;
        private PaddedLong _fizzBuzzCounter;

        public long FizzBuzzCounter
        {
            get { return _fizzBuzzCounter.value; }
        }

        public FizzBuzzEventHandler(FizzBuzzStep fizzBuzzStep, long iterations, ManualResetEvent mru)
        {
            _fizzBuzzStep = fizzBuzzStep;
            _iterations = iterations;
            _mru = mru;
            _fizzBuzzCounter.value = 0;
        }

        public void onEvent(FizzBuzzEvent data, long sequence, bool endOfBatch)
        {
            switch (_fizzBuzzStep)
            {
                case FizzBuzzStep.Fizz:
                    data.Fizz = (data.Value % 3) == 0;
                    break;
                case FizzBuzzStep.Buzz:
                    data.Buzz = (data.Value % 5) == 0;
                    break;

                case FizzBuzzStep.FizzBuzz:
                    if (data.Fizz && data.Buzz)
                    {
                        _fizzBuzzCounter.value++;
                    }
                    break;
            }
            if (sequence == _iterations - 1)
            {
                _mru.Set();
            }
        }
    }
}
