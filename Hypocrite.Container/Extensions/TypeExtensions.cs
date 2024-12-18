using System.Reflection;

namespace Hypocrite.Container.Extensions
{
    /// <summary>
    /// Useful extension methods on Type
    /// Stolen from https://github.com/canton7/Stylet/blob/master/Stylet/StyletIoC/Internal/TypeExtensions.cs
    /// </summary>
    internal static class TypeExtensions
	{
		private static readonly Dictionary<Type, string> primitiveNameMapping = new()
		{
			{ typeof(byte), "byte" },
			{ typeof(sbyte), "sbyte" },
			{ typeof(char), "char" },
			{ typeof(short), "short" },
			{ typeof(ushort), "ushort" },
			{ typeof(int), "int" },
			{ typeof(uint), "uint" },
			{ typeof(long), "long" },
			{ typeof(ulong), "ulong" },
			{ typeof(float), "float" },
			{ typeof(double), "double" },
			{ typeof(decimal), "decimal" },
			{ typeof(bool), "bool" },
			{ typeof(string), "string" },
		};

		/// <summary>
		/// Return a human-readable description of the given type
		/// </summary>
		/// <remarks>
		/// This returns things like 'List{int}' instead of 'List`1[System.Int32]'
		/// </remarks>
		/// <param name="type">Type to generate the description for</param>
		/// <returns>Description of the given type</returns>
		internal static string GetDescription(this Type type)
		{
			if (type.IsGenericTypeDefinition)
				return string.Format("{0}<{1}>", type.Name.Split('`')[0], string.Join(", ", type.GetTypeInfo().GenericTypeParameters.Select(x => x.Name)));
			Type[] genericArguments = type.GetGenericArguments();

			string name;
			if (genericArguments.Length > 0)
			{
				IEnumerable<string> genericArgumentNames = genericArguments.Select(x => primitiveNameMapping.TryGetValue(x, out name) ? name : x.Name);
				return string.Format("{0}<{1}>", type.Name.Split('`')[0], string.Join(", ", genericArgumentNames));
			}
			else
			{
				return primitiveNameMapping.TryGetValue(type, out name) ? name : type.Name;
			}
		}

		internal static object GetDefaultValue(this Type type)
		{
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
	}
}
