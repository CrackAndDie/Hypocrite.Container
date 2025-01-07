using System.Reflection.Emit;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Expressions;
using FastExpressionCompiler;
using Hypocrite.Container.Common;

namespace Hypocrite.Container.Creators
{
    internal static class InstanceCreator
    {
        internal static Func<object[], object> CreateLambda(int hash, ConstructorInfo ctor, object[] args)
        {
            Func<object[], object> creator;
            // check for cache
            var value = _cachedWithParams.Get(hash);
            if (value != null)
            {
                creator = value;
            }
            else
            {
                creator = GenerateFactoryWithParams(ctor);
                _cachedWithParams.AddOrReplace(hash, creator);
            }
            return GenerateFactoryWithParams(ctor);
        }

        private static Func<object[], object> GenerateFactoryWithParams(ConstructorInfo ctor)
        {
            // Param for args of ctor
            var param = Expression.Parameter(typeof(object[]), "args");

            // Convert params to args
            var constructorArgs = ctor.GetParameters()
                .Select((p, i) =>
                    Expression.Convert(
                        Expression.ArrayIndex(param, Expression.Constant(i)),
                        p.ParameterType))
                .ToArray();

            // Expression for ctor call
            var newExpr = Expression.New(ctor, constructorArgs);

            // Compiling the lambda
            var lambda = Expression.Lambda<Func<object[], object>>(newExpr, param);
            return lambda.CompileFast();
        }
    }
}
