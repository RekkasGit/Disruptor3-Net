using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor_Net3;
using System.Threading;
using Disruptor_Net3.Console.Events;
using Disruptor_Net3.Console.Factories;
using System.Diagnostics;

namespace Disruptor_Net3.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;

            Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Out));

            System.Console.WriteLine("Press enter to start test");
            System.Console.ReadLine();
            TestMultiIncrementSequence();
            TestMultiThreading();
            TestMulti3P1C();
            TestSingleThreading();
            FizzBuzz1P3CBlockingCollection();
            FizzBuzz1P3C();
            
            System.Console.ReadLine();
        }
        public static void FizzBuzz1P3CBlockingCollection()
        {
            System.Console.WriteLine("============================");
            System.Console.WriteLine("Starting FizzBuzz1P3CBlockingCollection Test");

            Disruptor_Net3.Tests.FizzBuzz1P3CBlockingCollectionPerfTest test = new Tests.FizzBuzz1P3CBlockingCollectionPerfTest();
            test.RunPass();
            System.Console.WriteLine("============================");
         
        }
        public static void FizzBuzz1P3C()
        {
            System.Console.WriteLine("============================");
            System.Console.WriteLine("Starting FizzBuzz1P3C Test");
          
            Disruptor_Net3.Tests.FizzBuzz1P3C test = new Tests.FizzBuzz1P3C();
         
            test.FizzBuzz1P3CDisruptorPerfTest();
            System.Console.WriteLine("============================");
        
        }
        public static void TestMulti3P1C()
        {
            Int32 numberOfThreads = 3;
            Int32 totalNumber = 10000000;
            System.Console.WriteLine("============================");
            System.Console.WriteLine("Starting TestMulti3P1C Test with "+String.Format("{0:###,###,###,###} entries",totalNumber));
           
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
            System.Console.WriteLine("============================");
       

        }
        public static void TestMultiIncrementSequence()
        {
            System.Console.WriteLine("============================");
            Int32 numberOfThreads = 3;
            Int32 totalNumber = 30000000;
            Int32 numberPerThread = totalNumber / numberOfThreads;

            System.Console.WriteLine("Starting TestMultiIncrementSequence Test with " + String.Format("{0:###,###,###,###} entries", totalNumber) + " and with "+numberOfThreads + " threads");
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
            dsl.Disruptor<TestEvent> disruptor = new dsl.Disruptor<TestEvent>(new TestEventFactory(), 1024, dsl.ProducerType.MULTI, new Disruptor_Net3.WaitStrategies.BusySpinWaitStrategy());
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
            System.Console.WriteLine("============================");
        
        }
        //about 16-18 million per second i72600k
        public static void TestMultiThreading()
        {
            System.Console.WriteLine("============================");
 
            Int32 numberOfThreads = 3;
            Int32 totalNumber = 10000000;
            Int32 numberPerThread = totalNumber / numberOfThreads;
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
            System.Console.WriteLine("Starting TestMultiThreading Test with " + String.Format("{0:###,###,###,###} entries", totalNumber) + " and with " + numberOfThreads + " threads");
         
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
            System.Console.WriteLine("============================");
 
        }
        //about 94,795,715 ops per second on a i72600k
        public static void TestSingleThreading()
        {
            System.Console.WriteLine("============================");
 
            Int32 totalNumber = 100000000;
            System.Console.WriteLine("Starting TestSingleThreading Test with " + String.Format("{0:###,###,###,###} entries", totalNumber) + " and a single thread");
         
         
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
            System.Console.WriteLine("============================");
 

        }

        
    }
}
