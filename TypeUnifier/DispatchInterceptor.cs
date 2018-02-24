using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using TypeUnifier.Adapting;

namespace TypeUnifier
{
    class DispatchInterceptor : StandardInterceptor
    {
        public INode Node { get; }

        private readonly Dictionary<MethodInfo, Action<IInvocation>> _knownHandlers;
        private readonly Dictionary<MethodInfo, MethodInfoAdapter> _methodInfoAdapters;
        private readonly IOrderedEnumerable<KeyValuePair<Type, Type>> _exceptionTypeMap;

        public DispatchInterceptor(INode node)
        {
            this.Node = node;
            _knownHandlers = new Dictionary<MethodInfo, Action<IInvocation>>();
            _methodInfoAdapters = new Dictionary<MethodInfo, MethodInfoAdapter>();

            _exceptionTypeMap = this.Node.Mappings
                .Where(m => typeof(Exception).IsAssignableFrom(m.Value))
                .OrderByDescending(m => GetInheritanceString(m.Value));

            string GetInheritanceString(Type type)
            {
                var typeNames = new Stack<string>();
                while (!type.Equals(typeof(object)))
                {
                    typeNames.Push(type.Name);
                    type = type.BaseType;
                }
                return string.Join("|", typeNames);
            }
        }

        protected override void PreProceed(IInvocation invocation)
        {
            var abstractionType = ((DispatchedObject)invocation.Proxy).AbstractionType;

            if (!this.Node.Mappings.ContainsKey(abstractionType))
                throw new NodeNotImplementedException(this.Node.Id, abstractionType);

            if (!_methodInfoAdapters.ContainsKey(invocation.Method))
            {
                bool isConstructor = false, isStatic = false;
                IEnumerable<string> aliasNames = null;

                foreach (var attr in invocation.Method.GetCustomAttributes<DispatchAttribute>())
                {
                    if (attr is ConstructorAttribute)
                    {
                        if (!invocation.Method.ReturnType.Equals(abstractionType))
                            throw new ConstructorReturnTypeException(invocation.Method, abstractionType);
                        isConstructor = true;
                    }
                    else
                    {
                        if (attr is StaticAttribute) isStatic = true;
                        if (attr is AliasAttribute a) aliasNames = a.Names;
                    }
                }

                _methodInfoAdapters[invocation.Method] = new MethodInfoAdapter(isConstructor, isStatic, aliasNames);
            }

            var implementation = ((DispatchedObject)invocation.Proxy).Implementation;

            if (_methodInfoAdapters[invocation.Method].IsConstructor && implementation != null)
                throw new MultipleConstructionException(invocation.Method);

            if (!_methodInfoAdapters[invocation.Method].IsStatic
                && !_methodInfoAdapters[invocation.Method].IsConstructor
                && implementation == null)
                throw new NotConstructedException(invocation.Method);
        }

        protected override void PerformProceed(IInvocation invocation)
        {
            if (!_knownHandlers.ContainsKey(invocation.Method))
            {
                MakeHandler(invocation);
            }

            _knownHandlers[invocation.Method](invocation);
        }

        IEnumerable<MethodDataAdapter> GetMethodCandidates(IInvocation invocation)
        {
            var abstractionType = ((DispatchedObject)invocation.Proxy).AbstractionType;
            var implementationType = Node.Mappings[abstractionType];
            var methodInfoAdapter = _methodInfoAdapters[invocation.Method];

            var bindingFlags = methodInfoAdapter.IsStatic
                ? BindingFlags.Public | BindingFlags.Static
                : BindingFlags.Public | BindingFlags.Instance;

            var candidateMethods = methodInfoAdapter.IsConstructor
                ? implementationType.GetConstructors(bindingFlags)
                : (IEnumerable<MethodBase>)implementationType
                    .GetMethods(bindingFlags)
                    .Where(m => methodInfoAdapter.AliasNames == null && invocation.Method.Name.Equals(m.Name)
                        || methodInfoAdapter.AliasNames != null && methodInfoAdapter.AliasNames.Contains(m.Name));

            var abstractionParameters = invocation.Method.GetParameters();

            foreach (var candidate in candidateMethods)
            {
                var implementationParameters = candidate.GetParameters();
                if (abstractionParameters.Length != implementationParameters.Length) continue;

                var parameterAdapters = new List<ITypeAdapter>();
                var parameterAdaptable = true;
                for (var i = 0; i < abstractionParameters.Length; i++)
                {
                    var parameterAdapter = TypeAdapter.Build(Node.Mappings, abstractionParameters[i].ParameterType, implementationParameters[i].ParameterType);
                    if (parameterAdapter == null) { parameterAdaptable = false; break; }
                    parameterAdapters.Add(parameterAdapter);
                }

                if (!parameterAdaptable) continue;

                var implementationReturnType = candidate is ConstructorInfo c ? c.DeclaringType : ((MethodInfo)candidate).ReturnType;
                var returnAdapter = TypeAdapter.Build(Node.Mappings, implementationReturnType, invocation.Method.ReturnType);
                if (returnAdapter != null) yield return new MethodDataAdapter(candidate, parameterAdapters, returnAdapter);
            }
        }

