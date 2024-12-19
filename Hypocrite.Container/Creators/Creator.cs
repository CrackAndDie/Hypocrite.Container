﻿using Hypocrite.Container.Extensions;
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
            PropsAndFieldsInjector.Inject(instance, );
        }

        internal static void InjectMethods(ILightContainer container)
        {

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
