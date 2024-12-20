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

        Dictionary<int, object> dictionary3;
        QuickSet<object> quickSet3;

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

            dictionary3 = new Dictionary<int, object>();
            dictionary3.Add(typeof(Test_PureResolveType).GetHashCode(), new Test_PureResolveType());
            quickSet3 = new QuickSet<object>();
            quickSet3.AddOrReplace(typeof(Test_PureResolveType).GetHashCode(), "", new Test_PureResolveType());
        }

        //[Benchmark]
        //public Test_PureResolveType WithDictionary()
        //{
        //    return dictionary[(123, "Anime")] as Test_PureResolveType;
        //}

        //[Benchmark]
        //public Test_PureResolveType WithQuickSet()
        //{
        //    return quickSet.Get(123, "Anime") as Test_PureResolveType;
        //}

        //[Benchmark]
        //public Test_PureResolveType WithDictionaryCalc()
        //{
        //    return dictionary2[(typeof(Test_PureResolveType), "Anime")] as Test_PureResolveType;
        //}

        //[Benchmark]
        //public Test_PureResolveType WithQuickSetCalc()
        //{
        //    return quickSet2.Get(typeof(Test_PureResolveType).GetHashCode(), "Anime") as Test_PureResolveType;
        //}

        //[Benchmark]
        //public Test_PureResolveType WithDictionaryCalc()
        //{
        //    int hash = typeof(Test_PureResolveType).GetHashCode();
        //    if (dictionary3.ContainsKey(hash))
        //        return dictionary3[hash] as Test_PureResolveType;
        //    return null;
        //}

        //[Benchmark]
        //public Test_PureResolveType WithDictionaryCalcTryGet()
        //{
        //    int hash = typeof(Test_PureResolveType).GetHashCode();
        //    if (dictionary3.TryGetValue(hash, out var val))
        //        return val as Test_PureResolveType;
        //    return null;
        //}

        [Benchmark]
        public Test_PureResolveType WithQuickSetCalc()
        {
            return quickSet2.Get(typeof(Test_PureResolveType).GetHashCode(), "") as Test_PureResolveType;
        }

        [Benchmark]
        public int WithIndex()
        {
            return (typeof(Test_PureResolveType).GetHashCode() & 0x7FFFFFFF) % 37;
        }
    }
}
