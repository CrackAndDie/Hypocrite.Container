using Hypocrite.Container.Interfaces;
using Hypocrite.Container.Registrations;

namespace Hypocrite.Container
{
    public class LightContainer : ILightContainer
    {
        private readonly Workspace _workspace;
        public LightContainer() 
        {
            _workspace = new Workspace(this);
        }

        public bool IsRegistered(Type type)
        {
            return _workspace.IsRegistered(type, string.Empty);
        }

        public bool IsRegistered<TFrom>()
        {
            return _workspace.IsRegistered(typeof(TFrom), string.Empty);
        }

        public bool IsRegistered(Type type, string name)
        {
            return _workspace.IsRegistered(type, name);
        }

        public bool IsRegistered<TFrom>(string name)
        {
            return _workspace.IsRegistered(typeof(TFrom), name);
        }


        public void Register(Type fromT, Type toT)
        {
            _workspace.Register(new ContainerRegistration()
            {
                RegisteredType = fromT,
                MappedToType = toT,
                RegistrationType = RegistrationType.Type
            }, 
            string.Empty);
        }

        public void Register<TFrom, TTo>()
        {
            _workspace.Register(new ContainerRegistration()
            {
                RegisteredType = typeof(TFrom),
                MappedToType = typeof(TTo),
                RegistrationType = RegistrationType.Type
            },
            string.Empty);
        }

        public void Register(Type fromT, Type toT, string name)
        {
            _workspace.Register(new ContainerRegistration()
            {
                RegisteredType = fromT,
                MappedToType = toT,
                RegistrationType = RegistrationType.Type
            },
            name);
        }

        public void Register<TFrom, TTo>(string name)
        {
            _workspace.Register(new ContainerRegistration()
            {
                RegisteredType = typeof(TFrom),
                MappedToType = typeof(TTo),
                RegistrationType = RegistrationType.Type
            },
            name);
        }

        public void RegisterFactory(Type fromT, Func<ILightContainer, Type, string, object> factory)
        {
            _workspace.Register(new ContainerRegistration()
            {
                RegisteredType = fromT,
                Factory = factory,
                RegistrationType = RegistrationType.Factory
            },
            string.Empty);
        }

        public void RegisterFactory<TFrom>(Func<ILightContainer, Type, string, object> factory)
        {
            _workspace.Register(new ContainerRegistration()
            {
                RegisteredType = typeof(TFrom),
                Factory = factory,
                RegistrationType = RegistrationType.Factory
            },
            string.Empty);
        }

        public void RegisterFactory(Type fromT, string name, Func<ILightContainer, Type, string, object> factory)
        {
            _workspace.Register(new ContainerRegistration()
            {
                RegisteredType = fromT,
                Factory = factory,
                RegistrationType = RegistrationType.Factory
            },
            name);
        }

        public void RegisterFactory<TFrom>(string name, Func<ILightContainer, Type, string, object> factory)
        {
            _workspace.Register(new ContainerRegistration()
            {
                RegisteredType = typeof(TFrom),
                Factory = factory,
                RegistrationType = RegistrationType.Factory
            },
            name);
        }

        public void RegisterInstance(Type fromT, object instance)
        {
            _workspace.Register(new ContainerRegistration()
            {
                RegisteredType = fromT,
                Instance = instance,
                RegistrationType = RegistrationType.Instance
            },
            string.Empty);
        }

        public void RegisterInstance<TFrom>(object instance)
        {
            _workspace.Register(new ContainerRegistration()
            {
                RegisteredType = typeof(TFrom),
                Instance = instance,
                RegistrationType = RegistrationType.Instance
            },
            string.Empty);
        }

        public void RegisterInstance(Type fromT, string name, object instance)
        {
            _workspace.Register(new ContainerRegistration()
            {
                RegisteredType = fromT,
                Instance = instance,
                RegistrationType = RegistrationType.Instance
            },
            name);
        }

        public void RegisterInstance<TFrom>(string name, object instance)
        {
            _workspace.Register(new ContainerRegistration()
            {
                RegisteredType = typeof(TFrom),
                Instance = instance,
                RegistrationType = RegistrationType.Instance
            },
            name);
        }

        public void RegisterSingleton(Type fromT, Type toT)
        {
            _workspace.Register(new ContainerRegistration()
            {
                RegisteredType = fromT,
                MappedToType = toT,
                RegistrationType = RegistrationType.Instance
            },
            string.Empty);
        }

        public void RegisterSingleton<TFrom, TTo>()
        {
            _workspace.Register(new ContainerRegistration()
            {
                RegisteredType = typeof(TFrom),
                MappedToType = typeof(TTo),
                RegistrationType = RegistrationType.Instance
            },
            string.Empty);
        }

        public void RegisterSingleton(Type fromT, Type toT, string name)
        {
            _workspace.Register(new ContainerRegistration()
            {
                RegisteredType = fromT,
                MappedToType = toT,
                RegistrationType = RegistrationType.Instance
            },
            name);
        }

        public void RegisterSingleton<TFrom, TTo>(string name)
        {
            _workspace.Register(new ContainerRegistration()
            {
                RegisteredType = typeof(TFrom),
                MappedToType = typeof(TTo),
                RegistrationType = RegistrationType.Instance
            },
            name);
        }


        public object Resolve(Type type)
        {
            return _workspace.Resolve(type, string.Empty);
        }

        public T Resolve<T>()
        {
            return (T)_workspace.Resolve(typeof(T), string.Empty);
        }

        public object Resolve(Type type, string name)
        {
            return _workspace.Resolve(type, name);
        }

        public T Resolve<T>(string name)
        {
            return (T)_workspace.Resolve(typeof(T), name);
        }
    }
}
