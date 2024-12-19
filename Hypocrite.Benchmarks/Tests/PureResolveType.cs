﻿using BenchmarkDotNet.Attributes;
using Hypocrite.Container;
using Hypocrite.Container.Interfaces;
using Ninject;
using StyletIoC;
using Unity;

namespace Hypocrite.Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class PureResolveType
    {
        ILightContainer _lightContainer;
        IUnityContainer _unityContainer;
        IKernel _ninjectContainer;
        IContainer _styletContainer;

        public PureResolveType()
        {
            _lightContainer = new LightContainer();
            _lightContainer.Register<Test_PureResolveType, Test_PureResolveType>();

            _unityContainer = new UnityContainer();
            _unityContainer.RegisterType<Test_PureResolveType>();

            _ninjectContainer = new StandardKernel();
            _ninjectContainer.Bind<Test_PureResolveType>().ToSelf();

            var builder = new StyletIoCBuilder();
            builder.Bind<Test_PureResolveType>().ToSelf();
            _styletContainer = builder.BuildContainer();
        }

        [Benchmark]
        public Test_PureResolveType WithUnityContainer()
        {
            return _unityContainer.Resolve<Test_PureResolveType>();
        }

        [Benchmark]
        public Test_PureResolveType WithLightContainer()
        {
            return _lightContainer.Resolve<Test_PureResolveType>();
        }

        [Benchmark]
        public Test_PureResolveType WithNinjectContainer()
        {
            return _ninjectContainer.Get<Test_PureResolveType>();
        }

        [Benchmark]
        public Test_PureResolveType WithStyletContainer()
        {
            return _styletContainer.Get<Test_PureResolveType>();
        }
    }

    public class Test_PureResolveType
    {
        public int A { get; set; }
        public string B { get; set; }
    }
}
