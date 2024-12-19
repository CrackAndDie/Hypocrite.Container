namespace Hypocrite.Container.Interfaces
{
	public interface ILightContainer
	{
        // Registrations
        void Register(Type fromT, Type toT);
        void Register<TFrom, TTo>();
        void Register(Type fromT, Type toT, string name);
        void Register<TFrom, TTo>(string name);
        // Singleton
        void RegisterSingleton(Type fromT, Type toT);
        void RegisterSingleton<TFrom, TTo>();
        void RegisterSingleton(Type fromT, Type toT, string name);
        void RegisterSingleton<TFrom, TTo>(string name);
        // Instance
        void RegisterInstance(Type fromT, object instance);
        void RegisterInstance<TFrom, TTo>(TTo instance);
        void RegisterInstance(Type fromT, string name, object instance);
        void RegisterInstance<TFrom, TTo>(string name, TTo instance);
        // Factory
        void RegisterFactory(Type fromT, Func<ILightContainer, Type, string, object> factory);
        void RegisterFactory<TFrom>(Func<ILightContainer, Type, string, object> factory);
        void RegisterFactory(Type fromT, string name, Func<ILightContainer, Type, string, object> factory);
        void RegisterFactory<TFrom>(string name, Func<ILightContainer, Type, string, object> factory);

        // Resolves
        object Resolve(Type type);
        T Resolve<T>();
        object Resolve(Type type, string name);
        T Resolve<T>(string name);
    }
}
