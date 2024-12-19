using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Reflection;

namespace Hypocrite.Container.Creators
{
    internal static class InstanceCreator
    {
        private static readonly Dictionary<Type, Func<object[], object>> _cachedWithParams = new Dictionary<Type, Func<object[], object>>();
        internal static T CreateWithParams<T>(ConstructorInfo ctor, object[] args)
        {
            Func<object[], object> creator;
            // check for cache
            if (_cachedWithParams.TryGetValue(typeof(T), out var cachedCreator))
            {
                creator = cachedCreator;
            }
            else
            {
                creator = GenerateFactoryWithParams(ctor);
                _cachedWithParams.Add(typeof(T), creator);
            }
            return (T)creator.Invoke(args);
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
            return lambda.Compile();
        }

        private static readonly Dictionary<Type, Func<object>> _cachedNoParams = new Dictionary<Type, Func<object>>();
        internal static T CreateNoParams<T>() where T : new()
        {
            Func<object> creator;
            // check for cache
            if (_cachedNoParams.TryGetValue(typeof(T), out var cachedCreator))
            {
                creator = cachedCreator;
            }
            else
            {
                creator = GenerateFactoryNoParams<T>();
                _cachedNoParams.Add(typeof(T), creator);
            }
            return (T)creator.Invoke();
        }

        private static Func<object> GenerateFactoryNoParams<T>() where T : new()
        {
            Expression<Func<T>> expr = () => new T();
            NewExpression newExpr = (NewExpression)expr.Body;

            var method = new DynamicMethod(
                name: "lambda",
                returnType: typeof(object),
                parameterTypes: new Type[0],
                m: typeof(InstanceCreator).Module,
                skipVisibility: true);

            ILGenerator ilGen = method.GetILGenerator();
            // Constructor for value types could be null
            if (newExpr.Constructor != null)
            {
                ilGen.Emit(OpCodes.Newobj, newExpr.Constructor);
            }
            else
            {
                LocalBuilder temp = ilGen.DeclareLocal(newExpr.Type);
                ilGen.Emit(OpCodes.Ldloca, temp);
                ilGen.Emit(OpCodes.Initobj, newExpr.Type);
                ilGen.Emit(OpCodes.Ldloc, temp);
            }
            ilGen.Emit(OpCodes.Ret);
            return (Func<object>)method.CreateDelegate(typeof(Func<object>));
        }
    }
}
