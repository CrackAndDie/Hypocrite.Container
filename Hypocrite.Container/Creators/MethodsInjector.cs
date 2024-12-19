namespace Hypocrite.Container.Creators
{
    internal static class MethodsInjector
    {
        private static readonly Dictionary<Type, Action<object, Dictionary<string, object[]>>> _cachedWithParams = new Dictionary<Type, Action<object, Dictionary<string, object[]>>>();
        internal static void Inject<T>(T instance, Dictionary<string, object[]> args)
        {
            Action<object, Dictionary<string, object[]>> injector;
            // check for cache
            if (_cachedWithParams.TryGetValue(typeof(T), out var cachedInjector))
            {
                injector = cachedInjector;
            }
            else
            {
                injector = GenerateFactoryWithParams<T>(args.Keys.ToArray());
                _cachedWithParams.Add(typeof(T), injector);
            }
            injector.Invoke(instance, args);
        }
    }
}
