using BenchmarkDotNet.Attributes;
using Hypocrite.Container;
using Hypocrite.Container.Common;
using Hypocrite.Container.Creators;
using Hypocrite.Container.Interfaces;
using Hypocrite.Container.Registrations;
using System;
using System.Collections.Generic;

namespace Hypocrite.Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class ComponentsTest
    {
        ILightContainer _lightContainer;

        private readonly QuickSet<ContainerRegistration> _registrations = new QuickSet<ContainerRegistration>();

        public ComponentsTest()
        {
            _lightContainer = new LightContainer();
            _lightContainer.Register<Test_PureResolveType, Test_PureResolveType>();

            _registrations.AddOrReplace(typeof(Test_PureResolveType).GetHashCode(), "", new ContainerRegistration()
            {
                RegisteredType = typeof(Test_PureResolveType),
                MappedToType = typeof(Test_PureResolveType),
                RegistrationType = RegistrationType.Type,
            });
        }

        [Benchmark]
        public Test_PureResolveType WithLightContainer()
        {
            return _lightContainer.Resolve<Test_PureResolveType>();
        }

        [Benchmark]
        public Type WithTypeof()
        {
            return typeof(Test_PureResolveType);
        }

        [Benchmark]
        public int WithGetHashCode()
        {
            return typeof(Test_PureResolveType).GetHashCode();
        }

        [Benchmark]
        public object WithGetRegistration()
        {
            var type = typeof(Test_PureResolveType);


            int hashCode = type.GetHashCode();
            var registration = _registrations.Get(hashCode, "");
            if (registration == null)
                throw new KeyNotFoundException($"Registration for type with name could not be found");

            // this is a cache for recursive resolve
            if (registration.RegistrationType == RegistrationType.Type && registration.Instance != null)
                return registration.Instance;

            // this is a singleton/instance
            if (registration.RegistrationType == RegistrationType.Instance && registration.Instance != null)
                return registration.Instance;
            return null;
        }

        [Benchmark]
        public Test_PureResolveType WithCreateContainer()
        {
            return Creator.Create(typeof(Test_PureResolveType), typeof(Test_PureResolveType).GetHashCode(), null) as Test_PureResolveType;
        }
    }
}
