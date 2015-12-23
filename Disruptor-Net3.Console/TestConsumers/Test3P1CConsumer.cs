using Disruptor_Net3.Console.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disruptor_Net3.Console
{
    class TestXP1CConsumer : Disruptor_Net3.Interfaces.IEventHandler<Test3P1CEvent>
    {

        PaddedLong[] _threadCounter = new PaddedLong[3];
        string _name;
        ManualResetEventSlim _resetEvent;
        System.Diagnostics.Stopwatch stopwatch = null;
        Int32 _numberOfThreads;
 
        public Int64 totalExpected = 0;
        public TestXP1CConsumer(string name, ManualResetEventSlim resetEvent, Int32 numberOfThreads)
        {
            _numberOfThreads = numberOfThreads;
            _name = name;
            _resetEvent = resetEvent;
            _threadCounter = new PaddedLong[numberOfThreads];
            for (Int32 i = 0; i < numberOfThreads;i++ )
            {
                _threadCounter[i].value = 0;
            }
        }

        public void onEvent(Test3P1CEvent eventToUse, long sequence, bool endOfBatch)
        {
            if (stopwatch == null)
            {
                stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
            }

            Int64 previousCounter = _threadCounter[eventToUse.threadID].value;
            if(previousCounter+1==eventToUse.currentCounter)
            {
                _threadCounter[eventToUse.threadID].value = eventToUse.currentCounter;
            }
            else
            {
                throw new Exception("Error, value not expected!");
            }

            Boolean allPassed = true;
            
            for (Int32 i = 0; i < _numberOfThreads;i++ )
            {
                if(_threadCounter[i].value!=totalExpected)
                {
                    allPassed = false;
                    break;
                }

                if(!allPassed)
                {
                    break;
                }

            }
            if(allPassed)
            {
                stopwatch.Stop();
                System.Console.WriteLine("[" + _name + "] Consumer is done processing Events! Total Time in milliseconds:" + stopwatch.Elapsed.TotalMilliseconds + " " + String.Format("{0:###,###,###,###}op/sec", ((totalExpected*_numberOfThreads) / stopwatch.Elapsed.TotalSeconds)));
                _resetEvent.Set();
        
            }
        }
    }
}
