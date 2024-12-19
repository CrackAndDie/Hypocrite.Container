using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hypocrite.Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class ForLoopTest
    {
        private static int[] _testArr;

        public ForLoopTest()
        {
            _testArr = Enumerable.Range(0, 100000).ToArray();
        }

        [Benchmark]
        public void WithFor()
        {
            int sum = 0;
            for (int i = 0; i < _testArr.Length; ++i)
            {
                sum += _testArr[i];
            }
        }

        [Benchmark]
        public void WithForeach()
        {
            int sum = 0;
            int i = 0;
            foreach (int x in _testArr)
            {
                sum += x;
                i++;
            }
        }
    }
}
