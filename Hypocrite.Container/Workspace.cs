using Hypocrite.Container.Common;
using Hypocrite.Container.Creators;
using Hypocrite.Container.Extensions;
using Hypocrite.Container.Interfaces;
using Hypocrite.Container.Registrations;
using System;
using System.Collections.Generic;

namespace Hypocrite.Container
{
    internal class Workspace : IDisposable
    {
        private readonly ILightContainer _parent;
        private readonly QuickSet<CreationInfo> _creationInfo = new QuickSet<CreationInfo>();
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
                // getting info of the shite
                var info = _creationInfo.Get(hashCode, name);
                // check for existance
                if (info == null)
                {
                    info = Creator.GetCreationInfo(registration.MappedToType, false);
                    _creationInfo.AddOrReplace(hashCode, name, info);
                }

                // create an instance
                registration.Instance = Creator.Create(info, _parent);

                // inject the shite
                Creator.Inject(registration.Instance, info, _parent);
            }
            else
            {
                // create the instance
                registration.Instance = registration.Factory.Invoke(_parent, registration.RegisteredType, name);

                // getting info of the shite
                var info = _creationInfo.Get(hashCode, name);
                // check for existance
                if (info == null && registration.Instance != null)
                {
                    info = Creator.GetCreationInfo(registration.Instance.GetType(), true);
                    _creationInfo.AddOrReplace(hashCode, name, info);
                }

                // inject the shite
                Creator.Inject(registration.Instance, info, _parent);
            }

            // reset cache if not instance
            var result = registration.Instance;
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
