using System;

namespace Hypocrite.Container
{
	/// <summary>
	/// This attribute is used to mark properties and parameters as targets for injection.
	/// </summary>
	/// <remarks>
	/// For properties, this attribute is necessary for injection to happen. For parameters,
	/// it's not needed unless you want to specify additional information to control how
	/// the parameter is resolved.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Constructor)]
	public sealed class InjectionAttribute : Attribute
	{
		/// <summary>
		/// Create an instance of <see cref="InjectionAttribute"/> with no name.
		/// </summary>
		public InjectionAttribute() { }
	}
}
