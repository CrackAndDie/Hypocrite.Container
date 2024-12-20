using BenchmarkDotNet.Attributes;
using Hypocrite.Container;
using Hypocrite.Container.Interfaces;
using StyletIoC;
using System.Diagnostics;
using Unity;

namespace Hypocrite.Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class ResolveTypeWithParamsInj
    {
        ILightContainer _lightContainer;
        IUnityContainer _unityContainer;
        IContainer _styletContainer;

        public ResolveTypeWithParamsInj()
        {
            _lightContainer = new LightContainer();
            _lightContainer.Register<Test_PureResolveType, Test_PureResolveType>();
            _lightContainer.Register<Test_ResolveTypeWithParamsInj_Light, Test_ResolveTypeWithParamsInj_Light>();

            _unityContainer = new UnityContainer();
            _unityContainer.RegisterType<Test_PureResolveType>();
            _unityContainer.RegisterType<Test_ResolveTypeWithParamsInj_Unity>();

            var builder = new StyletIoCBuilder();
            builder.Bind<Test_PureResolveType>().ToSelf();
            builder.Bind<Test_ResolveTypeWithParamsInj_Stylet>().ToSelf();
            _styletContainer = builder.BuildContainer();
        }

        [Benchmark]
        public Test_ResolveTypeWithParamsInj_Unity WithUnityContainer()
        {
            var resolved = _unityContainer.Resolve<Test_ResolveTypeWithParamsInj_Unity>();
            Debug.Assert(resolved.Test is Test_PureResolveType);
            Debug.Assert(resolved.test is Test_PureResolveType);
            return resolved;
        }

        [Benchmark]
        public Test_ResolveTypeWithParamsInj_Light WithLightContainer()
        {
            var resolved = _lightContainer.Resolve<Test_ResolveTypeWithParamsInj_Light>();
            Debug.Assert(resolved.Test is Test_PureResolveType);
            Debug.Assert(resolved.test is Test_PureResolveType);
            return resolved;
        }

        [Benchmark]
        public Test_ResolveTypeWithParamsInj_Stylet WithStyletContainer()
        {
            var resolved = _styletContainer.Get<Test_ResolveTypeWithParamsInj_Stylet>();
            Debug.Assert(resolved.Test is Test_PureResolveType);
            Debug.Assert(resolved.test is Test_PureResolveType);
            return resolved;
        }
    }

    public class Test_ResolveTypeWithParamsInj_Unity
    {
        public int A { get; set; }
        public string B { get; set; }

        [Dependency]
        public Test_PureResolveType Test { get; set; }
        [Dependency]
        public Test_PureResolveType test;
    }

    public class Test_ResolveTypeWithParamsInj_Light
    {
        public int A { get; set; }
        public string B { get; set; }

        [Injection]
        public Test_PureResolveType Test { get; set; }
        [Injection]
        public Test_PureResolveType test;
    }

    public class Test_ResolveTypeWithParamsInj_Stylet
    {
        public int A { get; set; }
        public string B { get; set; }

        [Inject]
        public Test_PureResolveType Test { get; set; }
        [Inject]
        public Test_PureResolveType test;
    }
}
