using BenchmarkDotNet.Attributes;
using Hypocrite.Container;
using Hypocrite.Container.Interfaces;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Hypocrite.Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class ResolveRecursiveInj
    {
        ILightContainer _lightContainer;
        IUnityContainer _unityContainer;
        IContainer _styletContainer;

        public ResolveRecursiveInj()
        {
            _lightContainer = new LightContainer();
            _lightContainer.Register<FirstClassRecursive_Light, FirstClassRecursive_Light>();
            _lightContainer.Register<SecondClassRecursive_Light, SecondClassRecursive_Light>();

            _unityContainer = new UnityContainer();
            _unityContainer.RegisterType<FirstClassRecursive_Unity>();
            _unityContainer.RegisterType<SecondClassRecursive_Unity>();

            var builder = new StyletIoCBuilder();
            builder.Bind<FirstClassRecursive_Stylet>().ToSelf();
            builder.Bind<SecondClassRecursive_Stylet>().ToSelf();
            _styletContainer = builder.BuildContainer();
        }

        [Benchmark]
        public FirstClassRecursive_Unity WithUnityContainer()
        {
            var resolved = _unityContainer.Resolve<FirstClassRecursive_Unity>();
            Debug.Assert(resolved.InjectedClass is SecondClassRecursive_Unity);
            Debug.Assert(resolved.InjectedClass.InjectedClass is FirstClassRecursive_Unity);
            return resolved;
        }

        [Benchmark]
        public FirstClassRecursive_Light WithLightContainer()
        {
            var resolved = _lightContainer.Resolve<FirstClassRecursive_Light>();
            Debug.Assert(resolved.InjectedClass is SecondClassRecursive_Light);
            Debug.Assert(resolved.InjectedClass.InjectedClass is FirstClassRecursive_Light);
            return resolved;
        }

        [Benchmark]
        public FirstClassRecursive_Stylet WithStyletContainer()
        {
            var resolved = _styletContainer.Get<FirstClassRecursive_Stylet>();
            Debug.Assert(resolved.InjectedClass is SecondClassRecursive_Stylet);
            Debug.Assert(resolved.InjectedClass.InjectedClass is FirstClassRecursive_Stylet);
            return resolved;
        }
    }

    public class FirstClassRecursive_Unity
    {
        [Dependency]
        public SecondClassRecursive_Unity InjectedClass { get; set; }
    }
    public class SecondClassRecursive_Unity
    {
        [Dependency]
        public FirstClassRecursive_Unity InjectedClass { get; set; }
    }

    public class FirstClassRecursive_Light
    {
        [Injection]
        public SecondClassRecursive_Light InjectedClass { get; set; }
    }
    public class SecondClassRecursive_Light
    {
        [Injection]
        public FirstClassRecursive_Light InjectedClass { get; set; }
    }

    public class FirstClassRecursive_Stylet
    {
        [Inject]
        public SecondClassRecursive_Stylet InjectedClass { get; set; }
    }
    public class SecondClassRecursive_Stylet
    {
        [Inject]
        public FirstClassRecursive_Stylet InjectedClass { get; set; }
    }
}
