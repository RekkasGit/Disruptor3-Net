using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor_Net3.Tests
{
    public abstract class PerfTest
    {
        protected PerfTest(int iterations)
        {
            Iterations = iterations;
        }

        public int PassNumber { get; set; }
        public int Iterations { get; private set; }
        //protected abstract void RunAsUnitTest();
        //public abstract void RunPerformanceTest();
        protected const int Million = 1000 * 1000;
        public abstract int MinimumCoresRequired { get; }
    }
}
