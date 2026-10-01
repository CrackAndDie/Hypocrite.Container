using BenchmarkDotNet.Attributes;
using Hypocrite.Container;
using Hypocrite.Container.Interfaces;
using StyletIoC;
using System.Diagnostics;
using Unity;

namespace Hypocrite.Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class ResolveTypeWithInheritedInj
    {
        ILightContainer _lightContainer;
        IUnityContainer _unityContainer;
        IContainer _styletContainer;

        public ResolveTypeWithInheritedInj()
        {
            _lightContainer = new LightContainer();
            _lightContainer.Register<Test_PureResolveType, Test_PureResolveType>();
            _lightContainer.Register<NormalClassInheritedInj_Light, NormalClassInheritedInj_Light>();

            _unityContainer = new UnityContainer();
            _unityContainer.RegisterType<Test_PureResolveType>();
            _unityContainer.RegisterType<NormalClassInheritedInj_Unity>();

            var builder = new StyletIoCBuilder();
            builder.Bind<Test_PureResolveType>().ToSelf();
            builder.Bind<NormalClassInheritedInj_Stylet>().ToSelf();
            _styletContainer = builder.BuildContainer();
        }

        [Benchmark]
        public NormalClassInheritedInj_Unity WithUnityContainer()
        {
            var resolved = _unityContainer.Resolve<NormalClassInheritedInj_Unity>();
            Debug.Assert(resolved.Test is Test_PureResolveType);
            return resolved;
        }

        [Benchmark]
        public NormalClassInheritedInj_Light WithLightContainer()
        {
            var resolved = _lightContainer.Resolve<NormalClassInheritedInj_Light>();
            Debug.Assert(resolved.Test is Test_PureResolveType);
            return resolved;
        }

        [Benchmark]
        public NormalClassInheritedInj_Stylet WithStyletContainer()
        {
            var resolved = _styletContainer.Get<NormalClassInheritedInj_Stylet>();
            Debug.Assert(resolved.Test is Test_PureResolveType);
            return resolved;
        }
    }

    public class BaseClassInheritedInj_Unity
    {
        [Dependency]
        public Test_PureResolveType Test { get; set; }
    }

    public class NormalClassInheritedInj_Unity : BaseClassInheritedInj_Unity
    {
    }

    public class BaseClassInheritedInj_Light
    {
        [Injection]
        public Test_PureResolveType Test { get; set; }
    }

    public class NormalClassInheritedInj_Light : BaseClassInheritedInj_Light
    {
    }

    public class BaseClassInheritedInj_Stylet
    {
        [Inject]
        public Test_PureResolveType Test { get; set; }
    }

    public class NormalClassInheritedInj_Stylet : BaseClassInheritedInj_Stylet
    {
    }
}
