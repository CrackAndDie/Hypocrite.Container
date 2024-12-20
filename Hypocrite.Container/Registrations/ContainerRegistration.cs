using Hypocrite.Container.Interfaces;
using System;

namespace Hypocrite.Container.Registrations
{
    internal class ContainerRegistration
    {
        internal Type RegisteredType { get; set; }

        internal Type MappedToType { get; set; }

        internal object Instance { get; set; }

        internal Func<ILightContainer, Type, string, object> Factory { get; set; }

        internal RegistrationType RegistrationType { get; set; }
    }

    internal enum RegistrationType : byte
    {
        Instance,
        Type,
        Factory
    }
}
