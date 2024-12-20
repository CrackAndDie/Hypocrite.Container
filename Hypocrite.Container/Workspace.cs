using Hypocrite.Container.Common;
using Hypocrite.Container.Creators;
using Hypocrite.Container.Extensions;
using Hypocrite.Container.Interfaces;
using Hypocrite.Container.Registrations;
using System;
using System.Collections.Generic;

namespace Hypocrite.Container
{
    internal class Workspace
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
                registration.Instance = Creator.Create(type, hashCode, _parent);
            else
                registration.Instance = registration.Factory.Invoke(_parent, registration.RegisteredType, name);

            // injecting
            var result = registration.Instance;
            Creator.InjectPropsAndFields(type, hashCode, result, _parent);
            Creator.InjectMethods(type, hashCode, result, _parent);
            
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
    }
}
