using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Disruptor_Net3.Tests.Handlers;
using Disruptor_Net3.Tests.Events;
using System.Threading;
using Disruptor_Net3.dsl;
using Disruptor_Net3.Tests.Factories;
using Disruptor_Net3.Tests.Enums;
using System.Diagnostics;

namespace Disruptor_Net3.Tests
{
    [TestClass]
    public class DiamondPath1P3C : AbstractFizzBuzz1P3CPerfTest
    {
        private RingBuffer<FizzBuzzEvent> _ringBuffer;
        private FizzBuzzEventHandler _fizzEventHandler;
        private FizzBuzzEventHandler _buzzEventHandler;
        private FizzBuzzEventHandler _fizzBuzzEventHandler;
        private ManualResetEvent _mru;
        private Disruptor<FizzBuzzEvent> _disruptor;
        public DiamondPath1P3C():base(1000 * Million)
        {
           Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        }
        [TestMethod]
        public void FizzBuzz1P3CDisruptorPerfTest()
        {
            _disruptor = new Disruptor<FizzBuzzEvent>( new FizzBuzzEventFactory(), 1024, ProducerType.SINGLE,new WaitStrategies.BusySpinWaitStrategy());

            _mru = new ManualResetEvent(false);
            _fizzEventHandler = new FizzBuzzEventHandler(FizzBuzzStep.Fizz, Iterations, _mru);
            _buzzEventHandler = new FizzBuzzEventHandler(FizzBuzzStep.Buzz, Iterations, _mru);
            _fizzBuzzEventHandler = new FizzBuzzEventHandler(FizzBuzzStep.FizzBuzz, Iterations, _mru);
            _disruptor.handleEventsWith(_fizzEventHandler, _buzzEventHandler).then(_fizzBuzzEventHandler);
    
            _ringBuffer = _disruptor.getRingBuffer();
            _disruptor.start();

            var sw = Stopwatch.StartNew();

            for (long i = 0; i < Iterations; i++)
            {
                var sequence = _ringBuffer.next();
                _ringBuffer.get(sequence).Value = i;
                _ringBuffer.publish(sequence);
            }

            _mru.WaitOne();

            var opsPerSecond = (Iterations * 1000L) / sw.ElapsedMilliseconds;

            _disruptor.shutdown();

            //Assert.AreEqual(ExpectedResult, _fizzBuzzEventHandler.FizzBuzzCounter);
            Trace.WriteLine("Expected:" + ExpectedResult + " Recieved:" + _fizzBuzzEventHandler.FizzBuzzCounter);
            Trace.WriteLine(String.Format("{0}: {1:###,###,###,###}op/sec", GetType().Name, opsPerSecond));
             
        }
      
    }
    public abstract class AbstractFizzBuzz1P3CPerfTest : PerfTest
    {
        protected const int Size = 1024 * 8;
        private long _expectedResult;

        protected AbstractFizzBuzz1P3CPerfTest(int iterations)
            : base(iterations)
        {
        }

        protected long ExpectedResult
        {
            get
            {
                if (_expectedResult == 0)
                {
                    for (long i = 0; i < Iterations; i++)
                    {
                        var fizz = (i % 3L) == 0;
                        var buzz = (i % 5L) == 0;

                        if (fizz && buzz)
                        {
                            ++_expectedResult;
                        }
                    }
                }
                return _expectedResult;
            }
        }

        public override int MinimumCoresRequired
        {
            get { return 4; }
        }
    }

}
