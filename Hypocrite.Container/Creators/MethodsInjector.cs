using Hypocrite.Container.Extensions;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hypocrite.Container.Creators
{
    internal static class MethodsInjector
    {
        private static readonly Dictionary<Type, Action<object, Dictionary<string, object[]>>> _cachedWithParams = new Dictionary<Type, Action<object, Dictionary<string, object[]>>>();
        internal static void Inject<T>(T instance, Dictionary<string, object[]> args)
        {
            Action<object, Dictionary<string, object[]>> injector;
            // check for cache
            if (_cachedWithParams.TryGetValue(typeof(T), out var cachedInjector))
            {
                injector = cachedInjector;
            }
            else
            {
                injector = GenerateFactoryWithParams<T>(args);
                _cachedWithParams.Add(typeof(T), injector);
            }
            injector.Invoke(instance, args);
        }

        private static Action<object, Dictionary<string, object[]>> GenerateFactoryWithParams<T>(Dictionary<string, object[]> args)
        {
            // Params
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            // Convert to required type
            var typedInstance = Expression.Convert(instanceParam, typeof(T));
            // Param for args 
            var dictParam = Expression.Parameter(typeof(Dictionary<string, object[]>), "args");

            var list = new List<Expression>();
            // methods shite
            var methodInfos = typeof(T).GetTypeInfo().DeclaredMethods;
            foreach (var methodInfo in methodInfos)
            {
                if (!args.ContainsKey(methodInfo.Name))
                    continue;

                Expression call = Expression.Call(
                                  typeof(DictionaryExtensions),
                                  "GetValue",
                                  new[] { typeof(object[]) },
                                  new Expression[]
                                   {
                                        dictParam,
                                        Expression.Constant(methodInfo.Name)
                                   });

                // Convert params to args
                var methodArgs = methodInfo.GetParameters()
                    .Select((p, i) =>
                        Expression.Convert(
                            Expression.ArrayIndex(call, Expression.Constant(i)),
                            p.ParameterType))
                    .ToArray();

                List<Expression> fullMethodArgs = new List<Expression>() { typedInstance };
                fullMethodArgs.AddRange(methodArgs);

                Expression methodCall = Expression.Call(
                                  typeof(T),
                                  methodInfo.Name,
                                  null,
                                  fullMethodArgs.ToArray());
                list.Add(methodCall);
            }

            // Block expression for inits
            var blockExpr = Expression.Block(list);

            // Compiling the lambda
            var lambda = Expression.Lambda<Action<object, Dictionary<string, object[]>>>(blockExpr, instanceParam, dictParam);
            return lambda.Compile();
        }
    }
}
