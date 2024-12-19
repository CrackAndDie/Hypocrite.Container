using Hypocrite.Container.Common;
using Hypocrite.Container.Creators;
using Hypocrite.Container.Extensions;
using Hypocrite.Container.Interfaces;
using Hypocrite.Container.Registrations;

namespace Hypocrite.Container
{
    internal class Workspace
    {
        private readonly ILightContainer _parent;
        private QuickSet<ContainerRegistration> _registrations { get; set; } = new QuickSet<ContainerRegistration>();

        internal Workspace(ILightContainer parent)
        {
            _parent = parent;
        }

        internal void Register(ContainerRegistration registation, string name)
        {
            _registrations.AddOrReplace(registation.RegistrationType.GetHashCode(), name, registation);
        }

        internal object Resolve<T>(string name)
        {
            var type = typeof(T);
            var entry = _registrations.Get(type.GetHashCode(), name);
            if (entry == null)
                throw new KeyNotFoundException($"Registration for type {type.GetDescription()} with name {name} could not be found");

            var registration = entry.Value.Value;

            // this is a cache for recursive resolve
            if (registration.RegistrationType == RegistrationType.Type && registration.Instance != null)
                return registration.Instance;

            // this is a singleton/instance
            if (registration.RegistrationType == RegistrationType.Instance && registration.Instance != null)
                return registration.Instance;

            // upper checks could be collaps but don't do this because of readability

            // creating an instance and injecting everything
            if (registration.RegistrationType != RegistrationType.Factory)
                registration.Instance = Creator.Create<T>(_parent);
            else
                registration.Instance = registration.Factory.Invoke(_parent, registration.RegisteredType, name);

            // injecting
            Creator.InjectPropsAndFields<T>((T)registration.Instance, _parent);
            Creator.InjectMethods<T>((T)registration.Instance, _parent);

            var result = registration.Instance;
            // reset cache if not instance
            if (registration.RegistrationType != RegistrationType.Instance)
                registration.Instance = null;

            return result;
        }

        internal bool IsRegistered<T>(string name)
        {
            var type = typeof(T);
            var entry = _registrations.Get(type.GetHashCode(), name);
            return entry != null;
        }
    }
}
