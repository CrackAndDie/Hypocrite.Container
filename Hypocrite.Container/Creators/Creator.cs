using Hypocrite.Container.Interfaces;
using System.Reflection;

namespace Hypocrite.Container.Creators
{
    internal static class Creator
    {
        internal static T Create<T>(ILightContainer container)
        {
            ConstructorInfo ctor = GetCtor<T>(out ParameterInfo[] pars);
            object[] args = GetArguments(container, pars);
            T instance = InstanceCreator.CreateWithParams<T>(ctor, args);
            return instance;
        }

        private static readonly Dictionary<Type, (ConstructorInfo, ParameterInfo[])> _cachedCtors = new Dictionary<Type, (ConstructorInfo, ParameterInfo[])>();
        private static ConstructorInfo GetCtor<T>(out ParameterInfo[] pars)
        {
            // check for cache
            if (_cachedCtors.TryGetValue(typeof(T), out var cachedCtor))
            {
                pars = cachedCtor.Item2;
                return cachedCtor.Item1;
            }

            ConstructorInfo ctor;
            var ctorsWithAttribute = typeof(T).GetConstructors().Where(x => x.GetCustomAttribute<InjectionAttribute>(true) != null).ToList();
            if (ctorsWithAttribute.Count > 1)
            {
                throw new AmbiguousMatchException($"Found more than one ctor with [InjectionAttribute]");
            }
            else if (ctorsWithAttribute.Count == 1)
            {
                ctor = ctorsWithAttribute[0];
            }
            else
            {
                // Search for parameterless ctor
                ctor = typeof(T).GetConstructors()
                    .Where(c => c.GetParameters().Length == 0)
                    .FirstOrDefault();

                if (ctor == null)
                    throw new EntryPointNotFoundException($"Ctor that should be used for creation could not be found");
            }
            pars = ctor.GetParameters();
            return ctor;
        }

        private static object[] GetArguments(ILightContainer container, ParameterInfo[] pars)
        {
            return new object[0];
        }
    }
}
