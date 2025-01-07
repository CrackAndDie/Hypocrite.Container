using FastExpressionCompiler;
using Hypocrite.Container.Common;
using Hypocrite.Container.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hypocrite.Container.Creators
{
    internal static class PropsAndFieldsInjector
    {
        internal static Action<object, Dictionary<string, object>> CreateInjector(Type type, int hash, object instance, Dictionary<string, object> args)
        {
            Action<object, Dictionary<string, object>> injector;
            // check for cache
            var value = _cachedWithParams.Get(hash);
            if (value != null)
            {
                injector = value;
            }
            else
            {
                injector = GenerateFactoryWithParams(type, args.Keys.ToArray());
                _cachedWithParams.AddOrReplace(hash, injector);
            }
            injector.Invoke(instance, args);

            return GenerateFactoryWithParams(type, args.Keys.ToArray());
        }

        private static Action<object, Dictionary<string, object>> GenerateFactoryWithParams(Type type, string[] keys)
        {
            // Params
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            // Convert to required type
            var typedInstance = Expression.Convert(instanceParam, type);
            var dictParam = Expression.Parameter(typeof(Dictionary<string, object>), "dict");

            var list = new List<Expression>();
            // props shite
            var propertyInfos = type.GetTypeInfo().DeclaredProperties;
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
            var fieldInfos = type.GetTypeInfo().DeclaredFields;
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
            return lambda.CompileFast();
        }
    }
}
