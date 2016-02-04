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
using Disruptor_Net3.Interfaces;

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

           // TestBatch();
            //TestMultiIncrementSequence();
            TestMultiThreading();
            //TestMulti3P1C();
            //TestSingleThreading();
           // FizzBuzz1P3CBlockingCollection();
            for (Int32 i = 0; i < 3; i++)
            {
                TestSingleThreading();
                System.Threading.Thread.Sleep(5000);
            }
            //for (Int32 i = 0; i < 3; i++)
            //{
            //    FizzBuzz1P3C();
            //    System.Threading.Thread.Sleep(5000);
            //}
         
            
            System.Console.ReadLine();
        }

        public static void TestBatch()
        {
            System.Console.WriteLine("============================");

            Int32 totalNumber = 1000000000;
            System.Console.WriteLine("Starting TestBatch Test with " + String.Format("{0:###,###,###,###} entries", totalNumber) + " and a single thread");
            
            TestConsumer handler = new TestConsumer("TestSingleThreading");
            RingBuffer<TestEvent> ringBuffer = RingBuffer<TestEvent>.createSingleProducer(new TestEventFactory(), 1024, new WaitStrategies.BusySpinWaitStrategy());

            ISequenceBarrier sBarrier = ringBuffer.newBarrier();


            BatchEventProcessor<TestEvent> batchEventProcessor = new BatchEventProcessor<TestEvent>(ringBuffer, sBarrier, handler);
            ringBuffer.addGatingSequences(batchEventProcessor.getSequence());

            long expectedCount = batchEventProcessor.getSequence().get() + totalNumber;
      
            Task.Factory.StartNew(() => batchEventProcessor.run(), TaskCreationOptions.LongRunning);
     
            Int64 currentCounter = 0;
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
      
            while (currentCounter < totalNumber)
            {
                long sequence = ringBuffer.next();
                TestEvent tEvent = ringBuffer.get(sequence);
                tEvent.value = 1;

                ringBuffer.publish(sequence);

                currentCounter++;
            }

            while(batchEventProcessor.getSequence().get()!=expectedCount)
            {
                Thread.Sleep(1);
            }

            stopwatch.Stop();
            System.Console.WriteLine("[TestBatch] Consumer is done processing Events! Total Time in milliseconds:" + stopwatch.Elapsed.TotalMilliseconds + " " + String.Format("{0:###,###,###,###}op/sec", ((totalNumber) / stopwatch.Elapsed.TotalSeconds)));
    
            batchEventProcessor.halt();
            
      
            System.Console.WriteLine("============================");
 

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
            Int32 totalNumber = 90000000;
            Int32 totalPerThread = totalNumber / numberOfThreads;
            System.Console.WriteLine("============================");
            System.Console.WriteLine("Starting TestMulti3P1C Test with "+String.Format("{0:###,###,###,###} entries",totalNumber));
           
            dsl.Disruptor<Test3P1CEvent> disruptor = new dsl.Disruptor<Test3P1CEvent>(new Test3P1CEventFactory(), 1024, dsl.ProducerType.MULTI, new Disruptor_Net3.WaitStrategies.BlockingWaitStrategy());
            TestXP1CConsumer handler = new TestXP1CConsumer("TestMulti3P1C");
             disruptor.handleEventsWith(handler);
            disruptor.start();

            RingBuffer<Test3P1CEvent> ringBuffer = disruptor.getRingBuffer();
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (Int32 i = 0; i < numberOfThreads; i++)
            {
                Int32 tempthreadId = i;
                Task.Factory.StartNew(() =>
                {
                    Int32 currentThreadId = tempthreadId;
                    PaddedLong currentCounter = new PaddedLong();
                    currentCounter.value= 1;

                    while (currentCounter.value <= totalPerThread)
                    {
                        long sequence = ringBuffer.next();
                        Test3P1CEvent tEvent = ringBuffer.get(sequence);
                        tEvent.currentCounter = currentCounter.value;
                        ringBuffer.publish(sequence);
                        currentCounter.value++;
                    }
                });

            }
            Int64 totalExpecting = totalNumber-1;
            while (ringBuffer.getCursor() != totalExpecting)
            {
                Thread.Sleep(1);
            }
            stopwatch.Stop();
            System.Console.WriteLine("[TestMulti3P1C] Consumer is done processing Events! Total Time in milliseconds:" + stopwatch.Elapsed.TotalMilliseconds + " " + String.Format("{0:###,###,###,###}op/sec", ((totalNumber) / stopwatch.Elapsed.TotalSeconds)));
    
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
           dsl.Disruptor<TestEvent> disruptor = new dsl.Disruptor<TestEvent>(new TestEventFactory(), 1024, dsl.ProducerType.MULTI, new Disruptor_Net3.WaitStrategies.BusySpinWaitStrategy());
            TestConsumers.TestMultiThreadedSequence handler = new TestConsumers.TestMultiThreadedSequence("TestMultiIncrementSequence");
            disruptor.handleEventsWith(handler);
            disruptor.start();
          
            RingBuffer<TestEvent> ringBuffer = disruptor.getRingBuffer();
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
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
            while (ringBuffer.getCursor() != (totalNumber - 1))
            {
                Thread.Sleep(1);
            }
            stopwatch.Stop();
            System.Console.WriteLine("[TestMultiIncrementSequence] Consumer is done processing Events! Total Time in milliseconds:" + stopwatch.Elapsed.TotalMilliseconds + " " + String.Format("{0:###,###,###,###}op/sec", (totalNumber / stopwatch.Elapsed.TotalSeconds)));
    

            disruptor.shutdown();
            System.Console.WriteLine("============================");
        
        }
        //about 16-18 million per second i72600k
        public static void TestMultiThreading()
        {
            System.Console.WriteLine("============================");
 
            Int32 numberOfThreads = 3;
            Int32 totalNumber = 30000000;
            Int32 numberPerThread = totalNumber / numberOfThreads;
             System.Console.WriteLine("Starting TestMultiThreading Test with " + String.Format("{0:###,###,###,###} entries", totalNumber) + " and with " + numberOfThreads + " threads");
         
            dsl.Disruptor<TestEvent> disruptor = new dsl.Disruptor<TestEvent>(new TestEventFactory(), 1024, dsl.ProducerType.MULTI, new Disruptor_Net3.WaitStrategies.BusySpinWaitStrategy());
            TestConsumer handler = new TestConsumer("TestMultiThreading");
            disruptor.handleEventsWith(handler);
            disruptor.start();
          
            RingBuffer<TestEvent> ringBuffer = disruptor.getRingBuffer();
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
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

            while (ringBuffer.getCursor() != (totalNumber - 1))
            {
                Thread.Sleep(1);
            }
            stopwatch.Stop();
            System.Console.WriteLine("[TestMultiThreading] Consumer is done processing Events! Total Time in milliseconds:" + stopwatch.Elapsed.TotalMilliseconds + " " + String.Format("{0:###,###,###,###}op/sec", (totalNumber / stopwatch.Elapsed.TotalSeconds)));
    
            disruptor.shutdown();
            System.Console.WriteLine("============================");
 
        }
        //about 94,795,715 ops per second on a i72600k
        public static void TestSingleThreading()
        {
            System.Console.WriteLine("============================");

           Int32 totalNumber = 1000000000;
            System.Console.WriteLine("Starting TestSingleThreading Test with " + String.Format("{0:###,###,###,###} entries", totalNumber) + " and a single thread");
         
         
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
        
            dsl.Disruptor<TestEvent> disruptor = new dsl.Disruptor<TestEvent>(new TestEventFactory(), 1024, dsl.ProducerType.SINGLE, new Disruptor_Net3.WaitStrategies.BusySpinWaitStrategy());
            TestConsumer handler = new TestConsumer("TestSingleThreading");
            disruptor.handleEventsWith(handler);
            disruptor.start();

            RingBuffer<TestEvent> ringBuffer = disruptor.getRingBuffer();
           
            
            Int64 currentCounter = 0;
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();

          
            stopwatch.Start();
            while (currentCounter <totalNumber)
            {
                long sequence = ringBuffer.next();
                TestEvent tEvent = ringBuffer.get(sequence);
                tEvent.value = 1;
                ringBuffer.publish(sequence);

                currentCounter++;
            }
            
            while (ringBuffer.getCursor() != (totalNumber-1))
            {
                Thread.Sleep(1);
            }
            stopwatch.Stop();
            System.Console.WriteLine("[TestSingleThreading] Consumer is done processing Events! Total Time in milliseconds:" + stopwatch.Elapsed.TotalMilliseconds + " " + String.Format("{0:###,###,###,###}op/sec", (totalNumber / stopwatch.Elapsed.TotalSeconds)));
         
            disruptor.shutdown();
            System.Console.WriteLine("============================");
 

        }

        
    }
}
