using Hypocrite.Container.Interfaces;
using Prism.Ioc;
using Prism.Ioc.Internals;
using System;
using System.Windows.Input;
using System.Xml.Linq;

namespace Hypocrite.Container.Prism
{
    public class HypocriteContainerExtension : IContainerExtension<ILightContainer>, IContainerInfo
    {
        public ILightContainer Instance { get; }

        private IScopedProvider _currentScope;
        public IScopedProvider CurrentScope => _currentScope;

#if !ContainerExtensions
        /// <summary>
        /// Constructs a default <see cref="HypocriteContainerExtension" />
        /// </summary>
        public HypocriteContainerExtension()
            : this(new LightContainer())
        {
        }

        /// <summary>
        /// Constructs a <see cref="HypocriteContainerExtension" /> with the specified <see cref="ILightContainer" />
        /// </summary>
        /// <param name="container"></param>
        public HypocriteContainerExtension(ILightContainer container)
        {
            Instance = container;
            Instance.RegisterInstance(typeof(ILightContainer), Instance);
            Instance.RegisterInstance(this.GetType(), this);
            Instance.RegisterInstance(typeof(IContainerExtension), this);
            Instance.RegisterInstance(typeof(IContainerProvider), this);
        }
#endif

        public IScopedProvider CreateScope()
        {
            return CreateScopeInternal();
        }

        public void FinalizeExtension()
        {
        }

        public Type GetRegistrationType(string key)
        {
            return Instance.GetRegistrationType(null, key);
        }

        public Type GetRegistrationType(Type serviceType)
        {
            return Instance.GetRegistrationType(serviceType);
        }

        public bool IsRegistered(Type type)
        {
            return Instance.IsRegistered(type);
        }

        public bool IsRegistered(Type type, string name)
        {
            return Instance.IsRegistered(type, name);
        }

        public IContainerRegistry Register(Type from, Type to)
        {
            Instance.Register(from, to);
            return this;
        }

        public IContainerRegistry Register(Type from, Type to, string name)
        {
            Instance.Register(from, to, name);
            return this;
        }

        public IContainerRegistry Register(Type type, Func<object> factoryMethod)
        {
            Instance.RegisterFactory(type, (c, t, n) => { return factoryMethod(); });
            return this;
        }

        public IContainerRegistry Register(Type type, Func<IContainerProvider, object> factoryMethod)
        {
            Instance.RegisterFactory(type, (c, t, n) => { return factoryMethod(this); });
            return this;
        }

        public IContainerRegistry RegisterInstance(Type type, object instance)
        {
            Instance.RegisterInstance(type, instance);
            return this;
        }

        public IContainerRegistry RegisterInstance(Type type, object instance, string name)
        {
            Instance.RegisterInstance(type, name, instance);
            return this;
        }

        public IContainerRegistry RegisterMany(Type type, params Type[] serviceTypes)
        {
            throw new NotImplementedException();
        }

        public IContainerRegistry RegisterManySingleton(Type type, params Type[] serviceTypes)
        {
            throw new NotImplementedException();
        }

        public IContainerRegistry RegisterScoped(Type from, Type to)
        {
            throw new NotImplementedException();
        }

        public IContainerRegistry RegisterScoped(Type type, Func<object> factoryMethod)
        {
            throw new NotImplementedException();
        }

        public IContainerRegistry RegisterScoped(Type type, Func<IContainerProvider, object> factoryMethod)
        {
            throw new NotImplementedException();
        }

        public IContainerRegistry RegisterSingleton(Type from, Type to)
        {
            Instance.RegisterSingleton(from, to);
            return this;
        }

        public IContainerRegistry RegisterSingleton(Type from, Type to, string name)
        {
            Instance.RegisterSingleton(from, to, name);
            return this;
        }

        public IContainerRegistry RegisterSingleton(Type type, Func<object> factoryMethod)
        {
            throw new NotImplementedException();
        }

        public IContainerRegistry RegisterSingleton(Type type, Func<IContainerProvider, object> factoryMethod)
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type type)
        {
            return Instance.Resolve(type);
        }

        public object Resolve(Type type, params (Type Type, object Instance)[] parameters)
        {
            return Instance.Resolve(type);
        }

        public object Resolve(Type type, string name)
        {
            return Instance.Resolve(type, name);
        }

        public object Resolve(Type type, string name, params (Type Type, object Instance)[] parameters)
        {
            return Instance.Resolve(type, name);
        }

        protected IScopedProvider CreateScopeInternal()
        {
            var child = Instance;
            _currentScope = new HypocriteScopedProvider(child);
            return _currentScope;
        }

        private class HypocriteScopedProvider : IScopedProvider
        {
            public HypocriteScopedProvider(ILightContainer container)
            {
                Container = container;
            }

            public ILightContainer Container { get; private set; }

            public bool IsAttached { get; set; }

            public IScopedProvider CurrentScope => this;

            public IScopedProvider CreateScope()
            {
                return this;
            }

            public void Dispose()
            {
                Container.Dispose();
            }

            public object Resolve(Type type)
            {
                return Container.Resolve(type);
            }

            public object Resolve(Type type, params (Type Type, object Instance)[] parameters)
            {
                return Container.Resolve(type);
            }

            public object Resolve(Type type, string name)
            {
                return Container.Resolve(type, name);
            }

            public object Resolve(Type type, string name, params (Type Type, object Instance)[] parameters)
            {
                return Container.Resolve(type, name);
            }
        }
    }
}
