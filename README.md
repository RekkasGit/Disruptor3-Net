# Disruptor3-Net
Disruptor 3.3.8 converted over for .net usage.

Was a big fan of the Java version, and tried to use the .net version. Unfortunatly I encountered issues where it could stall and looked like the project was abandoned.
I took two days (over Christmas holidays 2015) and converted the then current version (3.3.8) over to .net. 

For stability, it seems to work well and it is currently used in production environments without problem. 

Performance numbers (done on an i72600k at around 4ghz)
<br/>

Single producer single consumer using batch.
<br/>
1,000,000,000 entries<br/>

| Time(ms)        | ops/sec           | 
|:------------- |-------------:| 
| 6124.9933      | 163,265,485 | 
| 6057.9575	     | 165,072,139      | 
| 6134.7049 |163,007,026     | 
| 6134.8383 | 163,003,481     | 
<br/>
Single consumer, single producer, but not using batch. <br/>
<br/>
600,000,000 entries<br/>

| Time(ms)        | ops/sec           | 
|:------------- |-------------:| 
| 6271.0181      |95,678,244 | 
| 6463.5193    | 92,828,685      | 
|6454.4628	 |92,958,937     | 
|6434.5762 | 93,246,234    | 

Three producer, single consumer , using batch.<br/>
<br/>
600,000,000 entries<br/>

| Time(ms)        | ops/sec           | 
|:------------- |-------------:| 
| 7092.9208      |82,905,835 | 
| 7092.9208    | 84,591,386      | 
|7226.0525	 |83,032,887     | 
|7072.2216 |  84,838,971     | 



Three producer, single consumer , *NOT* using batch.<br/>
<br/>
90,000,000 entries

| Time(ms)        | ops/sec           | 
|:------------- |-------------:| 
| 6026.7175     |14,933,502 | 
| 6869.5029    |13,101,385      | 
|5848.7995		 |15,387,773     | 
|5795.8661 | 15,528,309    | 
<br/>

Round numbers from a colleague's performance test program<br/>

Concurrent Queue 1P1C		28-33 million op/sec<br/> 
Concurrent Queue 2P1C		14-17 million op/sec<br/>
Concurrent Queue 3P1C		13-16 million op/sec<br/>
<br/>
Blocking Collection 1P1C	5.0 million op/sec<br/>
Blocking Collection 2P1C	4.8 million op/sec<br/>
Blocking Collection 3P1C	4.5 million op/sec<br/>
<br/>
BufferBlockSpin 1P1C		15 million op/sec<br/>
BufferBlockSpin 2P1C		15 million op/sec<br/>
BufferBlockSpin 3P1C		13 million op/sec<br/>
<br/>
ActionBlock 1P1C 30-50 milion op/sec<br/>
<br/>
Example usage.
<br/>
**NOTE**! for performance numbers, be sure to have the application in release/64bit mode (on the console turn off perfer 32bit)
<br/>
```c#
//First we define the Disruptor
//In this case we only have a single producer and a single consumer. This allows us to use the Single mode.
//Far faster than the Multi.

Int32 ringBufferSize = 1024;//Must be a power of 2.

Disruptor<TestEvent> disruptor = new dsl.Disruptor<TestEvent>(new TestEventFactory(), ringBufferSize,ProducerType.SINGLE, new WaitStrategies.BusySpinWaitStrategy());

// TestEvent is our Model as it were. It only holds a single value
/*
	public class TestEvent
    {
        public Int64 value = 0;

    }
}
*/
//We also have a Factory to create the Model
/*
public class TestEventFactory: Disruptor_Net3.Interfaces.IEventFactory<TestEvent>
{
    public TestEvent newInstance()
    {
        return new TestEvent();
    }
}

*/

//The wait strategy is depending on the amount of CPU cores you have available. In this case we want to burn a core for lower latencies higher throughput so we choose BusySpinWait
//Generally you will use spin, yield or blocking.
```
```C#
//So now we have our disruptor ready, but we need something to handle the events that will fire so we create one.

TestConsumer handler = new TestConsumer("TestSingleThreading");
/*
 public class TestConsumer : Disruptor_Net3.Interfaces.IEventHandler<TestEvent>
{

    string _name;
    Int64 tempValue;
    public TestConsumer (string name)
    {
        _name = name;
    }

	//Work is done here!
    public void onEvent(TestEvent eventToUse, long sequence, bool endOfBatch)
    {
       tempValue = sequence;//just to do something but not much overall
    }
}
*/

//Now to register the handler to the disruptor.
disruptor.handleEventsWith(handler);

//Lets start the threads that do the processing, return value is the ring buffer.            
disruptor.start();
```
```c#
//we can also get the ring buffer by just asking the distruptor for it
RingBuffer<TestEvent> ringBuffer = disruptor.getRingBuffer();

//now that we have all the setup done, lets actually use it!

//First we claim a sequence number. You can get more than one, but for now we will just do one.
long sequence = ringBuffer.next();
//now retrieve the object out of the ring buffer
TestEvent tEvent = ringBuffer.get(sequence);
//do something with the object
tEvent.value = 1;
//notify disruptor that this sequence number is avaialble.
ringBuffer.publish(sequence);

/********IMPORTANT**************
If you do a next, you *MUST* do a publish. Period.
You can do a try/finally but there are some pretty heafty performance penalities on it
try/catch is better performance.  Note, if you need to skip the event you can set the object to be skipped such as a boolean flag
on the object itself , but you must still publish it.
*/


```
```c#
//if we wish to do a batch it would be more like this.

var hi = ringBuffer.next(batchSize);
var low = hi - batchSize + 1; //zero based index, so add 1

for (long l = low; l <= hi; l++)
{
    var entry = ringBuffer.get(l);
    entry.value = 1;
                            
}
ringBuffer.publish(low,hi);
```
```c#
//You can also chain consumers like thus
disruptor.handleEventsWith(_fizzEventHandler, _buzzEventHandler).then(_fizzBuzzEventHandler);
//what this means is fizz/buzz will get the event, each on a seperate thread at the same time. After both of these have been processed, _fizzBuzzEventHandler will then process on its own thread.   


```




