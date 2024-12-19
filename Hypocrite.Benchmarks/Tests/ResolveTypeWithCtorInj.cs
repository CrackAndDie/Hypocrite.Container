using BenchmarkDotNet.Attributes;
using Hypocrite.Container;
using Hypocrite.Container.Interfaces;
using StyletIoC;
using Unity;

namespace Hypocrite.Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class ResolveTypeWithCtorInj
    {
        ILightContainer _lightContainer;
        IUnityContainer _unityContainer;
        IContainer _styletContainer;

        public ResolveTypeWithCtorInj()
        {
            _lightContainer = new LightContainer();
            _lightContainer.Register<Test_PureResolveType, Test_PureResolveType>();
            _lightContainer.Register<Test_ResolveTypeWithCtorInj_Light, Test_ResolveTypeWithCtorInj_Light>();

            _unityContainer = new UnityContainer();
            _unityContainer.RegisterType<Test_PureResolveType>();
            _unityContainer.RegisterType<Test_ResolveTypeWithCtorInj_Unity>();

            var builder = new StyletIoCBuilder();
            builder.Bind<Test_PureResolveType>().ToSelf();
            builder.Bind<Test_ResolveTypeWithCtorInj_Stylet>().ToSelf();
            _styletContainer = builder.BuildContainer();
        }

        [Benchmark]
        public Test_ResolveTypeWithCtorInj_Unity WithUnityContainer()
        {
            return _unityContainer.Resolve<Test_ResolveTypeWithCtorInj_Unity>();
        }

        [Benchmark]
        public Test_ResolveTypeWithCtorInj_Light WithLightContainer()
        {
            return _lightContainer.Resolve<Test_ResolveTypeWithCtorInj_Light>();
        }

        [Benchmark]
        public Test_ResolveTypeWithCtorInj_Stylet WithStyletContainer()
        {
            return _styletContainer.Get<Test_ResolveTypeWithCtorInj_Stylet>();
        }
    }

    public class Test_ResolveTypeWithCtorInj_Unity
    {
        public int A { get; set; }
        public string B { get; set; }

        public Test_PureResolveType Test { get; set; }

        [InjectionConstructor]
        public Test_ResolveTypeWithCtorInj_Unity(Test_PureResolveType test)
        {
            Test = test;
        }
    }

    public class Test_ResolveTypeWithCtorInj_Light
    {
        public int A { get; set; }
        public string B { get; set; }

        public Test_PureResolveType Test { get; set; }

        [Injection]
        public Test_ResolveTypeWithCtorInj_Light(Test_PureResolveType test)
        {
            Test = test;
        }
    }

    public class Test_ResolveTypeWithCtorInj_Stylet
    {
        public int A { get; set; }
        public string B { get; set; }

        public Test_PureResolveType Test { get; set; }

        [Inject]
        public Test_ResolveTypeWithCtorInj_Stylet(Test_PureResolveType test)
        {
            Test = test;
        }
    }
}
