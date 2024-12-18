using Hypocrite.Container.Extensions;
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
            object[] args = new object[pars.Length];
            for (int i = 0; i < pars.Length; ++i)
            {
                var par = pars[i];
                var arg = GetArgument(container, par.ParameterType);

                // if there was no the Type in container and param has default value - apply it
                if (arg == null && par.HasDefaultValue)
                    arg = par.DefaultValue;
                // if tehre was no the Type in container - use default
                else if (arg == null)
                    arg = par.ParameterType.GetDefaultValue();
                args[i] = arg;
            }
            return args;
        }

        private static object GetArgument(ILightContainer container, Type type)
        {
            try
            {
                return container.Resolve(type);
            }
            catch (KeyNotFoundException)
            {
                // if the entry not found
                return null;
            }
        }
    }
}
