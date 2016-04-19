using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Disruptor3_Net.Tests.Handlers;
using Disruptor3_Net.Tests.Events;
using System.Threading;
using Disruptor3_Net.dsl;
using Disruptor3_Net.Tests.Factories;
using Disruptor3_Net.Tests.Enums;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Disruptor3_Net.Tests
{
    [TestClass]
    public class FizzBuzz1P3C : AbstractFizzBuzz1P3CPerfTest
    {
        private RingBuffer<FizzBuzzEvent> _ringBuffer;
        private FizzBuzzEventHandler _fizzEventHandler;
        private FizzBuzzEventHandler _buzzEventHandler;
        private FizzBuzzEventHandler _fizzBuzzEventHandler;
        private ManualResetEvent _mru;
        private Disruptor<FizzBuzzEvent> _disruptor;
        public FizzBuzz1P3C():base(300 * Million)
        {
        }
        [TestMethod]
        public void FizzBuzz1P3CDisruptorPerfTest()
        {
            _disruptor = new Disruptor<FizzBuzzEvent>( new FizzBuzzEventFactory(), 1024, ProducerType.SINGLE,new WaitStrategies.YieldingWaitStrategy());

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
                var entry = _ringBuffer.get(sequence);
                entry.Value = i;
                _ringBuffer.publish(sequence);
            }

            _mru.WaitOne();

           
            sw.Stop();
            var opsPerSecond = (Iterations * 1000L) / sw.Elapsed.TotalMilliseconds;
            Int64 expectedResult = ExpectedResult;
            _ringBuffer.getCursor();
         
            Trace.WriteLine("Expected:" + expectedResult + " Recieved:" + _fizzBuzzEventHandler.FizzBuzzCounter);
            Assert.AreEqual(expectedResult, _fizzBuzzEventHandler.FizzBuzzCounter);
            Trace.WriteLine(String.Format("{0}: {1:###,###,###,###}op/sec for {2:###,###,###,###} iterations.", GetType().Name, opsPerSecond, Iterations));
            Trace.WriteLine("Total time in MS:" + sw.Elapsed.TotalMilliseconds);
            _disruptor.shutdown();
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
    [TestClass]
    public class FizzBuzz1P3CBlockingCollectionPerfTest : AbstractFizzBuzz1P3CPerfTest
    {
        private readonly BlockingCollection<long> _fizzInputQueue = new BlockingCollection<long>(Size);
        private readonly BlockingCollection<long> _buzzInputQueue = new BlockingCollection<long>(Size);
        private readonly BlockingCollection<bool> _fizzOutputQueue = new BlockingCollection<bool>(Size);
        private readonly BlockingCollection<bool> _buzzOutputQueue = new BlockingCollection<bool>(Size);

        private readonly FizzBuzzQueueEventProcessor _fizzQueueEventProcessor;
        private readonly FizzBuzzQueueEventProcessor _buzzQueueEventProcessor;
        private readonly FizzBuzzQueueEventProcessor _fizzBuzzQueueEventProcessor;
      
        public FizzBuzz1P3CBlockingCollectionPerfTest()
            : base(20 * Million)
        {
       
            _fizzQueueEventProcessor = new FizzBuzzQueueEventProcessor(FizzBuzzStep.Fizz, _fizzInputQueue, _buzzInputQueue, _fizzOutputQueue, _buzzOutputQueue, Iterations);
            _buzzQueueEventProcessor = new FizzBuzzQueueEventProcessor(FizzBuzzStep.Buzz, _fizzInputQueue, _buzzInputQueue, _fizzOutputQueue, _buzzOutputQueue, Iterations);
            _fizzBuzzQueueEventProcessor = new FizzBuzzQueueEventProcessor(FizzBuzzStep.FizzBuzz, _fizzInputQueue, _buzzInputQueue, _fizzOutputQueue, _buzzOutputQueue, Iterations);
        }
          [TestMethod]
        public  long RunPass()
        {
            _fizzBuzzQueueEventProcessor.Reset();

            (new Thread(_fizzQueueEventProcessor.Run) { Name = "Fizz" }).Start();
            (new Thread(_buzzQueueEventProcessor.Run) { Name = "Buzz" }).Start();
            (new Thread(_fizzBuzzQueueEventProcessor.Run) { Name = "FizzBuzz" }).Start();

            var sw = Stopwatch.StartNew();

            for (long i = 0; i < Iterations; i++)
            {
                _fizzInputQueue.Add(i);
                _buzzInputQueue.Add(i);
            }

            while (!_fizzBuzzQueueEventProcessor.Done)
            {
                Thread.Yield();
            }
            sw.Stop();
            var opsPerSecond = (Iterations * 1000L) / sw.ElapsedMilliseconds;
            Int64 expectedResult = ExpectedResult;

            Assert.AreEqual(expectedResult, _fizzBuzzQueueEventProcessor.FizzBuzzCounter);
            Trace.WriteLine("Expected:" + expectedResult + " Recieved:" + _fizzBuzzQueueEventProcessor.FizzBuzzCounter);
            Trace.WriteLine(String.Format("{0}: {1:###,###,###,###}op/sec for {2:###,###,###,###} iterations.", GetType().Name, opsPerSecond, Iterations));
            Trace.WriteLine("Total time in MS:" + sw.Elapsed.TotalMilliseconds);
            return opsPerSecond;
        }

        
    }
    public class FizzBuzzQueueEventProcessor
    {
        private readonly FizzBuzzStep _fizzBuzzStep;
        private readonly BlockingCollection<long> _fizzInputQueue;
        private readonly BlockingCollection<long> _buzzInputQueue;
        private readonly BlockingCollection<bool> _fizzOutputQueue;
        private readonly BlockingCollection<bool> _buzzOutputQueue;
        private readonly long _iterations;
        private volatile bool _done;
        private volatile bool _running;
        private long _fizzBuzzCounter;

        public FizzBuzzQueueEventProcessor(FizzBuzzStep fizzBuzzStep,
                                 BlockingCollection<long> fizzInputQueue,
                                 BlockingCollection<long> buzzInputQueue,
                                 BlockingCollection<bool> fizzOutputQueue,
                                 BlockingCollection<bool> buzzOutputQueue,
                                 long iterations)
        {
            _fizzBuzzStep = fizzBuzzStep;

            _fizzInputQueue = fizzInputQueue;
            _buzzInputQueue = buzzInputQueue;
            _fizzOutputQueue = fizzOutputQueue;
            _buzzOutputQueue = buzzOutputQueue;
            _iterations = iterations;
            _done = false;
        }

        public bool Done
        {
            get { return _done; }
        }

        public long FizzBuzzCounter
        {
            get { return _fizzBuzzCounter; }
        }

        public void Reset()
        {
            _done = false;
            _fizzBuzzCounter = 0;
        }

        public void Halt()
        {
            _running = false;
        }

        public void Run()
        {
            _running = true;

            for (var i = 0; i < _iterations; i++)
            {
                try
                {
                    switch (_fizzBuzzStep)
                    {
                        case FizzBuzzStep.Fizz:
                            {
                                var value = _fizzInputQueue.Take();
                                _fizzOutputQueue.Add((value % 3) == 0);
                                break;
                            }

                        case FizzBuzzStep.Buzz:
                            {
                                var value = _buzzInputQueue.Take();
                                _buzzOutputQueue.Add((value % 5) == 0);
                                break;
                            }

                        case FizzBuzzStep.FizzBuzz:
                            {
                                var fizz = _fizzOutputQueue.Take();
                                var buzz = _buzzOutputQueue.Take();
                                if (fizz && buzz)
                                {
                                    ++_fizzBuzzCounter;
                                }
                                break;
                            }
                    }


                }
                catch (Exception)
                {
                    break;
                }
            }
            _done = true;
        }
    }

}
