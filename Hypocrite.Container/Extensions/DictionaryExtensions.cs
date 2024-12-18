namespace Hypocrite.Container.Extensions
{
    internal static class DictionaryExtensions
    {
        internal static TType GetValue<TType>(this Dictionary<string, object> d, string name)
        {
            object value;
            return d.TryGetValue(name, out value) ? (TType)value : default(TType);
        }
    }
}
