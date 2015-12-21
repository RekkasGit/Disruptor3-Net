# Disruptor3-Net
Disruptor converted over for .net usage.
Was a big fan of the Java version, and tried to use the .net version. Had issues and looked like the project was abandoned.
So I took two days (over christmas holidays) and converted the current version (3.3.8) over to .net. 
Initial development state, though seems to work well. 

(Note, this is just a place holder till I can tidy it up after the holidays)

As a FYI, performance seems better than the original Disruptor-Net version.
On a i72600K, set to High Priority.

Just passing an integer around between two threads.
1C1P = over 90-100 million ops/second

Just passsing an integer around between 4 threads.
3P1C = 16 milllion ops/second

Doing actual work of fizz buzz with 3 consuemrs (check for fizz, check for buzz, check for fizzbuzz)
FizzBuzz 1P3C = around 50-64 million ops/second.

Any feedback would be appricative but still have more work to do. Esplically in the tests as this is *very* raw at the moment.

PS: forgive the name in the project will be changed eventually :) (and still need to get the licence stuff in there)
