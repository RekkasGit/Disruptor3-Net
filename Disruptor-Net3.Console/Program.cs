using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor_Net3;
using System.Threading;

namespace Disruptor_Net3.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Press enter to start test");
            System.Console.ReadLine();
            
            TestSingleThreading();
            
            FizzBuzz1P3C();
            
            TestMultiIncrementSequence();
            
            TestMultiThreading();
            
            System.Console.ReadLine();
        }
        public static void FizzBuzz1P3C()
        {
            System.Console.WriteLine("Starting FizzBuzz1P3C Test");
          
            Disruptor_Net3.Tests.DiamondPath1P3C test = new Tests.DiamondPath1P3C();
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
        
            test.FizzBuzz1P3CDisruptorPerfTest();
            stopwatch.Stop();
            System.Console.WriteLine("Total time for DiamondPath1P3C:" + stopwatch.Elapsed.TotalMilliseconds);
        }
        public static void TestMultiIncrementSequence()
        {
            System.Console.WriteLine("Starting TestMultiIncrementSequence Test");
            Int32 numberOfThreads = 3;
            Int32 totalNumber = 100000000;
            Int32 numberPerThread = totalNumber / numberOfThreads;
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
            dsl.Disruptor<TestEvent> disruptor = new dsl.Disruptor<TestEvent>(new TestEventFactory(), 1024, dsl.ProducerType.MULTI, new Disruptor_Net3.WaitStrategies.BlockingWaitStrategy());
            TestConsumers.TestSequence handler = new TestConsumers.TestSequence("TestMultiIncrementSequence", resetEvent);
            handler.totalExpected = numberPerThread * numberOfThreads;
            disruptor.handleEventsWith(handler);
            disruptor.start();
          
            RingBuffer<TestEvent> ringBuffer = disruptor.getRingBuffer();
            for (Int32 i = 0; i < numberOfThreads; i++)
            {
                Task.Factory.StartNew(() =>
                {

                    Int64 currentCounter = 1;

                    while (currentCounter <= numberPerThread)
                    {
                        long sequence = ringBuffer.next();
                        TestEvent tEvent = ringBuffer.get(sequence);
                        tEvent.value = i;
                        ringBuffer.publish(sequence);

                        currentCounter++;
                    }
                });

            }
            resetEvent.Wait();
            disruptor.shutdown();
        }
        //about 16-18 million per second i72600k
        public static void TestMultiThreading()
        {
            System.Console.WriteLine("Starting TestMultiThreading Test");
            Int32 numberOfThreads = 3;
            Int32 totalNumber = 100000000;
            Int32 numberPerThread = totalNumber / numberOfThreads;
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
        
            dsl.Disruptor<TestEvent> disruptor = new dsl.Disruptor<TestEvent>(new TestEventFactory(), 1024, dsl.ProducerType.MULTI, new Disruptor_Net3.WaitStrategies.BusySpinWaitStrategy());
            TestConsumer handler = new TestConsumer("TestMultiThreading",resetEvent);
            handler.totalExpected = numberPerThread * numberOfThreads;
            disruptor.handleEventsWith(handler);
            disruptor.start();

            RingBuffer<TestEvent> ringBuffer = disruptor.getRingBuffer();
            for(Int32 i = 0;i<numberOfThreads;i++)
            {
                Task.Factory.StartNew(() => {

                    Int64 currentCounter = 1;

                    while(currentCounter<=numberPerThread)
                    {
                        long sequence = ringBuffer.next();
                        TestEvent tEvent = ringBuffer.get(sequence);
                        tEvent.value = i;
                        ringBuffer.publish(sequence);

                        currentCounter++;
                    }
                });

            }
            resetEvent.Wait();
            disruptor.shutdown();

        }
        //about 94,795,715 ops per second on a i72600k
        public static void TestSingleThreading()
        {
            Int32 totalNumber = 1000000000;
            System.Console.WriteLine("Starting TestSingleThreading Test");
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
        
            dsl.Disruptor<TestEvent> disruptor = new dsl.Disruptor<TestEvent>(new TestEventFactory(), 1024, dsl.ProducerType.SINGLE, new Disruptor_Net3.WaitStrategies.BusySpinWaitStrategy());
            TestConsumer handler = new TestConsumer("TestSingleThreading", resetEvent);
            handler.totalExpected = totalNumber;
            disruptor.handleEventsWith(handler);
            disruptor.start();

            RingBuffer<TestEvent> ringBuffer = disruptor.getRingBuffer();
           
            
            Int64 currentCounter = 0;

            while (currentCounter <totalNumber)
            {
                long sequence = ringBuffer.next();
                TestEvent tEvent = ringBuffer.get(sequence);
                tEvent.value = 1;
                ringBuffer.publish(sequence);

                currentCounter++;
            }
            resetEvent.Wait();
            disruptor.shutdown();
            

        }

        
    }
}
