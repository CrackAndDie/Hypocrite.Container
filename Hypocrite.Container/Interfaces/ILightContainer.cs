namespace Hypocrite.Container.Interfaces
{
	public interface ILightContainer
	{
        // Resolves
        object Resolve(Type type);
        object Resolve(Type type, string name);
    }
}
