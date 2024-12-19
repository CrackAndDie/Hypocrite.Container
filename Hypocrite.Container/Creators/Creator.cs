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
        internal static object Create(Type type, int hash, ILightContainer container)
        {
            ConstructorInfo ctor = GetCtor(type, hash, out InjectionElement[] pars);
            object[] args = GetArguments(container, pars);
            object instance = InstanceCreator.CreateWithParams(type, ctor, args);
            return instance;
        }

        internal static void InjectPropsAndFields(Type type, int hash, object instance, ILightContainer container)
        {
            GetPropsAndFields(type, hash, out InjectionElement[] propsAndFields);
            if (propsAndFields.Length == 0)
                return;
            object[] args = GetArguments(container, propsAndFields);

            // gen data
            Dictionary<string, object> data = new Dictionary<string, object>(propsAndFields.Length);
            for (int i = 0; i < propsAndFields.Length; ++i)
                data.Add(propsAndFields[i].Name, args[i]);

            PropsAndFieldsInjector.Inject(type, instance, data);
        }

        internal static void InjectMethods(Type type, int hash, object instance, ILightContainer container)
        {
            GetMethods(type, hash, out Dictionary<string, InjectionElement[]> methods);
            if (methods.Count == 0)
                return;

            // gen data
            Dictionary<string, object[]> data = new Dictionary<string, object[]>(methods.Count);
            foreach (var mtd in methods)
                data.Add(mtd.Key, GetArguments(container, mtd.Value));

            MethodsInjector.Inject(type, instance, data);
        }

        private static readonly QuickSet<(ConstructorInfo, InjectionElement[])> _cachedCtors = new QuickSet<(ConstructorInfo, InjectionElement[])>();
        private static ConstructorInfo GetCtor(Type type, int hash, out InjectionElement[] pars)
        {
            // check for cache
            var entry = _cachedCtors.Get(hash, string.Empty);
            if (entry.HasValue)
            {
                pars = entry.Value.Value.Item2;
                return entry.Value.Value.Item1;
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
            _cachedCtors.AddOrReplace(hash, string.Empty, (ctor, pars));
            return ctor;
        }

        private static readonly QuickSet<InjectionElement[]> _cachedPropsAndFields = new QuickSet<InjectionElement[]>();
        private static void GetPropsAndFields(Type type, int hash, out InjectionElement[] propsAndFields)
        {
            // check for cache
            var entry = _cachedPropsAndFields.Get(hash, string.Empty);
            if (entry.HasValue)
            {
                propsAndFields = entry.Value.Value;
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
            _cachedPropsAndFields.AddOrReplace(hash, string.Empty, propsAndFields);
        }

        private static readonly QuickSet<Dictionary<string, InjectionElement[]>> _cachedMethods = new QuickSet<Dictionary<string, InjectionElement[]>>();
        private static void GetMethods(Type type, int hash, out Dictionary<string, InjectionElement[]> methods)
        {
            // check for cache
            var entry = _cachedMethods.Get(hash, string.Empty);
            if (entry.HasValue)
            {
                methods = entry.Value.Value;
                return;
            }

            Dictionary<string, InjectionElement[]> elements = new Dictionary<string, InjectionElement[]>();
            var methodInfos = type.GetTypeInfo().DeclaredMethods.Where(x => x.GetCustomAttribute<InjectionAttribute>(true) != null);
            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo.IsStatic)
                    throw new MemberAccessException($"Methods with [InjectionAttribute] could not be static: {type.GetDescription()}.{methodInfo.Name}");

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
            _cachedMethods.AddOrReplace(hash, string.Empty, methods);
        }

        private static object[] GetArguments(ILightContainer container, InjectionElement[] pars)
        {
            if (pars.Length == 0)
                return Array.Empty<object>();

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