        void MakeHandler(IInvocation invocation)
        {
            var abstractionType = ((DispatchedObject)invocation.Proxy).AbstractionType;
            var implementationType = Node.Mappings[abstractionType];

            var candidates = this.GetMethodCandidates(invocation).ToArray();

            if (candidates.Length == 0)
            {
                _knownHandlers.Add(invocation.Method, i => throw new MethodNotImplementedException(i.Method, implementationType));
                return;
            }

            if (candidates.Length > 1)
            {
                _knownHandlers.Add(invocation.Method, i => throw new AmbiguousMethodException(candidates.Select(c => c.Method)));
                return;
            }

            var candidate = candidates.Single();
            var variables = new List<ParameterExpression>();
            var body = new List<Expression>();
            var byRefAssignBack = new List<Expression>();

            var invocationDefinition = Expression.Parameter(typeof(IInvocation));
            var invocationArguments = Expression.MakeMemberAccess(invocationDefinition, Ref.IInvocation_Arguments);

            for (var i = 0; i < candidate.ParameterAdapters.Length; i++)
            {
                var adapter = candidate.ParameterAdapters[i];
                var byRef = adapter.To.IsByRef;
                var variableType = byRef ? adapter.To.GetElementType() : adapter.To;
                var variable = Expression.Variable(variableType);
                var assignVariable =
                    Expression.Assign(
                        variable,
                        Expression.Convert(
                            Expression.Call(
                                Expression.Constant(adapter),
                                Ref.ITypeAdapter_Adapt,
                                Expression.ArrayAccess(invocationArguments, Expression.Constant(i)),
                                Expression.Constant(this)),
                            variableType));

                variables.Add(variable);
                body.Add(assignVariable);

                if (byRef)
                {
                    var assignBackAdapter = TypeAdapter.Build(Node.Mappings, variableType, adapter.From.GetElementType());
                    byRefAssignBack.Add(
                        Expression.Assign(
                            Expression.ArrayAccess(invocationArguments, Expression.Constant(i)),
                            Expression.Convert(
                                Expression.Call(
                                    Expression.Constant(assignBackAdapter),
                                    Ref.ITypeAdapter_Adapt,
                                    variable,
                                    Expression.Constant(this)),
                                typeof(object))));
                }
            }

            Expression invocationReturnValue = Expression.MakeMemberAccess(invocationDefinition, Ref.IInvocation_ReturnValue);

            if (candidate.Method is ConstructorInfo ctor)
            {
                body.Add(
                    Expression.Assign(
                        invocationReturnValue,
                        Expression.Convert(
                            Expression.Call(
                                Expression.Constant(candidate.ReturnAdapter),
                                Ref.ITypeAdapter_Adapt,
                                Expression.New(ctor, variables),
                                Expression.Constant(this)),
                            typeof(object))));
            }
            else if (candidate.Method is MethodInfo method)
            {
                Expression invokeMethod = method.IsStatic
                    ? Expression.Call(method, variables)
                    : Expression.Call(
                        Expression.Convert(
                            Expression.MakeMemberAccess(
                                Expression.Convert(
                                    Expression.MakeMemberAccess(invocationDefinition, Ref.IInvocation_Proxy),
                                    typeof(DispatchedObject)),
                                Ref.DispatchedObject_Implementation),
                            implementationType),
                        method,
                        variables);

                if (method.ReturnType == typeof(void))
                {
                    body.Add(invokeMethod);
                }
                else
                {
                    body.Add(
                        Expression.Assign(
                            invocationReturnValue,
                            Expression.Convert(
                                Expression.Call(
                                    Expression.Constant(candidate.ReturnAdapter),
                                    Ref.ITypeAdapter_Adapt,
                                    Expression.Convert(invokeMethod, typeof(object)),
                                    Expression.Constant(this)),
                                typeof(object))));
                }
            }

            if (byRefAssignBack.Any()) body.AddRange(byRefAssignBack);

            var handler = Expression.Lambda<Action<IInvocation>>(WrapExceptions(Expression.Block(variables, body)), invocationDefinition);
            _knownHandlers.Add(invocation.Method, handler.Compile());
        }

        Expression WrapExceptions(Expression body)
        {
            if (!_exceptionTypeMap.Any()) return body;

            var catchExps =
                from map in _exceptionTypeMap
                let ex = Expression.Parameter(map.Value)
                select Expression.Catch(
                    ex,
                    Expression.Block(
                        Expression.Throw(
                            Expression.New(
                                typeof(Exception<>)
                                    .MakeGenericType(map.Key)
                                    .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                                    .Single(),
                                Expression.Convert(
                                    Expression.Call(
                                        Ref.ImplementationToAbstractionAdapter_ForceAdapt,
                                        ex,
                                        Expression.Constant(map.Value),
                                        Expression.Constant(map.Key),
                                        Expression.Constant(this)),
                                    map.Key)))));

            return Expression.TryCatch(Expression.Block(typeof(void), body), catchExps.ToArray());
        }
    }
}
