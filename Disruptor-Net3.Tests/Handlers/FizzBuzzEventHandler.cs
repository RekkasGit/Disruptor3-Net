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
        private Int64 _fizzBuzzCounter;

        public long FizzBuzzCounter
        {
            get { return _fizzBuzzCounter; }
        }
        public FizzBuzzEventHandler(FizzBuzzStep fizzBuzzStep, long iterations, ManualResetEvent mru)
        {
            _fizzBuzzStep = fizzBuzzStep;
            _iterations = iterations-1;
            _mru = mru;
            _fizzBuzzCounter = 0;
        }

        public void onEvent(FizzBuzzEvent data, long sequence, bool endOfBatch)
        {
            switch (_fizzBuzzStep)
            {
                case FizzBuzzStep.Fizz:
                   
                    data.Fizz = data.Value % 3;
                    break;
                case FizzBuzzStep.Buzz:
                         
                    data.Buzz = data.Value % 5;
                    break;

                case FizzBuzzStep.FizzBuzz:
                    if (data.Fizz==0 && data.Buzz==0)
                    {
                        _fizzBuzzCounter++;
                    }
                    break;
            }
            if (sequence == _iterations)
            {
                _mru.Set();
            }
        }
    }
}
