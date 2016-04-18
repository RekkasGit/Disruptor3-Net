# Disruptor3-Net
Disruptor converted over for .net usage.
Was a big fan of the Java version, and tried to use the .net version. Had issues and looked like the project was abandoned.
So I took two days (over Christmas holidays 2015) and converted the current version (3.3.8) over to .net. 

This is an initial development state, though seems to work well. 


Performance numbers, as they always are what people are looking for.
<br/>

Single producer single consumer using batch.
<br/><br>
1,000,000,000 entries<br/>

| Time(ms)        | ops/sec           | 
|:------------- |-------------:| 
| 6124.9933      | 163,265,485 | 
| 6057.9575	     | 165,072,139      | 
| 6134.7049 |163,007,026     | 
| 6134.8383 | 163,003,481     | 
<br/>
Single consumer, single producer, but not using batch. <br/>
This tends to fluxuate a bit.<br/>
<br/>
1,000,000,000 entries<br/>
| Time(ms)        | ops/sec           | 
|:------------- |-------------:| 
| 13833.3955      |72,288,832 | 
| 10664.2537     | 93,771,213      | 
|12565.4032	 |79,583,598     | 
|10404.4818 | 96,112,427     | 
<br/>
Three Producer, single consumer , using batch.<br/>
<br/>
600,000,000 entries<br/>
| Time(ms)        | ops/sec           | 
|:------------- |-------------:| 
| 7138.7307      |84,048,555 | 
| 7259.6242    | 82,648,906      | 
|7123.6162	 |84,226,885     | 
|7223.8552 | 83,058,143    | 

<br/>
Three Producer, single consumer , *NOT* using batch.<br/>
<br/>
90,000,000 entries
| Time(ms)        | ops/sec           | 
|:------------- |-------------:| 
| 7109.6871      |12,658,785 | 
| 7071.3333    |12,727,444      | 
|7466.917		 |12,053,167     | 
|8295.9761 | 10,848,633    | 
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
BufferBlock with link 1P1C	5.8 million op/sec<br/>
BufferBlock with link 2P1C	6.1 million op/sec<br/>
BufferBlock with link 3P1C	5.7 million op/sec<br/>
<br/>
<br/>
Example usage.
<br/>
**NOTE**! for performance numbers, be sure to have the application in release/64bit mode (on the console turn off perfer 32bit)
<br/>
```c#
//First we define the Disruptor
//In this case we only have a single producer and a single consumer. This allows us to use the Single mode. Far faster than the Multi.

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
try/catch is better performance.  and you can set the object to be skipped such as a boolean flag
on the object itself to tell the event to be skipped.
*/

//if we wish to do a batch it would be more like this.

var hi = ringBuffer.next(batchSize);
var low = hi - batchSize + 1; //zero based index, so add 1

for (long l = low; l <= hi; l++)
{
    var entry = ringBuffer.get(l);
    entry.value = 1;
                            
}
ringBuffer.publish(low,hi);



//You can also chain consumers like thus
_isruptor.handleEventsWith(_fizzEventHandler, _buzzEventHandler).then(_fizzBuzzEventHandler);
//what this means is fizz/buzz will get the event, each on a seperate thread at the same time. After both of these have been processed, _fizzBuzzEventHandler will then process on its own thread.   


```




