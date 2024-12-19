using Hypocrite.Container.Extensions;
using Hypocrite.Container.Interfaces;
using System.Reflection;

namespace Hypocrite.Container.Creators
{
    internal static class Creator
    {
        internal static T Create<T>(ILightContainer container)
        {
            ConstructorInfo ctor = GetCtor<T>(out InjectionElement[] pars);
            object[] args = GetArguments(container, pars);
            T instance = InstanceCreator.CreateWithParams<T>(ctor, args);
            return instance;
        }

        internal static void InjectPropsAndFields<T>(T instance, ILightContainer container)
        {
            GetPropsAndFields<T>(out InjectionElement[] propsAndFields);
            object[] args = GetArguments(container, propsAndFields);

            // gen data
            Dictionary<string, object> data = new Dictionary<string, object>(propsAndFields.Length);
            for (int i = 0; i < propsAndFields.Length; ++i)
                data.Add(propsAndFields[i].Name, args[i]);

            PropsAndFieldsInjector.Inject(instance, data);
        }

        internal static void InjectMethods<T>(T instance, ILightContainer container)
        {
            GetMethods<T>(out Dictionary<string, InjectionElement[]> methods);

            // gen data
            Dictionary<string, object[]> data = new Dictionary<string, object[]>(methods.Count);
            foreach (var mtd in methods)
                data.Add(mtd.Key, GetArguments(container, mtd.Value));


        }

        private static readonly Dictionary<Type, (ConstructorInfo, InjectionElement[])> _cachedCtors = new Dictionary<Type, (ConstructorInfo, InjectionElement[])>();
        private static ConstructorInfo GetCtor<T>(out InjectionElement[] pars)
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
                throw new AmbiguousMatchException($"Found more than one ctor with [InjectionAttribute] in {typeof(T).GetDescription()}");
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
                    throw new EntryPointNotFoundException($"Ctor of {typeof(T).GetDescription()} that could be used for creation could not be found");
            }
            pars = ctor.GetParameters().Select(x => InjectionElement.FromParameterInfo(x)).ToArray();
            // caching
            _cachedCtors.Add(typeof(T), (ctor, pars));
            return ctor;
        }

        private static readonly Dictionary<Type, InjectionElement[]> _cachedPropsAndFields = new Dictionary<Type, InjectionElement[]>();
        private static void GetPropsAndFields<T>(out InjectionElement[] propsAndFields)
        {
            // check for cache
            if (_cachedPropsAndFields.TryGetValue(typeof(T), out var cachedElements))
            {
                propsAndFields = cachedElements;
                return;
            }

            List<InjectionElement> elements = new List<InjectionElement>();
            // props shite
            var propertyInfos = typeof(T).GetTypeInfo().DeclaredProperties.Where(x => x.GetCustomAttribute<InjectionAttribute>(true) != null);
            foreach (var propertyInfo in propertyInfos)
            {
                if (!propertyInfo.CanWrite)
                    throw new MemberAccessException($"Property {typeof(T).GetDescription()}.{propertyInfo.Name} has to be writable");
                elements.Add(InjectionElement.FromPropertyInfo(propertyInfo));
            }
            // fields shite
            var fieldInfos = typeof(T).GetTypeInfo().DeclaredFields.Where(x => x.GetCustomAttribute<InjectionAttribute>(true) != null);
            foreach (var fieldInfo in fieldInfos)
            {
                elements.Add(InjectionElement.FromFieldInfo(fieldInfo));
            }
            propsAndFields = elements.ToArray();
            // caching
            _cachedPropsAndFields.Add(typeof(T), propsAndFields);
        }

        private static readonly Dictionary<Type, Dictionary<string, InjectionElement[]>> _cachedMethods = new Dictionary<Type, Dictionary<string, InjectionElement[]>>();
        private static void GetMethods<T>(out Dictionary<string, InjectionElement[]> methods)
        {
            // check for cache
            if (_cachedMethods.TryGetValue(typeof(T), out var cachedMethods))
            {
                methods = cachedMethods;
                return;
            }

            Dictionary<string, InjectionElement[]> elements = new Dictionary<string, InjectionElement[]>();
            var methodInfos = typeof(T).GetTypeInfo().DeclaredMethods.Where(x => x.GetCustomAttribute<InjectionAttribute>(true) != null);
            foreach (var methodInfo in methodInfos)
            {
                var methodPars = methodInfo.GetParameters();
                InjectionElement[] pars = new InjectionElement[methodPars.Length];
                for (int i = 0; i < methodPars.Length; ++i)
                {
                    pars[i] = InjectionElement.FromParameterInfo(methodPars[i]);
                }
                elements.Add(methodInfo.Name, pars);
            }
            methods = elements;
            // caching
            _cachedMethods.Add(typeof(T), methods);
        }

        private static object[] GetArguments(ILightContainer container, InjectionElement[] pars)
        {
            object[] args = new object[pars.Length];
            for (int i = 0; i < pars.Length; ++i)
            {
                var par = pars[i];
                var arg = GetArgument(container, par.ElementType);

                // if there was no the Type in container and param has default value - apply it
                if (arg == null && par.HasDefaultValue)
                    arg = par.DefaultValue;
                // if tehre was no the Type in container - use default
                else if (arg == null)
                    arg = par.ElementType.GetDefaultValue();
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

        /// <summary>
        /// Common class for params/fields/props
        /// </summary>
        private class InjectionElement
        {
            public string Name { get; set; }
            public bool HasDefaultValue { get; set; }
            public object DefaultValue { get; set; }
            public Type ElementType { get; set; }

            public static InjectionElement FromParameterInfo(ParameterInfo parameterInfo)
            {
                return new InjectionElement()
                {
                    Name = parameterInfo.Name,
                    HasDefaultValue = parameterInfo.HasDefaultValue,
                    DefaultValue = parameterInfo.DefaultValue,
                    ElementType = parameterInfo.ParameterType,
                };
            }

            public static InjectionElement FromPropertyInfo(PropertyInfo propertyInfo)
            {
                return new InjectionElement()
                {
                    Name = propertyInfo.Name,
                    HasDefaultValue = false,
                    DefaultValue = null,
                    ElementType = propertyInfo.PropertyType,
                };
            }

            public static InjectionElement FromFieldInfo(FieldInfo fieldInfo)
            {
                return new InjectionElement()
                {
                    Name = fieldInfo.Name,
                    HasDefaultValue = false,
                    DefaultValue = null,
                    ElementType = fieldInfo.FieldType,
                };
            }
        }
    }
}
