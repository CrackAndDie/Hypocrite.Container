using Hypocrite.Container.Common;
using Hypocrite.Container.Extensions;
using Hypocrite.Container.Interfaces;
using Hypocrite.Container.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Hypocrite.Container.Creators
{
    internal static class Creator
    {
        internal static CreationInfo GetCreationInfo(Type type, bool skipCtor)
        {
            // create new one
            var info = new CreationInfo();

            // if not skip ctor info
            if (!skipCtor)
            {
                ConstructorInfo ctor = GetCtor(type, out InjectionElement[] pars);
                var lambda = InstanceCreator.CreateLambda(ctor);

                // save info 
                info.CtorData = Tuple.Create(ctor, pars);
                info.CtorLambda = lambda;
            }

            // props/fields
            {
                GetPropsAndFields(type, out InjectionElement[] propsAndFields);
                info.PropsAndFieldsData = propsAndFields;
                if (propsAndFields.Length > 0)
                {
                    var inj = PropsAndFieldsInjector.CreateInjector(type, propsAndFields.Select(x => x.Name).ToArray());
                    info.PropsAndFieldsInjector = inj;
                }
            }

            // methods
            {
                GetMethods(type, out Dictionary<string, InjectionElement[]> methods);
                info.MethodsData = methods;
                if (methods.Count > 0)
                {
                    // gen data
                    Dictionary<string, object[]> data = new Dictionary<string, object[]>(methods.Count);
                    foreach (var mtd in methods) data.Add(mtd.Key, mtd.Value);

                    var inj = MethodsInjector.CreateInjector(type, data);
                    info.MethodsInjector = inj;
                }
            }
            return info;
        }

        internal static object Create(CreationInfo info, ILightContainer container)
        {
            // creating an object
            object[] ctorArgs = GetArguments(container, info.CtorData.Item2);
            var obj = info.CtorLambda.Invoke(ctorArgs);
            return obj;
        }

        internal static void Inject(object instance, CreationInfo info, ILightContainer container)
        {
            // props/fields
            {
                var data = info.PropsAndFieldsData;
                var dataLen = data.Length;
                if (dataLen > 0)
                {
                    object[] args = GetArguments(container, info.PropsAndFieldsData);
                    // gen data
                    Dictionary<string, object> dt = new Dictionary<string, object>(dataLen);
                    for (int i = 0; i < dataLen; ++i)
                        dt.Add(data[i].Name, args[i]);

                    info.PropsAndFieldsInjector.Invoke(instance, dt);
                }
            }
            // nethods
            {
                var data = info.MethodsData;
                var dataLen = data.Count;
                if (dataLen > 0)
                {
                    // gen data
                    Dictionary<string, object[]> dt = new Dictionary<string, object[]>(dataLen);
                    foreach (var mtd in data)
                        dt.Add(mtd.Key, GetArguments(container, mtd.Value));

                    info.MethodsInjector.Invoke(instance, dt);
                }
            }
        }

        private static ConstructorInfo GetCtor(Type type, out InjectionElement[] pars)
        {
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

                // just any ctor pls
                if (ctor == null)
                    ctor = type.GetConstructors().FirstOrDefault();

                if (ctor == null)
                    throw new EntryPointNotFoundException($"Ctor of {type.GetDescription()} that could be used for creation could not be found");
            }
            pars = ctor.GetParameters().Select(x => InjectionElement.FromParameterInfo(x)).ToArray();
            return ctor;
        }

        private static void GetPropsAndFields(Type type, out InjectionElement[] propsAndFields)
        {
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
        }

        private static void GetMethods(Type type, out Dictionary<string, InjectionElement[]> methods)
        {
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
        }
        
        private static readonly object[] _constEmptyObjectArray = Array.Empty<object>();
        private static object[] GetArguments(ILightContainer container, InjectionElement[] pars)
        {
            if (pars.Length == 0)
                return _constEmptyObjectArray;

            object[] args = new object[pars.Length];
            InjectionElement par;
            object arg;
            for (int i = 0; i < pars.Length; ++i)
            {
                par = pars[i];
                arg = container.Resolve(par.ElementType);

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
