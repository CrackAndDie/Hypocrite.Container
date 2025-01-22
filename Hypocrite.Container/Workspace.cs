using Hypocrite.Container.Common;
using Hypocrite.Container.Creators;
using Hypocrite.Container.Extensions;
using Hypocrite.Container.Interfaces;
using Hypocrite.Container.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;
using static Hypocrite.Container.Creators.Creator;

namespace Hypocrite.Container
{
    internal class Workspace : IDisposable
    {
        private readonly ILightContainer _parent;
        private readonly QuickSet<ContainerRegistration> _registrations = new QuickSet<ContainerRegistration>();

        internal Workspace(ILightContainer parent)
        {
            _parent = parent;
        }

        internal void Register(ContainerRegistration registation, string name)
        {
            _registrations.AddOrReplace(registation.RegisteredType.GetHashCode(), name, registation);
        }

        internal object Resolve(Type type, string name)
        {
            int hashCode = type.GetHashCode();
            var registration = _registrations.Get(hashCode, name);
            if (registration == null)
                throw new KeyNotFoundException($"Registration for type {type.GetDescription()} with name {name} could not be found");

            // this is a cache for recursive resolve
            if (registration.RegistrationType == RegistrationType.Type && registration.Instance != null)
                return registration.Instance;

            // this is a singleton/instance
            if (registration.RegistrationType == RegistrationType.Instance && registration.Instance != null)
                return registration.Instance;

            // upper checks could be collaps but don't do this because of readability

            // creating an instance and injecting everything
            if (registration.RegistrationType != RegistrationType.Factory)
            {
                ConstructorInfo ctor = GetCtor(type, hashCode, out InjectionElement[] pars);
                object[] args = GetArguments(_parent, pars);
                registration.Instance = InstanceCreator.CreateWithParams(hashCode, ctor, args);
            }
            else
                registration.Instance = registration.Factory.Invoke(_parent, registration.RegisteredType, name);

            // injecting
            var result = registration.Instance;

            // props/fields
            {
                GetPropsAndFields(type, hashCode, out InjectionElement[] propsAndFields);
                if (propsAndFields.Length > 0)
                {
                    object[] args = GetArguments(_parent, propsAndFields);

                    // gen data
                    Dictionary<string, object> data = new Dictionary<string, object>(propsAndFields.Length);
                    for (int i = 0; i < propsAndFields.Length; ++i)
                        data.Add(propsAndFields[i].Name, args[i]);

                    PropsAndFieldsInjector.Inject(type, hashCode, result, data);
                }
            }
            // nethods
            {
                GetMethods(type, hashCode, out Dictionary<string, InjectionElement[]> methods);
                if (methods.Count > 0)
                {
                    // gen data
                    Dictionary<string, object[]> data = new Dictionary<string, object[]>(methods.Count);
                    foreach (var mtd in methods)
                        data.Add(mtd.Key, GetArguments(_parent, mtd.Value));

                    MethodsInjector.Inject(type, hashCode, result, data);
                }
            }
            
            // reset cache if not instance
            if (registration.RegistrationType != RegistrationType.Instance)
                registration.Instance = null;

            return result;
        }

        internal bool IsRegistered(Type type, string name)
        {
            var entry = _registrations.Get(type.GetHashCode(), name);
            return entry != null;
        }

        internal Type GetRegistrationType(Type type, string name)
        {
            if (type == null)
                return _registrations.Get(0, name)?.MappedToType;
            else
                return _registrations.Get(type.GetHashCode(), name)?.MappedToType;
        }

        public void Dispose()
        {
            _registrations.Clear();
        }
    }
}
