using BenchmarkDotNet.Attributes;
using Hypocrite.Container;
using Hypocrite.Container.Interfaces;
using StyletIoC;
using Unity;

namespace Hypocrite.Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class PureResolveSingleton
    {
        ILightContainer _lightContainer;
        IUnityContainer _unityContainer;
        IContainer _styletContainer;

        public PureResolveSingleton()
        {
            _lightContainer = new LightContainer();
            _lightContainer.RegisterSingleton<Test_PureResolveSingleton, Test_PureResolveSingleton>();

            _unityContainer = new UnityContainer();
            _unityContainer.RegisterSingleton<Test_PureResolveSingleton>();

            var builder = new StyletIoCBuilder();
            builder.Bind<Test_PureResolveSingleton>().ToSelf().InSingletonScope();
            _styletContainer = builder.BuildContainer();
        }

        [Benchmark]
        public Test_PureResolveSingleton WithUnityContainer()
        {
            return _unityContainer.Resolve<Test_PureResolveSingleton>();
        }

        [Benchmark]
        public Test_PureResolveSingleton WithLightContainer()
        {
            return _lightContainer.Resolve<Test_PureResolveSingleton>();
        }

        [Benchmark]
        public Test_PureResolveSingleton WithStyletContainer()
        {
            return _styletContainer.Get<Test_PureResolveSingleton>();
        }
    }

    public class Test_PureResolveSingleton
    {
        public int A { get; set; }
        public string B { get; set; }
    }
}
