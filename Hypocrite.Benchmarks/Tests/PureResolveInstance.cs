using BenchmarkDotNet.Attributes;
using Hypocrite.Container;
using Hypocrite.Container.Interfaces;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Hypocrite.Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class PureResolveInstance
    {
        ILightContainer _lightContainer;
        IUnityContainer _unityContainer;
        IContainer _styletContainer;

        public PureResolveInstance()
        {
            _lightContainer = new LightContainer();
            _lightContainer.RegisterInstance<Test_PureResolveInstance>(new Test_PureResolveInstance());

            _unityContainer = new UnityContainer();
            _unityContainer.RegisterInstance(new Test_PureResolveInstance());

            var builder = new StyletIoCBuilder();
            builder.Bind<Test_PureResolveInstance>().ToFactory(container => new Test_PureResolveInstance()).InSingletonScope(); // i couldn't find instance registration
            _styletContainer = builder.BuildContainer();
        }

        [Benchmark]
        public Test_PureResolveInstance WithUnityContainer()
        {
            return _unityContainer.Resolve<Test_PureResolveInstance>();
        }

        [Benchmark]
        public Test_PureResolveInstance WithLightContainer()
        {
            return _lightContainer.Resolve<Test_PureResolveInstance>();
        }

        [Benchmark]
        public Test_PureResolveInstance WithStyletContainer()
        {
            return _styletContainer.Get<Test_PureResolveInstance>();
        }
    }

    public class Test_PureResolveInstance
    {
        public int A { get; set; }
        public string B { get; set; }
    }
}
