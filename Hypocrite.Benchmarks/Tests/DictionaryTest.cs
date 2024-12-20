using BenchmarkDotNet.Attributes;
using Hypocrite.Container.Common;
using Hypocrite.Container.Extensions;
using System;
using System.Collections.Generic;

namespace Hypocrite.Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class DictionaryTest
    {
        Dictionary<(int, string), object> dictionary;
        QuickSet<object> quickSet;

        Dictionary<(Type, string), object> dictionary2;
        QuickSet<object> quickSet2;

        public DictionaryTest()
        {
            dictionary = new Dictionary<(int, string), object>();
            dictionary.Add((123, "Anime"), new Test_PureResolveType());
            quickSet = new QuickSet<object>();
            quickSet.AddOrReplace(123, "Anime", new Test_PureResolveType());

            dictionary2 = new Dictionary<(Type, string), object>();
            dictionary2.Add((typeof(Test_PureResolveType), "Anime"), new Test_PureResolveType());
            quickSet2 = new QuickSet<object>();
            quickSet2.AddOrReplace(typeof(Test_PureResolveType).GetHashCode(), "Anime", new Test_PureResolveType());
        }

        [Benchmark]
        public Test_PureResolveType WithDictionary()
        {
            return dictionary[(123, "Anime")] as Test_PureResolveType;
        }

        [Benchmark]
        public Test_PureResolveType WithQuickSet()
        {
            return quickSet.Get(123, "Anime") as Test_PureResolveType;
        }

        [Benchmark]
        public Test_PureResolveType WithDictionaryCalc()
        {
            return dictionary2[(typeof(Test_PureResolveType), "Anime")] as Test_PureResolveType;
        }

        [Benchmark]
        public Test_PureResolveType WithQuickSetCalc()
        {
            return quickSet2.Get(typeof(Test_PureResolveType).GetHashCode(), "Anime") as Test_PureResolveType;
        }
    }
}
