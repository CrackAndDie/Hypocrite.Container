using Hypocrite.Container.Extensions;
using System.Linq.Expressions;
using System.Reflection;

namespace Hypocrite.Container.Creators
{
    internal static class PropsAndFieldsInjector
    {
        private static readonly Dictionary<Type, Action<object, Dictionary<string, object>>> _cachedWithParams = new Dictionary<Type, Action<object, Dictionary<string, object>>>();
        internal static void Inject<T>(T instance, Dictionary<string, object> args)
        {
            Action<object, Dictionary<string, object>> injector;
            // check for cache
            if (_cachedWithParams.TryGetValue(typeof(T), out var cachedInjector))
            {
                injector = cachedInjector;
            }
            else
            {
                injector = GenerateFactoryWithParams<T>(args.Keys.ToArray());
                _cachedWithParams.Add(typeof(T), injector);
            }
            injector.Invoke(instance, args);
        }

        private static Action<object, Dictionary<string, object>> GenerateFactoryWithParams<T>(string[] keys)
        {
            // Params
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            // Convert to required type
            var typedInstance = Expression.Convert(instanceParam, typeof(T));
            var dictParam = Expression.Parameter(typeof(Dictionary<string, object>), "dict");

            var list = new List<Expression>();
            // props shite
            var propertyInfos = typeof(T).GetTypeInfo().DeclaredProperties;
            foreach (var propertyInfo in propertyInfos)
            {
                if (!keys.Contains(propertyInfo.Name) || !propertyInfo.CanWrite)
                    continue;

                Expression call = Expression.Call(
                                   typeof(DictionaryExtensions),
                                   "GetValue", 
                                   new[] { propertyInfo.PropertyType },
                                   new Expression[]
                                    {
                                        dictParam,
                                        Expression.Constant(propertyInfo.Name)
                                    });

                var assign = Expression.Assign(
                    Expression.Property(typedInstance, propertyInfo), call
                );
                list.Add(assign);
            }
            // fields shite
            var fieldInfos = typeof(T).GetTypeInfo().DeclaredFields;
            foreach (var fieldInfo in fieldInfos)
            {
                if (!keys.Contains(fieldInfo.Name))
                    continue;

                Expression call = Expression.Call(
                                   typeof(DictionaryExtensions),
                                   "GetValue",
                                   new[] { fieldInfo.FieldType },
                                   new Expression[]
                                    {
                                        dictParam,
                                        Expression.Constant(fieldInfo.Name)
                                    });
                var assign = Expression.Assign(
                     Expression.Field(typedInstance, fieldInfo), call
                 );
                list.Add(assign);
            }

            // Block expression for inits
            var blockExpr = Expression.Block(list);

            // Compiling the lambda
            var lambda = Expression.Lambda<Action<object, Dictionary<string, object>>>(blockExpr, instanceParam, dictParam);
            return lambda.Compile();
        }
    }
}
