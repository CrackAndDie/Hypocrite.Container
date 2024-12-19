using BenchmarkDotNet.Attributes;

namespace Hypocrite.Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class OutParamTest
    {
        [Benchmark]
        public void WithOut()
        {
            var str = TestOut(out var arr);
        }

        private string TestOut(out int[] arr)
        {
            arr = new int[] { 1, 2, 3 };
            return "Anime";
        }

        [Benchmark]
        public void WithoutOut()
        {
            var (str, arr) = TestNoOut();
        }

        private (string, int[]) TestNoOut()
        {
            return ("Anime", new int[] { 1, 2, 3 });
        }
    }
}
