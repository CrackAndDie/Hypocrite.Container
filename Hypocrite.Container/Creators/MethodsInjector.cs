﻿using FastExpressionCompiler;
using Hypocrite.Container.Common;
using Hypocrite.Container.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hypocrite.Container.Creators
{
    internal static class MethodsInjector
    {
        internal static Action<object, Dictionary<string, object[]>> CreateInjector(Type type, Dictionary<string, object[]> args)
        {
            return GenerateFactoryWithParams(type, args);
        }

        private static Action<object, Dictionary<string, object[]>> GenerateFactoryWithParams(Type type, Dictionary<string, object[]> args)
        {
            // Params
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            // Convert to required type
            var typedInstance = Expression.Convert(instanceParam, type);
            // Param for args 
            var dictParam = Expression.Parameter(typeof(Dictionary<string, object[]>), "args");

            var list = new List<Expression>();
            // methods shite
            var methodInfos = type.GetTypeInfo().DeclaredMethods;
            foreach (var methodInfo in methodInfos)
            {
                if (!args.ContainsKey(methodInfo.Name) || args[methodInfo.Name].Length != methodInfo.GetParameters().Length)
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
                Expression[] methodArgs = methodInfo.GetParameters()
                    .Select((p, i) =>
                        Expression.Convert(
                            Expression.ArrayIndex(call, Expression.Constant(i)),
                            p.ParameterType) as Expression)
                    .ToArray();

                // Handle 'params' word as the last one
                var lastParam = methodInfo.GetParameters().LastOrDefault();
                if (lastParam?.IsDefined(typeof(ParamArrayAttribute), false) == true)
                {
                    // Создаем массив для параметра params
                    var arrayExpr = Expression.NewArrayInit(
                        lastParam.ParameterType.GetElementType(),
                        methodArgs.Skip(lastParam.Position)
                    );

                    methodArgs = methodArgs.Take(lastParam.Position)
                        .Concat(new[] { arrayExpr })
                        .ToArray();
                }

                Expression methodCall = Expression.Call(
                                  typedInstance,
                                  methodInfo,
                                  methodArgs);
                list.Add(methodCall);
            }

            // Block expression for inits
            var blockExpr = Expression.Block(list);

            // Compiling the lambda
            var lambda = Expression.Lambda<Action<object, Dictionary<string, object[]>>>(blockExpr, instanceParam, dictParam);
            return lambda.CompileFast();
        }
    }
}
