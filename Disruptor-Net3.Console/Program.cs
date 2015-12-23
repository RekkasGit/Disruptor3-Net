using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor_Net3;
using System.Threading;
using Disruptor_Net3.Console.Events;
using Disruptor_Net3.Console.Factories;

namespace Disruptor_Net3.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Press enter to start test");
            System.Console.ReadLine();
            
            TestMultiIncrementSequence();
            TestMultiThreading();
            TestMulti3P1C();
            TestSingleThreading();
            FizzBuzz1P3C();
            System.Console.ReadLine();
        }
        public static void FizzBuzz1P3C()
        {
            System.Console.WriteLine("Starting FizzBuzz1P3C Test");
          
            Disruptor_Net3.Tests.FizzBuzz1P3C test = new Tests.FizzBuzz1P3C();
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
        
            test.FizzBuzz1P3CDisruptorPerfTest();
            stopwatch.Stop();
            System.Console.WriteLine("Total time for FizzBuzz1P3C:" + stopwatch.Elapsed.TotalMilliseconds);
        }
        public static void TestMulti3P1C()
        {
            System.Console.WriteLine("Starting TestMulti3P1C Test");
            Int32 numberOfThreads = 3;
            Int32 totalNumber = 100000000;
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
            dsl.Disruptor<Test3P1CEvent> disruptor = new dsl.Disruptor<Test3P1CEvent>(new Test3P1CEventFactory(), 1024, dsl.ProducerType.MULTI, new Disruptor_Net3.WaitStrategies.BlockingWaitStrategy());
            TestXP1CConsumer handler = new TestXP1CConsumer("TestMulti3P1C", resetEvent,numberOfThreads);
            handler.totalExpected = totalNumber;
            disruptor.handleEventsWith(handler);
            disruptor.start();

            RingBuffer<Test3P1CEvent> ringBuffer = disruptor.getRingBuffer();
            for (Int32 i = 0; i < numberOfThreads; i++)
            {
                Int32 tempthreadId = i;
                Task.Factory.StartNew(() =>
                {
                    Int32 currentThreadId = tempthreadId;
                    PaddedLong currentCounter = new PaddedLong();
                    currentCounter.value= 1;

                    while (currentCounter.value <= totalNumber)
                    {
                        long sequence = ringBuffer.next();
                        Test3P1CEvent tEvent = ringBuffer.get(sequence);
                        tEvent.currentCounter = currentCounter.value;
                        tEvent.threadID = currentThreadId;
                        ringBuffer.publish(sequence);
                        currentCounter.value++;
                    }
                });

            }
            resetEvent.Wait();
            disruptor.shutdown();


        }
        public static void TestMultiIncrementSequence()
        {
            System.Console.WriteLine("Starting TestMultiIncrementSequence Test");
            Int32 numberOfThreads = 3;
            Int32 totalNumber = 300000000;
            Int32 numberPerThread = totalNumber / numberOfThreads;
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
            dsl.Disruptor<TestEvent> disruptor = new dsl.Disruptor<TestEvent>(new TestEventFactory(), 1024, dsl.ProducerType.MULTI, new Disruptor_Net3.WaitStrategies.BlockingWaitStrategy());
            TestConsumers.TestMultiThreadedSequence handler = new TestConsumers.TestMultiThreadedSequence("TestMultiIncrementSequence", resetEvent,numberOfThreads);
            handler.totalExpected = totalNumber;
            disruptor.handleEventsWith(handler);
            disruptor.start();
          
            RingBuffer<TestEvent> ringBuffer = disruptor.getRingBuffer();
            for (Int32 i = 0; i < numberOfThreads; i++)
            {
                Task.Factory.StartNew(() =>
                {

                    PaddedLong currentCounter = new PaddedLong();
                    currentCounter.value = 1;

                    while (currentCounter.value <= numberPerThread)
                    {
                        long sequence = ringBuffer.next();
                        TestEvent tEvent = ringBuffer.get(sequence);
                        ringBuffer.publish(sequence);

                        currentCounter.value++;
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

                    PaddedLong currentCounter = new PaddedLong();
                    currentCounter.value = 1;

                    while(currentCounter.value<=numberPerThread)
                    {
                        long sequence = ringBuffer.next();
                        TestEvent tEvent = ringBuffer.get(sequence);
                        tEvent.value = i;
                        ringBuffer.publish(sequence);

                        currentCounter.value++;
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
