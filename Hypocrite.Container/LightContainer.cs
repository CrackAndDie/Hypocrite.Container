using Hypocrite.Container.Interfaces;

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
            return _workspace.IsRegistered<type>();
        }

        public bool IsRegistered<TFrom>()
        {
            throw new NotImplementedException();
        }

        public bool IsRegistered(Type type, string name)
        {
            throw new NotImplementedException();
        }

        public bool IsRegistered<TFrom>(string name)
        {
            throw new NotImplementedException();
        }


        public void Register(Type fromT, Type toT)
        {
            throw new NotImplementedException();
        }

        public void Register<TFrom, TTo>()
        {
            throw new NotImplementedException();
        }

        public void Register(Type fromT, Type toT, string name)
        {
            throw new NotImplementedException();
        }

        public void Register<TFrom, TTo>(string name)
        {
            throw new NotImplementedException();
        }

        public void RegisterFactory(Type fromT, Func<ILightContainer, Type, string, object> factory)
        {
            throw new NotImplementedException();
        }

        public void RegisterFactory<TFrom>(Func<ILightContainer, Type, string, object> factory)
        {
            throw new NotImplementedException();
        }

        public void RegisterFactory(Type fromT, string name, Func<ILightContainer, Type, string, object> factory)
        {
            throw new NotImplementedException();
        }

        public void RegisterFactory<TFrom>(string name, Func<ILightContainer, Type, string, object> factory)
        {
            throw new NotImplementedException();
        }

        public void RegisterInstance(Type fromT, object instance)
        {
            throw new NotImplementedException();
        }

        public void RegisterInstance<TFrom, TTo>(TTo instance)
        {
            throw new NotImplementedException();
        }

        public void RegisterInstance(Type fromT, string name, object instance)
        {
            throw new NotImplementedException();
        }

        public void RegisterInstance<TFrom, TTo>(string name, TTo instance)
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton(Type fromT, Type toT)
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton<TFrom, TTo>()
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton(Type fromT, Type toT, string name)
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton<TFrom, TTo>(string name)
        {
            throw new NotImplementedException();
        }


        public object Resolve(Type type)
        {
            throw new NotImplementedException();
        }

        public T Resolve<T>()
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type type, string name)
        {
            throw new NotImplementedException();
        }

        public T Resolve<T>(string name)
        {
            throw new NotImplementedException();
        }
    }
}
