using Hypocrite.Container.Interfaces;

namespace Hypocrite.Container
{
	public class LightContainer : ILightContainer
	{
        public object Resolve(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public object Resolve(Type type, string name)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
