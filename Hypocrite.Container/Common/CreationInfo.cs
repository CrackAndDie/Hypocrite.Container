using System;
using System.Collections.Generic;
using System.Reflection;
using static Hypocrite.Container.Creators.Creator;

namespace Hypocrite.Container.Common
{
    internal class CreationInfo
    {
        internal Tuple<ConstructorInfo, InjectionElement[]> CtorData { get; set; }
        internal Func<object[], object> CtorLambda { get; set; }
        internal InjectionElement[] PropsAndFieldsData { get; set; }
        internal Action<object, Dictionary<string, object>> PropsAndFieldsInjector { get; set; }
        internal Dictionary<string, InjectionElement[]> MethodsData { get; set; }
        internal Action<object, Dictionary<string, object[]>> MethodsInjector { get; set; }
    }
}
