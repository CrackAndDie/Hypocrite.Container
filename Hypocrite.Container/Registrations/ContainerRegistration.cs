using Hypocrite.Container.Interfaces;
using System;

namespace Hypocrite.Container.Registrations
{
    internal class ContainerRegistration
    {
        public Type RegisteredType { get; set; }

        public Type MappedToType { get; set; }

        public object Instance { get; set; }

        public Func<ILightContainer, Type, string, object> Factory { get; set; }

        public RegistrationType RegistrationType { get; set; }
    }

    public enum RegistrationType : byte
    {
        Instance,
        Type,
        Factory
    }
}
