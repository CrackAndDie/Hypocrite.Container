using BenchmarkDotNet.Attributes;
using Hypocrite.Container;
using Hypocrite.Container.Interfaces;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Hypocrite.Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class PureResolveFactory
    {
        ILightContainer _lightContainer;
        IUnityContainer _unityContainer;
        IContainer _styletContainer;

        public PureResolveFactory()
        {
            _lightContainer = new LightContainer();
            _lightContainer.RegisterFactory<Test_PureResolveFactory>((container, type, name) => new Test_PureResolveFactory());

            _unityContainer = new UnityContainer();
            _unityContainer.RegisterFactory<Test_PureResolveFactory>((container) => new Test_PureResolveFactory());

            var builder = new StyletIoCBuilder();
            builder.Bind<Test_PureResolveFactory>().ToFactory(container => new Test_PureResolveFactory());
            _styletContainer = builder.BuildContainer();
        }

        [Benchmark]
        public Test_PureResolveFactory WithUnityContainer()
        {
            return _unityContainer.Resolve<Test_PureResolveFactory>();
        }

        [Benchmark]
        public Test_PureResolveFactory WithLightContainer()
        {
            return _lightContainer.Resolve<Test_PureResolveFactory>();
        }

        [Benchmark]
        public Test_PureResolveFactory WithStyletContainer()
        {
            return _styletContainer.Get<Test_PureResolveFactory>();
        }
    }

    public class Test_PureResolveFactory
    {
        public int A { get; set; }
        public string B { get; set; }
    }
}
