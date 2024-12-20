using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System.Numerics;

namespace Hypocrite.Benchmarks.Tests
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.SlowestToFastest, MethodOrderPolicy.Declared)]
    public class DivBench
    {
        [Benchmark(Baseline = true)]
        [Arguments(432415, 37)]
        public int Div(int a, int b)
        {
            return a / b;
        }

        [Benchmark]
        [Arguments(432415, 37)]
        // Inst:        Reg:     Ports:     Latency:
        // ------------------------------------------
        // SHR SHL SAR   r,i     p06             1
        // SHR SHL SAR   m,i     2p06 p237 p4    2 
        public int Div_Bit_Fast(int a, int b)
        {
            int ret = 0; int mask = 1;
            while (a >= b)
            {
                b <<= 1;
                mask <<= 1;
            }

            while ((mask & 1) == 0)
            {
                b >>= 1; mask >>= 1;
                if (a >= b)
                {
                    ret |= mask;
                    a -= b;
                };
            };
            return ret;
        }

        [Benchmark]
        [Arguments(432415, 37)]
        // Inst:        Reg:     Ports:     Latency:
        // ------------------------------------------
        // SHR SHL SAR   r,i     p06             1
        // SHR SHL SAR   m,i     2p06 p237 p4    2 
        public int Div_Bit_Faster(int a, int b)
        {
            int ret = 0; int mask = 1;
            while (a >= b)
            {
                unchecked
                {
                    var div1 = BitOperations.LeadingZeroCount((uint)a);
                    var div2 = BitOperations.LeadingZeroCount((uint)b);
                    var shl = (div2 - div1) + 1;
                    b <<= shl;
                    mask <<= shl;
                }
            };

            while ((mask & 1) == 0)
            {
                b >>= 1;
                mask >>= 1;
                if (a >= b)
                {
                    ret |= mask;
                    a -= b;
                };
            };
            return ret;
        }

        [Benchmark]
        [Arguments(432415, 37)]
        public int Div_Power(int a, int b)
        {
            var tmp = b - 1;
            if ((b & tmp) == 0) //Is Power of Two
            {
                unchecked
                {
                    var div = BitOperations.LeadingZeroCount((uint)tmp);
                    var shr = 64 - div - 1;

                    return a >> shr;
                }
            }

            return a / b;
        }

        /* Inst:   Reg:    Ports:          Latency:
        -----------------------------------------
           DIV     r32     p0 p1 p5 p6     26 
           DIV     r64     p0 p1 p5 p6     35-88
           IDIV    r32     p0 p1 p5 p6     26 
           IDIV    r64     p0 p1 p5 p6     42-95 */
        [Benchmark]
        [Arguments(432415, 37)]
        public int Div_Fast_Lowering(int a, int b)
        {
            return a / b;
        }

    }
}
