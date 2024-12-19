using BenchmarkDotNet.Attributes;
using Hypocrite.Container.Common;
using Hypocrite.Container.Extensions;
using System.Collections.Generic;

namespace Hypocrite.Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class DictionaryTest
    {
        Dictionary<(int, string), object> dictionary;
        QuickSet<object> quickSet;

        public DictionaryTest()
        {
            dictionary = new Dictionary<(int, string), object>();
            dictionary.Add((123, "Anime"), new Test_PureResolveType());

            quickSet = new QuickSet<object>();
            quickSet.AddOrReplace(123, "Anime", new Test_PureResolveType());
        }

        [Benchmark]
        public Test_PureResolveType WithDictionary()
        {
            return dictionary[(123, "Anime")] as Test_PureResolveType;
        }

        [Benchmark]
        public Test_PureResolveType WithQuickSet()
        {
            return quickSet.Get(123, "Anime").Value.Value as Test_PureResolveType;
        }
    }
}
