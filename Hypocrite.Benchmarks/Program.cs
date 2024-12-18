﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Hypocrite.Benchmarks.Tests;
using Hypocrite.Container;
using System;
using System.Linq;
using System.Reflection;

namespace Hypocrite.Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Use reflection for a more maintainable way of creating the benchmark switcher,
            // Benchmarks are listed in namespace order first (e.g. BenchmarkDotNet.Samples.CPU,
            // BenchmarkDotNet.Samples.IL, etc) then by name, so the output is easy to understand
            var benchmarks = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                             .Any(m => m.GetCustomAttributes(typeof(BenchmarkAttribute), false).Any()))
                .OrderBy(t => t.Namespace)
                .ThenBy(t => t.Name)
                .ToArray();
            var benchmarkSwitcher = new BenchmarkSwitcher(benchmarks);
            benchmarkSwitcher.Run(args);

            //var _lightContainer = new LightContainer();
            //_lightContainer.Register<Test_PureResolveType, Test_PureResolveType>();

            //_lightContainer.Resolve<Test_PureResolveType>();
            //_lightContainer.Resolve<Test_PureResolveType>();

            Console.ReadKey();
        }
    }
}
