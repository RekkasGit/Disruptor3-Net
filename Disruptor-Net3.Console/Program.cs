using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor3_Net;
using System.Threading;
using Disruptor3_Net.Console.Events;
using Disruptor3_Net.Console.Factories;
using System.Diagnostics;
using Disruptor3_Net.Interfaces;
using Disruptor3_Net.dsl;

namespace Disruptor3_Net.Console
{
    class Program
    {
        static void Main(string[] args)
        {

            //*NOTE* be sure to run in 64bit mode/Release (turn off prefer 32bit)

            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;

            Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Out));

            System.Console.WriteLine("Press enter to start test");
            System.Console.ReadLine();

            for(Int32 i = 0;i<20;i++)
            {
                //Test1P1CBatch();
                //Test3P1CBatch();
                Test1P1C();
                //TestMulti3P1C();
                System.Threading.Thread.Sleep(3000);
               
            }

  

            System.Console.ReadLine();
        }
        public static void Test1P1CBatch()
        {

            System.Console.WriteLine("============================");

            //Int32 totalNumber = 1000000000;
            Int32 totalNumber = 1000000000;
            System.Console.WriteLine("Starting Test1P1CBatch **Batch** Test with " + String.Format("{0:###,###,###,###} entries", totalNumber) + " and a single thread");


            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

            dsl.Disruptor<TestEvent> disruptor = new dsl.Disruptor<TestEvent>(new TestEventFactory(), 1024 * 2, dsl.ProducerType.SINGLE, new Disruptor3_Net.WaitStrategies.BusySpinWaitStrategy());
            TestConsumer handler = new TestConsumer("TestSingleThreading");
            disruptor.handleEventsWith(handler);
            disruptor.start();

            RingBuffer<TestEvent> ringBuffer = disruptor.getRingBuffer();


            Int64 currentCounter = 0;
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();

            Int32 batchSize = 100;
            stopwatch.Start();
            while (currentCounter < totalNumber)
            {
                if (currentCounter + batchSize < totalNumber)
                {
                    var sequence = ringBuffer.next(batchSize);
                    var low = sequence - batchSize + 1;
                    Int64 originalLow = low;
                    for (; low <= sequence; low++)
                    {
                        var entry = ringBuffer.get(low);
                        entry.value = 1;
                      
                    }
                    ringBuffer.publish(originalLow,sequence);
                    currentCounter += batchSize - 1;

                }
                else
                {

                    long sequence = ringBuffer.next();
                    TestEvent tEvent = ringBuffer.get(sequence);
                    tEvent.value = 1;
                    ringBuffer.publish(sequence);


                }
                currentCounter++;
            }

            while (ringBuffer.getCursor() != (totalNumber - 1))
            {
                Thread.Sleep(1);
            }
            stopwatch.Stop();
            System.Console.WriteLine("[Test1P1CBatch] Consumer is done processing Events! Total Time in milliseconds:" + stopwatch.Elapsed.TotalMilliseconds + " " + String.Format("{0:###,###,###,###}op/sec", (totalNumber / stopwatch.Elapsed.TotalSeconds)));

            disruptor.shutdown();
            System.Console.WriteLine("============================");


        }
        public static void Test3P1CBatch()
        {
            RingBuffer<TestEvent> ringBuffer = RingBuffer<TestEvent>.createMultiProducer(new TestEventFactory(), 1024 * 64, new WaitStrategies.BusySpinWaitStrategy());
            Int32 batchSize = 10;
           
            Int32 numberOfThreads = 3;
            Int64 iterations = 1000L * 1000L *200L;
            Int64 expectedCount = iterations * numberOfThreads;
            
            System.Console.WriteLine("Starting Test3P1CBatch **Batch** Test with " + String.Format("{0:###,###,###,###} entries", expectedCount));

            TestConsumer handler = new TestConsumer("TestBatchMultiProducer");
            ISequenceBarrier sBarrier = ringBuffer.newBarrier();
            BatchEventProcessor<TestEvent> batchEventProcessor = new BatchEventProcessor<TestEvent>(ringBuffer, sBarrier, handler);
            ringBuffer.addGatingSequences(batchEventProcessor.getSequence());
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (Int32 x = 0;x < numberOfThreads; x++)
            {
                Int32 tempthreadId = x;
                Task.Factory.StartNew(() =>
                {
                    for (long i = 0; i < iterations; i++)
                    {
                        var hi = ringBuffer.next(batchSize);
                        var low = hi - batchSize + 1;

                        for (long l = low; l <= hi; l++)
                        {
                            var entry = ringBuffer.get(l);
                            entry.value = 1;
                            
                        }
                        ringBuffer.publish(low,hi);
                        i += batchSize - 1; //-1 as the iteration will increment
                        
                    }
                });

            }
            Task.Factory.StartNew(() =>
                {
                    batchEventProcessor.run();
                });
         
            while (batchEventProcessor.getSequence().get() != (expectedCount-1))
            {
                Thread.Sleep(1);
            }
            stopwatch.Stop();
            System.Console.WriteLine("[Test3P1CBatch] Consumer is done processing Events! Total Time in milliseconds:" + stopwatch.Elapsed.TotalMilliseconds + " " + String.Format("{0:###,###,###,###}op/sec", ((expectedCount) / stopwatch.Elapsed.TotalSeconds)));

            batchEventProcessor.halt();


            System.Console.WriteLine("============================");

          
        }
     
        public static void FizzBuzz1P3CBlockingCollection()
        {
            System.Console.WriteLine("============================");
            System.Console.WriteLine("Starting FizzBuzz1P3CBlockingCollection Test");

            Disruptor3_Net.Tests.FizzBuzz1P3CBlockingCollectionPerfTest test = new Tests.FizzBuzz1P3CBlockingCollectionPerfTest();
            test.RunPass();
            System.Console.WriteLine("============================");
         
        }
        public static void FizzBuzz1P3C()
        {
            System.Console.WriteLine("============================");
            System.Console.WriteLine("Starting FizzBuzz1P3C Test");
          
            Disruptor3_Net.Tests.FizzBuzz1P3C test = new Tests.FizzBuzz1P3C();
         
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
           
            dsl.Disruptor<Test3P1CEvent> disruptor = new dsl.Disruptor<Test3P1CEvent>(new Test3P1CEventFactory(), 2048, dsl.ProducerType.MULTI, new Disruptor3_Net.WaitStrategies.BusySpinWaitStrategy());
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
                    Int64 currentCounter = 0;
                    currentCounter= 1;

                    while (currentCounter <= totalPerThread)
                    {
                        long sequence = ringBuffer.next();
                        Test3P1CEvent tEvent = ringBuffer.get(sequence);
                        tEvent.currentCounter = currentCounter;
                        ringBuffer.publish(sequence);
                        currentCounter++;
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
           dsl.Disruptor<TestEvent> disruptor = new dsl.Disruptor<TestEvent>(new TestEventFactory(), 1024, dsl.ProducerType.MULTI, new Disruptor3_Net.WaitStrategies.BusySpinWaitStrategy());
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

            Int32 batchSize = 1;
            Int32 currentBatchSize = batchSize;

            Int32 numberOfThreads = 1;
            Int32 totalNumber = batchSize * 10000000*numberOfThreads; ;
            Int32 numberPerThread = totalNumber / numberOfThreads;
             System.Console.WriteLine("Starting TestMultiThreading Test with " + String.Format("{0:###,###,###,###} entries", totalNumber) + " and with " + numberOfThreads + " threads");
         
            dsl.Disruptor<TestEvent> disruptor = new dsl.Disruptor<TestEvent>(new TestEventFactory(), 2048*2, dsl.ProducerType.MULTI, new Disruptor3_Net.WaitStrategies.BusySpinWaitStrategy());
            TestConsumer handler = new TestConsumer("TestMultiThreading");
            disruptor.handleEventsWith(handler);
            disruptor.start();
          
            RingBuffer<TestEvent> ringBuffer = disruptor.getRingBuffer();
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
          

            for(Int32 i = 0;i<numberOfThreads;i++)
            {
                Task.Factory.StartNew(() => {

                    Random rand = new Random();

                    Int64 currentCounter = 1;

                   
                    while(currentCounter<=numberPerThread)
                    {
                       
                        Int64 sequence = ringBuffer.next(currentBatchSize);
                        Int64 indexLocation=0;
                        for (Int32 x = 0; x < currentBatchSize; x++)
                        {
                            indexLocation = sequence - (currentBatchSize-1) + x;
                            TestEvent tEvent = ringBuffer.get(indexLocation);
                            tEvent.value = i;
                            ringBuffer.publish(indexLocation);

                        }
                        //ringBuffer.publish(sequence);

                        currentCounter += currentBatchSize;
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
        public static void Test1P1C()
        {
            System.Console.WriteLine("============================");

           Int32 totalNumber = 30000000;
            System.Console.WriteLine("Starting Test1P1C Test with " + String.Format("{0:###,###,###,###} entries", totalNumber) + " and a single thread");
         
         
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
        
            Disruptor<TestEvent> disruptor = new dsl.Disruptor<TestEvent>(new TestEventFactory(), 2048,ProducerType.SINGLE, new WaitStrategies.BusySpinWaitStrategy());
            TestConsumer handler = new TestConsumer("TestSingleThreading");
            disruptor.handleEventsWith(handler);
            disruptor.start();

            RingBuffer<TestEvent> ringBuffer = disruptor.getRingBuffer();
          
            for(Int32 i = 1; i < 5;i++)
            {
                Int64 currentCounter = 0;
                System.Diagnostics.Stopwatch stopwatch = new Stopwatch();


                stopwatch.Start();
                while (currentCounter < totalNumber)
                {
                    long sequence = ringBuffer.next(1);
                    TestEvent tEvent = ringBuffer.get(sequence);
                    tEvent.value = 1;
                    ringBuffer.publish(sequence);

                    currentCounter++;
                }

                while (ringBuffer.getCursor() != (totalNumber*i - 1))
                {
                    Thread.Sleep(1);
                }
                stopwatch.Stop();
                System.Console.WriteLine("[TestSingleThreading] Consumer is done processing Events! Total Time in milliseconds:" + stopwatch.Elapsed.TotalMilliseconds + " " + String.Format("{0:###,###,###,###}op/sec", (totalNumber / stopwatch.Elapsed.TotalSeconds)));

            }


            disruptor.shutdown();
            
            System.Console.WriteLine("============================");
 

        }

        
    }
}
