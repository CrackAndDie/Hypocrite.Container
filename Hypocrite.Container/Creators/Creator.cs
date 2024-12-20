using Hypocrite.Container.Common;
using Hypocrite.Container.Extensions;
using Hypocrite.Container.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hypocrite.Container.Creators
{
    internal static class Creator
    {
        private static readonly Dictionary<int, (ConstructorInfo, InjectionElement[])> _cachedCtors = new Dictionary<int, (ConstructorInfo, InjectionElement[])>();
        internal static ConstructorInfo GetCtor(Type type, int hash, out InjectionElement[] pars)
        {
            // check for cache
            if (_cachedCtors.TryGetValue(hash, out var value))
            {
                pars = value.Item2;
                return value.Item1;
            }

            ConstructorInfo ctor;
            var ctorsWithAttribute = type.GetConstructors().Where(x => x.GetCustomAttribute<InjectionAttribute>(true) != null).ToList();
            if (ctorsWithAttribute.Count > 1)
            {
                throw new AmbiguousMatchException($"Found more than one ctor with [InjectionAttribute] in {type.GetDescription()}");
            }
            else if (ctorsWithAttribute.Count == 1)
            {
                ctor = ctorsWithAttribute[0];
            }
            else
            {
                // Search for parameterless ctor
                ctor = type.GetConstructors()
                    .Where(c => c.GetParameters().Length == 0)
                    .FirstOrDefault();

                if (ctor == null)
                    throw new EntryPointNotFoundException($"Ctor of {type.GetDescription()} that could be used for creation could not be found");
            }
            pars = ctor.GetParameters().Select(x => InjectionElement.FromParameterInfo(x)).ToArray();
            // caching
            _cachedCtors.Add(hash, (ctor, pars));
            return ctor;
        }

        private static readonly Dictionary<int, InjectionElement[]> _cachedPropsAndFields = new Dictionary<int, InjectionElement[]>();
        internal static void GetPropsAndFields(Type type, int hash, out InjectionElement[] propsAndFields)
        {
            // check for cache
            if (_cachedPropsAndFields.TryGetValue(hash, out var value))
            {
                propsAndFields = value;
                return;
            }

            List<InjectionElement> elements = new List<InjectionElement>();
            // props shite
            var propertyInfos = type.GetTypeInfo().DeclaredProperties.Where(x => x.GetCustomAttribute<InjectionAttribute>(true) != null);
            foreach (var propertyInfo in propertyInfos)
            {
                if (!propertyInfo.CanWrite)
                    throw new MemberAccessException($"Property {type.GetDescription()}.{propertyInfo.Name} has to be writable");
                elements.Add(InjectionElement.FromPropertyInfo(propertyInfo));
            }
            // fields shite
            var fieldInfos = type.GetTypeInfo().DeclaredFields.Where(x => x.GetCustomAttribute<InjectionAttribute>(true) != null);
            foreach (var fieldInfo in fieldInfos)
            {
                elements.Add(InjectionElement.FromFieldInfo(fieldInfo));
            }
            propsAndFields = elements.ToArray();
            // caching
            _cachedPropsAndFields.Add(hash, propsAndFields);
        }

        private static readonly Dictionary<int, Dictionary<string, InjectionElement[]>> _cachedMethods = new Dictionary<int, Dictionary<string, InjectionElement[]>>();
        internal static void GetMethods(Type type, int hash, out Dictionary<string, InjectionElement[]> methods)
        {
            // check for cache
            if (_cachedMethods.TryGetValue(hash, out var value))
            {
                methods = value;
                return;
            }

            Dictionary<string, InjectionElement[]> elements = new Dictionary<string, InjectionElement[]>();
            var methodInfos = type.GetTypeInfo().DeclaredMethods.Where(x => x.GetCustomAttribute<InjectionAttribute>(true) != null);
            ParameterInfo[] methodPars;
            InjectionElement[] injectionPars;
            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo.IsStatic)
                    throw new MemberAccessException($"Methods with [InjectionAttribute] could not be static: {type.GetDescription()}.{methodInfo.Name}");

                methodPars = methodInfo.GetParameters();
                injectionPars = new InjectionElement[methodPars.Length];
                for (int i = 0; i < methodPars.Length; ++i)
                {
                    injectionPars[i] = InjectionElement.FromParameterInfo(methodPars[i]);
                }
                elements.Add(methodInfo.Name, injectionPars);
            }
            methods = elements;
            // caching
            _cachedMethods.Add(hash, methods);
        }
        
        private static readonly object[] _constEmptyObjectArray = Array.Empty<object>();
        internal static object[] GetArguments(ILightContainer container, InjectionElement[] pars)
        {
            if (pars.Length == 0)
                return _constEmptyObjectArray;

            object[] args = new object[pars.Length];
            InjectionElement par;
            object arg;
            for (int i = 0; i < pars.Length; ++i)
            {
                par = pars[i];
                arg = GetArgument(container, par.ElementType);

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
        internal class InjectionElement
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
