using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Irony.Parsing;
using System.Reflection;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace My.ExpressionParser
{
    public class ExpressionParser
    {
        private const string TransparentIdentifier = "<>h__TransparentIdentifier";
        private static Dictionary<string, ExpressionType> binaryOps = new Dictionary<string, ExpressionType>();
        private static Dictionary<string, ExpressionType> unaryOps = new Dictionary<string, ExpressionType>();
        private static ConcurrentDictionary<string, Type> knownTypes = new ConcurrentDictionary<string, Type>();
        private static ConcurrentDictionary<string, MethodInfo> extensionMethods = new ConcurrentDictionary<string, MethodInfo>();

        private List<Type> _paramTypes = new List<Type>();
        private Dictionary<string, object> _knownVariables = new Dictionary<string, object>();
        private Stack<IEnumerable<ParameterExpression>> _parameters = new Stack<IEnumerable<ParameterExpression>>();
        private Stack<LINQContext> _linqVariables;

        static ExpressionParser()
        {
            #region BinaryOps
            binaryOps["+"] = ExpressionType.Add;
            binaryOps["-"] = ExpressionType.Subtract;
            binaryOps["*"] = ExpressionType.Multiply;
            binaryOps["/"] = ExpressionType.Divide;
            binaryOps["%"] = ExpressionType.Modulo;
            binaryOps["&"] = ExpressionType.And;
            binaryOps["|"] = ExpressionType.Or;
            binaryOps["^"] = ExpressionType.ExclusiveOr;
            binaryOps["??"] = ExpressionType.Coalesce;
            binaryOps["<<"] = ExpressionType.LeftShift;
            binaryOps[">>"] = ExpressionType.RightShift;
            binaryOps["+="] = ExpressionType.AddAssign;
            binaryOps["-="] = ExpressionType.SubtractAssign;
            binaryOps["*="] = ExpressionType.MultiplyAssign;
            binaryOps["/="] = ExpressionType.DivideAssign;
            binaryOps["%="] = ExpressionType.ModuloAssign;
            binaryOps["&="] = ExpressionType.AndAssign;
            binaryOps["|="] = ExpressionType.OrAssign;
            binaryOps["^="] = ExpressionType.ExclusiveOrAssign;
            binaryOps["<<="] = ExpressionType.LeftShiftAssign;
            binaryOps[">>="] = ExpressionType.RightShiftAssign;
            binaryOps["=="] = ExpressionType.Equal;
            binaryOps["!="] = ExpressionType.NotEqual;
            binaryOps["&&"] = ExpressionType.AndAlso;
            binaryOps["||"] = ExpressionType.OrElse;
            binaryOps["is"] = ExpressionType.TypeIs;
            binaryOps["="] = ExpressionType.Assign;
            #endregion

            #region UnaryOps
            unaryOps["++"] = ExpressionType.PreIncrementAssign;
            unaryOps["--"] = ExpressionType.PreDecrementAssign;
            unaryOps["post++"] = ExpressionType.PostIncrementAssign;
            unaryOps["post--"] = ExpressionType.PostDecrementAssign;
            unaryOps["!"] = ExpressionType.Not;
            unaryOps["~"] = ExpressionType.Not;
            unaryOps["+"] = ExpressionType.Quote;
            unaryOps["-"] = ExpressionType.Negate;
            //ops["as"] = ExpressionType.TypeAs;
            #endregion

            #region BuiltinTypes
            knownTypes["bool"] = typeof(bool);
            knownTypes["sbyte"] = typeof(sbyte);
            knownTypes["byte"] = typeof(byte);
            knownTypes["short"] = typeof(short);
            knownTypes["ushort"] = typeof(ushort);
            knownTypes["int"] = typeof(int);
            knownTypes["uint"] = typeof(uint);
            knownTypes["long"] = typeof(long);
            knownTypes["ulong"] = typeof(ulong);
            knownTypes["float"] = typeof(float);
            knownTypes["double"] = typeof(double);
            knownTypes["char"] = typeof(char);
            knownTypes["string"] = typeof(string);
            knownTypes["decimal"] = typeof(decimal);
            knownTypes["object"] = typeof(object);
            knownTypes["void"] = typeof(void);
            #endregion

            #region ExtensionMethods
            extensionMethods["select"] = typeof(Queryable).GetExtensionMethod("Select", typeof(IQueryable<>), typeof(Expression<>).MakeGenericType(typeof(Func<,>)));
            extensionMethods["let"] = extensionMethods["select"];
            extensionMethods["from"] = typeof(Queryable).GetExtensionMethod("SelectMany", typeof(IQueryable<>), typeof(Expression<>).MakeGenericType(typeof(Func<,>)), typeof(Expression<>).MakeGenericType(typeof(Func<,,>)));
            //extensionMethods["from"] = typeof(Queryable).GetMember("SelectMany")[3] as MethodInfo;
            extensionMethods["group"] = typeof(Queryable).GetExtensionMethod("GroupBy", typeof(IQueryable<>), typeof(Expression<>).MakeGenericType(typeof(Func<,>)));
            extensionMethods["join"] = typeof(Queryable).GetExtensionMethod("Join", typeof(IQueryable<>), typeof(IEnumerable<>), typeof(Expression<>).MakeGenericType(typeof(Func<,>)), typeof(Expression<>).MakeGenericType(typeof(Func<,>)), typeof(Expression<>).MakeGenericType(typeof(Func<,,>)));
            extensionMethods["groupjoin"] = typeof(Queryable).GetExtensionMethod("GroupJoin", typeof(IQueryable<>), typeof(IEnumerable<>), typeof(Expression<>).MakeGenericType(typeof(Func<,>)), typeof(Expression<>).MakeGenericType(typeof(Func<,>)), typeof(Expression<>).MakeGenericType(typeof(Func<,,>)));
            extensionMethods["orderby"] = typeof(Queryable).GetExtensionMethod("OrderBy", typeof(IQueryable<>), typeof(Expression<>).MakeGenericType(typeof(Func<,>)));
            extensionMethods["orderbydescending"] = typeof(Queryable).GetExtensionMethod("OrderByDescending", typeof(IQueryable<>), typeof(Expression<>).MakeGenericType(typeof(Func<,>)));
            extensionMethods["thenby"] = typeof(Queryable).GetExtensionMethod("ThenBy", typeof(IOrderedQueryable<>), typeof(Expression<>).MakeGenericType(typeof(Func<,>)));
            extensionMethods["thenbydescending"] = typeof(Queryable).GetExtensionMethod("ThenByDescending", typeof(IOrderedQueryable<>), typeof(Expression<>).MakeGenericType(typeof(Func<,>)));
            extensionMethods["where"] = typeof(Queryable).GetExtensionMethod("Where", typeof(IQueryable<>), typeof(Expression<>).MakeGenericType(typeof(Func<,>)));
            extensionMethods["DefaultIfEmpty"] = typeof(Enumerable).GetExtensionMethod("DefaultIfEmpty", typeof(IEnumerable<>));
            #endregion
        }

        private static MethodInfo MakeMethod(string methodName, params Type[] genericTypes)
        {
            if (!extensionMethods[methodName].IsGenericMethod)
                return extensionMethods[methodName];
            var len = extensionMethods[methodName].GetGenericArguments().Length;
            return extensionMethods[methodName].MakeGenericMethod(genericTypes.Take(len).ToArray());
        }

        private static Type GetAnonymousType(string[] propertyNames, Type[] propertyTypes)
        {
            List<AnonymousMetaProperty> properties = new List<AnonymousMetaProperty>();
            for (int i = 0; i < propertyNames.Length; ++i)
            {
                properties.Add(new AnonymousMetaProperty(propertyNames[i], propertyTypes[i]));
            }
            return  AnonymousTypeHelper.GetAnonymousType(properties.ToArray()).GetClrType();
        }

        public ExpressionParser With<T>(Expression<Func<T>> variable)
        {
            var name = variable.Body.ToReadableString();
            var @var = variable.Compile()();

            return With(name, @var);
        }

        public ExpressionParser With(string name, object variable)
        {
            _knownVariables.Add(name, variable);
            knownTypes.TryAdd(variable.GetType().Name, variable.GetType());
            return this;
        }

        public ExpressionParser With(Type type)
        {
            knownTypes.TryAdd(type.Name, type);
            return this;
        }
        
        public Expression Parse(string expression)
        {
            var parser = new Parser(new CSharpExpressionGrammar());
            var tree = parser.Parse(expression);
            if (tree.Status == ParseTreeStatus.Error) 
                throw new Exception("Parse Error");
            _parameters.Clear();
            return ProcessExpression(tree.Root);
        }

        public Expression<TDelegate> Parse<TDelegate>(string expression)
        {
            _paramTypes.Clear();
            var genericParameters = typeof(TDelegate).GetGenericTypeDefinition().GetGenericArguments();
            var realArguments = typeof(TDelegate).GetGenericArguments();
            _paramTypes.AddRange(
                realArguments.Where((a, i) => 
                    genericParameters.First(p => p.GenericParameterPosition == i).
                    GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant)));

            var exp = Parse(expression);

            var genericlambda = exp as Expression<TDelegate>;
            if (genericlambda != null)
                return genericlambda;
            
            var lambda = exp as LambdaExpression;
            if (lambda != null)
            {
                return Expression.Lambda<TDelegate>(lambda.Body, lambda.Parameters);
            }

            return Expression.Lambda<TDelegate>(exp);
        }

        private Expression ProcessExpression(ParseTreeNode expNode)
        {
            switch (expNode.GetName())
            {
                default:
                    throw new Exception(expNode.GetName());

                case "anonymous_function_body":
                case "parenthesized_expression":
                    return ProcessExpression(expNode.FirstChild);
                
                case "lambda_expression":
                    return ProcessLambdaExpression(expNode);

                case "typecast_expression":
                    return ProcessConvertExpression(expNode);

                case "primary_expression":
                    return ProcessUnaryExpression(expNode);

                case "bin_op_expression":
                    return ProcessBinaryExpression(expNode);

                case "conditional_expression":
                    return ProcessConditionalExpression(expNode);

                case "member_access":
                    return ProcessMemberAccessExpression(expNode);

                case "object_creation_expression":
                    return ProcessNewExpression(expNode);

                case "anonymous_type_creation_expression":
                    return ProcessNewAnonymousExpression(expNode);

                case "literal":
                    return ProcessConstantExpression(expNode);

                case "query_expression":
                    return ProcessLINQ(expNode);
            }

            return Expression.Empty();
        }

        private Expression ProcessLINQ(ParseTreeNode expNode)
        {
            _linqVariables = new Stack<LINQContext>();
            Expression linq = Expression.Empty();

            foreach (var child in expNode.ChildNodes)
            {
                linq = ProcessQueryExpression(child);
            }

            return linq;
        }

        private void EnterScope(ParameterExpression[] parameters)
        {
            if (_parameters.Count > 0)
                _parameters.Push(_parameters.Peek().Union(parameters));
            else
                _parameters.Push(parameters);
        }

        private void LeaveScope()
        {
            _parameters.Pop();
        }

        private Expression ProcessQueryExpression(ParseTreeNode expNode)
        {
            var op = expNode.FindTokenAndGetText();

            switch (expNode.GetName())
            {
                default:
                    if (expNode.ChildNodes.Count == 0) break;
                    return ProcessQueryExpression(expNode.FirstChild);
                
                case "query_list":
                    foreach (var sub in expNode.ChildNodes)
                    {
                        ProcessQueryExpression(sub);
                    }
                    break;

                case "query_from_expression":
                    var isFirst = _linqVariables.Count == 0;
                    if (!isFirst)
                        EnterScope(new[] { _linqVariables.Peek().NextParameter });
                    var source = ProcessExpression(expNode.LastChild);
                    if (!isFirst)
                        LeaveScope();
                    var parameterType = source.GetElementType();
                    var parameterName = expNode.GetChild("anonymous_function_parameter_decl").FindTokenAndGetText();
                    var parameter = Expression.Parameter(parameterType, parameterName);
                    if (isFirst) 
                    {
                        _linqVariables.Push(new LINQContext
                        {
                            Parameter = parameter,
                            Name = GetTransparentIdentifier(),
                            IsFirst = true,
                            Self = source
                        });
                    } 
                    else 
                    {
                        var prev = _linqVariables.Peek();

                        var body = GetAnonymousExpression(
                            new[] { prev.NextParameter.Name, parameterName },
                            new[] { prev.NextParameter.Type, parameterType },
                            new[] {prev.NextParameter, parameter}
                        );

                        var name = GetTransparentIdentifier();
                        var from = Expression.Call(MakeMethod(op, prev.NextParameter.Type, parameterType, body.Type), 
                                prev.Self,
                                Expression.Lambda(Expression.Convert(source, typeof(IEnumerable<>).MakeGenericType(parameterType)), prev.NextParameter),
                                Expression.Lambda(body, prev.NextParameter, parameter)
                            );
                        _linqVariables.Push(new LINQContext
                        {
                            Parameter = parameter,
                            Name = name,
                            Self = from
                        });
                    }
                    break;
                case "query_where_expression":
                    GetQueryExpression(op, expNode.LastChild, true);
                    break;
                case "query_orderby_expression":
                    var orderbylist = expNode.GetChild("query_orderby_list");
                    for (int i = 0; i < orderbylist.ChildNodes.Count; ++i)
                    {
                        var orderchild = orderbylist.ChildNodes[i];
                        var order = orderchild.GetChild("query_orderby_order");
                        var orderind = "ascending";
                        if (order != null)
                            orderind = order.FindTokenAndGetText();
                        if (i > 0)
                            op = "thenby";
                        op += orderind == "ascending" ? String.Empty : orderind;
                        GetQueryExpression(op, orderchild.FirstChild, true);
                    }
                    break;
                case "query_let_expression":
                    var id = expNode.ChildNodes[1].FindTokenAndGetText();
                    var last = _linqVariables.Peek();
                    EnterScope(new[] { last.NextParameter });
                    var letBody = ProcessExpression(expNode.LastChild);
                    LeaveScope();
                    var let = GetAnonymousExpression(
                                    new[] { last.NextParameter.Name, id },
                                    new[] { last.NextParameter.Type, letBody.Type },
                                    new[] { last.NextParameter, letBody });
                    var letexp = Expression.Call(MakeMethod(op, last.NextParameter.Type, let.Type),
                                                last.Self,
                                                Expression.Lambda(let, last.NextParameter));
                    _linqVariables.Push(new LINQContext
                    {
                        Parameter = last.NextParameter,
                        Name = GetTransparentIdentifier(),
                        Self = letexp,
                    });
                    break;
                case "query_groupinto_expression":
                    var group = expNode.FirstChild;
                    op = group.FindTokenAndGetText();
                    GetQueryExpression(op, group.LastChild);
                    var into = expNode.LastChild;
                    var intoid = into.LastChild.FindTokenAndGetText();
                    op = "select";
                    last = _linqVariables.Peek();
                    var grouptype = last.Self.GetElementType();
                    var intoparameter = Expression.Parameter(grouptype, intoid);
                    var intoexp = Expression.Call(MakeMethod(op, grouptype, grouptype),
                                                    last.Self,
                                                    Expression.Lambda(intoparameter, intoparameter));
                    _linqVariables.Push(new LINQContext
                    {
                        Parameter = intoparameter,
                        Name = intoparameter.Name,
                        Self = intoexp,
                    });
                    break;
                case "query_join_expression":
                    var joinon = expNode.FirstChild;
                    var joinSource = ProcessExpression(joinon.ChildNodes[3]);
                    var joinParameterName = joinon.GetChild("anonymous_function_parameter_decl").FindTokenAndGetText();
                    var joinParameter = Expression.Parameter(joinSource.GetElementType(), joinParameterName);
                    last = _linqVariables.Peek();
                    EnterScope(new[] { last.NextParameter, joinParameter });
                    var left = ProcessExpression(joinon.ChildNodes[5]);
                    var right = ProcessExpression(joinon.LastChild);
                    LeaveScope();
                    var result = GetAnonymousExpression(new [] { last.NextParameter.Name, joinParameterName },
                                                        new[] { last.NextParameter.Type, joinParameter.Type },
                                                        new [] { last.NextParameter, joinParameter });
                    var joinexp = Expression.Call(
                                    MakeMethod(op, last.Self.GetElementType(), joinSource.GetElementType(), left.GetElementType(), result.Type),
                                    last.Self,
                                    joinSource,
                                    Expression.Lambda(left, last.NextParameter),
                                    Expression.Lambda(right, joinParameter),
                                    Expression.Lambda(result, last.NextParameter, joinParameter));
                    _linqVariables.Push(new LINQContext
                    {
                        Parameter = joinParameter,
                        Name = GetTransparentIdentifier(),
                        Self = joinexp,
                    });
                    break;
                case "query_groupjoin_expression":
                    op = "groupjoin";
                    joinon = expNode.FirstChild;
                    joinSource = ProcessExpression(joinon.ChildNodes[3]);
                    joinParameterName = joinon.GetChild("anonymous_function_parameter_decl").FindTokenAndGetText();
                    joinParameter = Expression.Parameter(joinSource.GetElementType(), joinParameterName);
                    last = _linqVariables.Peek();
                    EnterScope(new[] { last.NextParameter, joinParameter });
                    left = ProcessExpression(joinon.ChildNodes[5]);
                    right = ProcessExpression(joinon.LastChild);
                    LeaveScope();
                    var groupjoinid = expNode.LastChild.LastChild.FindTokenAndGetText();
                    var groupsource = Expression.Convert(joinSource, typeof(IEnumerable<>).MakeGenericType(joinSource.GetElementType()));
                    var groupParameter = Expression.Parameter(groupsource.Type, groupjoinid);
                    result = GetAnonymousExpression(new[] { last.NextParameter.Name, groupjoinid },
                                                        new[] { last.NextParameter.Type, groupsource.Type },
                                                        new Expression[] { last.NextParameter, groupParameter });
                    joinexp = Expression.Call(
                                    MakeMethod(op, last.Self.GetElementType(), joinSource.GetElementType(), left.GetElementType(), result.Type),
                                    last.Self,
                                    joinSource,
                                    Expression.Lambda(left, last.NextParameter),
                                    Expression.Lambda(right, joinParameter),
                                    Expression.Lambda(result, last.NextParameter, groupParameter));

                    _linqVariables.Push(new LINQContext
                    {
                        Parameter = joinParameter,
                        Name = GetTransparentIdentifier(),
                        Self = joinexp,
                    });
                    break;
                case "query_end_expression":
                    var child = expNode.FirstChild;
                    return GetQueryExpression(op, child.LastChild);
            }

            return Expression.Empty();
        }

        private string GetTransparentIdentifier()
        {
            return String.Format("{1}{0}", _linqVariables.Count, TransparentIdentifier);
        }

        private Expression GetAnonymousExpression(IEnumerable<string> names, IEnumerable<Type> types, IEnumerable<Expression> membs)
        {
            var newobj = GetAnonymousType(names.ToArray(), types.ToArray());
            return Expression.New(newobj.GetConstructor(types.ToArray()), membs, newobj.GetProperties());
        }

        private Expression GetQueryExpression(string op, ParseTreeNode expNode, bool inhiretParameter = false)
        {
            var lastVariable = _linqVariables.Peek();
            EnterScope(new[] { lastVariable.NextParameter });
            var queryBody = ProcessExpression(expNode);
            LeaveScope();
            var exp = Expression.Call(MakeMethod(op, lastVariable.NextParameter.Type, queryBody.Type),
                            lastVariable.Self,
                            Expression.Lambda(queryBody, lastVariable.NextParameter));
            var newVariable = new LINQContext
            {
                Parameter = lastVariable.NextParameter,
                Name = GetTransparentIdentifier(),
                Self = exp,
            };

            if (inhiretParameter)
                newVariable.NextParameter = lastVariable.NextParameter;

            _linqVariables.Push(newVariable);

            return exp;
        }

        private Expression ProcessLambdaExpression(ParseTreeNode expNode)
        {
            List<ParameterExpression> arglist = new List<ParameterExpression>();

            var args = expNode.GetChild("lambda_function_signature").GetDescendant("anonymous_function_parameter_list_opt");
            if (args == null) 
            {
                var child = expNode.GetChild("lambda_function_signature");
                arglist.Add(Expression.Parameter(
                        child.GetNodeClrType() ?? _paramTypes.FirstOrDefault() ?? typeof(int), 
                        child.GetChild("Identifier").GetValue())); 
            }
            else 
            { 
                for (int i = 0; i < args.ChildNodes.Count; ++i) 
                {
                    var child = args.ChildNodes[i];
                    arglist.Add(Expression.Parameter(
                        child.GetNodeClrType() ?? _paramTypes.Skip(i).FirstOrDefault() ?? typeof(int), 
                        child.GetChild("Identifier").GetValue()));
                }
            }

            EnterScope(arglist.ToArray());

            var expression = expNode.GetChild("anonymous_function_body");
            var body = ProcessExpression(expression);

            LeaveScope();

            return Expression.Lambda(body, arglist);
        }

        private Expression ProcessConvertExpression(ParseTreeNode expNode)
        {
            var type = expNode.GetChild("typecast_parenthesized_expression").GetNodeClrType();
            var exp = ProcessExpression(expNode.GetChild("primary_expression"));
            return Expression.Convert(exp, type);
        }

        private Expression ProcessUnaryExpression(ParseTreeNode expNode)
        {
            string op;
            var first = expNode.FirstChild;
            var second = expNode.LastChild;
            switch (first.GetName())
            {
                case "unary_operator":
                    op = first.FirstChild.GetValue();
                    break;
                case "pre_incr_decr_expression":
                    op = first.GetChild("incr_or_decr").FirstChild.GetValue();
                    second = first.LastChild;
                    break;
                case "post_incr_decr_expression":
                    op = "post" + first.GetChild("incr_or_decr").FirstChild.GetValue();
                    second = first.FirstChild;
                    break;
                case "typeof_expression":
                    return ProcessTypeofExpression(first);
                default:
                    return ProcessExpression(expNode.FirstChild);
            }
            return Expression.MakeUnary(unaryOps[op], ProcessExpression(second), null);
        }

        private Expression ProcessTypeofExpression(ParseTreeNode expNode)
        {
            var node = expNode.GetChild("typeof_parenthesized_expression").GetDescendant("type_ref_typeof");
            var glist = node.GetDescendant("type_argument_list_opt");
            Type type = node.GetNodeClrType();
            if (glist != null)
            {
                type = Type.GetType(String.Format("{0}`{1}", type.FullName, glist.ChildNodes.Count));
            }
            var rank = node.GetChild("rank_specifiers_opt");
            var ranks = rank.GetDescendant("rank_specifiers");
            if (ranks != null)
            {
                for (int i = ranks.ChildNodes.Count - 1; i >= 0; i--)
                {
                    var r = ranks.ChildNodes[i];
                    var rs = r.ChildNodes.Count;
                    type = rs > 1 ? type.MakeArrayType(rs) : type.MakeArrayType();
                }
            }
            return Expression.Constant(type);
        }

        private Expression ProcessBinaryExpression(ParseTreeNode expNode)
        {
            var left = expNode.ChildNodes[0];
            var right = expNode.ChildNodes[2];
            var op = expNode.ChildNodes[1].FirstChild.GetValue();
            if (op == "as")
            {
                var typeName = right.GetDescendant("Identifier").GetValue();
                return Expression.TypeAs(ProcessExpression(left), GetType(typeName));
            }
            return Expression.MakeBinary(binaryOps[op], ProcessExpression(left), ProcessExpression(right));
        }

        private Expression ConvertType(Expression exp, Type targetType)
        {
            if (exp.Type.IsValueType)
            {
                return Expression.Convert(exp, targetType);
            }

            return Expression.TypeAs(exp, targetType);
        }

        private Expression ProcessConditionalExpression(ParseTreeNode expNode)
        {
            var cond = ProcessExpression(expNode.FirstChild);
            var exp1 = ProcessExpression(expNode.ChildNodes[2]);
            var exp2 = ProcessExpression(expNode.ChildNodes[3]);
            if (exp1.Type != exp2.Type)
            {
                exp2 = ConvertType(exp2, exp1.Type);
            }
            return Expression.Condition(cond, exp1, exp2);
        }
    
        private Expression ProcessMemberAccessExpression(ParseTreeNode expNode)
        {
            Expression self = null;
            ParseTreeNode args;
            List<Expression> arglist;

            var identifier = expNode.GetDescendant("Identifier");
            var members = expNode.LastChild;
            var variableName = identifier.GetValue();
            var parameter = _parameters.Count > 0 ? _parameters.Peek().FirstOrDefault(p => p.Name == variableName) : null;
            if (parameter != null)
            {
                self = parameter;
            }
            else
            {
                var pair = _knownVariables.FirstOrDefault(p => p.Key == variableName);
                if (pair.Key == variableName)
                {
                    self = Expression.Constant(pair.Value);
                }
                else if (_parameters.Count > 0)
                {
                    var parameters = _parameters.Peek();
                    var usedParameter = parameters.FirstOrDefault(p => p.Type.GetMember(variableName).Length > 0);
                    if (usedParameter != null)
                        self = Expression.MakeMemberAccess(usedParameter, usedParameter.Type.GetMember(variableName).First());
                } 
            }
            if (members.ChildNodes.Count == 0)
            {
                if (self == null)
                    throw new Exception(variableName);
                return self;
            }
            foreach (var child in members.ChildNodes)
            {
                Type type;
                if (self == null)
                    type = knownTypes.ContainsKey(variableName) ? knownTypes[variableName] : Type.GetType(variableName) ?? Type.GetType("System." + variableName);
                else
                    type = self.Type;

                if (type == null)
                    throw new Exception(variableName);

                var member = child.LastChild;
                MemberInfo membinfo;
                switch (member.GetName())
                {
                    case "Identifier":
                        if (type == typeof(object)) //Dynamic
                        {
                            var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(CSharpBinderFlags.None,
                                                                            member.GetValue(),
                                                                            type,
                                                                            new[]
                                                                            {
                                                                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                                                            });
                            self = Expression.Dynamic(binder, type, self);
                        }
                        else 
                        {
                            membinfo = type.GetMember(member.GetValue()).First();
                            self = Expression.MakeMemberAccess(self, membinfo);
                        }
                        break;
                    case "member_invoke":
                        var methodName = member.FirstChild.GetValue();
                        var method = type.GetMethod(methodName);
                        var targs = member.GetGenericArguments();
                        if (targs != null)
                        {
                            method = method.MakeGenericMethod(targs);
                        }
                        args = member.GetChild("argument_list");
                        arglist = new List<Expression>();
                        if (args != null)
                        {
                            foreach (var arg in args.ChildNodes)
                            {
                                arglist.Add(ProcessExpression(arg.FirstChild));
                            }
                        }
                        if (method == null)
                        {
                            method = MakeMethod(methodName, new[] { self.GetElementType() }.Union(arglist.Select(arg => arg.Type)).ToArray());
                            self = Expression.Call(method, new[] { self }.Union(arglist));
                        }
                        else
                        {
                            var parameters = method.GetParameters();
                            for (int i = 0; i < parameters.Length; ++i)
                            {
                                if (parameters[i].ParameterType != arglist[i].Type)
                                {
                                    arglist[i] = ConvertType(arglist[i], parameters[i].ParameterType);
                                }
                            }
                            self = Expression.Call(self, method, arglist);
                        }
                        break;
                    case "member_indexer":
                        var indexer = type.GetProperty(member.FirstChild.GetValue());
                        args = member.GetDescendant("expression_list");
                        arglist = new List<Expression>();
                        foreach (var arg in args.ChildNodes)
                        {
                            arglist.Add(ProcessExpression(arg.FirstChild));
                        }
                        self = Expression.MakeIndex(self, indexer, arglist);
                        break;
                    default:
                        throw new Exception(member.GetName());
                }
            }
            return self;
        }

        private Expression ProcessConstantExpression(ParseTreeNode expNode)
        {
            var constant = expNode.FirstChild;
            return Expression.Constant(constant.GetObject());
        }

        private Expression ProcessNewExpression(ParseTreeNode expNode)
        {
            //var typeName = expNode.GetChild("qual_name_with_targs").FindTokenAndGetText();
            var args = expNode.GetChild("creation_args").GetDescendant("argument_list");
            var arglist = new List<Expression>();
            if (args != null)
            {
                foreach (var arg in args.ChildNodes)
                {
                    arglist.Add(ProcessExpression(arg.FirstChild));
                }
            }
            var type = expNode.GetNodeClrType();

            if (expNode.GetChild("creation_args").HasChild("array_creation_args"))
            {
                var elements = expNode.GetChild("array_initializer_opt").GetDescendant("elem_initializer_list");
                var elemlist = new List<Expression>();
                if (elements != null)
                {
                    foreach (var child in elements.ChildNodes)
                    {
                        elemlist.Add(ProcessExpression(child.GetChild("initializer_value").FirstChild));
                    }
                }
                return Expression.NewArrayInit(type, elemlist);
            }

            var newexp = Expression.New(type.GetConstructor(arglist.Select(e => e.Type).ToArray()), arglist);
            var members = expNode.GetChild("array_initializer_opt").GetDescendant("elem_initializer_list");
            var bindlist = new List<MemberBinding>();
            if (members != null)
            {
                foreach (var child in members.ChildNodes)
                {
                    var memberName = child.GetChild("Identifier").GetValue();
                    var expr = ProcessExpression(child.GetChild("initializer_value").FirstChild);
                    bindlist.Add(Expression.Bind(type.GetMember(memberName).First(), expr));
                }
            }
            return Expression.MemberInit(newexp, bindlist);
        }

        private Expression ProcessNewAnonymousExpression(ParseTreeNode expNode)
        {
            if (expNode.HasChild("anonymous_array_creation_expression"))
            {
                var elements = expNode.GetDescendant("element_declarator_list");
                var elemlist = new List<Expression>();
                var elemtype = typeof(void);
                if (elements != null)
                {
                    foreach (var child in elements.ChildNodes)
                    {
                        var exp = ProcessExpression(child.FirstChild);
                        elemtype = exp.Type;
                        elemlist.Add(exp);
                    }
                }
                return Expression.NewArrayInit(elemtype, elemlist);
            }

            var members = expNode.GetDescendant("member_declarator_list");
            var memblist = new List<Expression>();
            var names = new List<string>();
            var types = new List<Type>();
            List<AnonymousMetaProperty> properties = new List<AnonymousMetaProperty>();
            if (members != null)
            {
                foreach (var child in members.ChildNodes)
                {
                    var name = child.GetChild("Identifier").GetValue();
                    var expr = ProcessExpression(child.GetChild("primary_expression"));
                    names.Add(name);
                    types.Add(expr.Type);
                    memblist.Add(expr);
                }
            }

            return GetAnonymousExpression(names.ToArray(), types.ToArray(), memblist.ToArray());
        }

        internal static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
                knownTypes.TryGetValue(typeName, out type);
            return type;
        }
    }

    internal class LINQContext
    {
        public ParameterExpression Parameter { get; set; }
        public string Name { get; set; }
        public Expression Self { get; set; }

        private ParameterExpression nextParameter;
        public ParameterExpression NextParameter
        {
            get
            {
                if (nextParameter != null) return nextParameter;
                nextParameter = IsFirst ? Parameter : Expression.Parameter(Self.GetElementType(), Name);
                return nextParameter;
            }
            set
            {
                nextParameter = value;
            }
        }

        private bool isFirst = false;
        public bool IsFirst
        {
            get { return isFirst; }
            set { isFirst = value; }
        }
    }

    public static class ExpressionExtensions
    {
        public static string ToReadableString(this Expression exp)
        {
            return ExpressionToCodeLib.ExpressionToCode.ToCode(exp);
        }

        internal static Type GetElementType(this Expression exp)
        {
            var args = exp.Type.GetGenericArguments();
            if (args.Length > 0)
                return args[0];
            return exp.Type;
        }
    }

    internal static class ParseTreeNodeExtensions
    {
        public static ParseTreeNode FirstOrDefault(this ParseTreeNode node, Func<ParseTreeNode, bool> predicate)
        {
            if (predicate(node)) return node;
            foreach (var n in node.ChildNodes)
            {
                var found = n.FirstOrDefault(predicate);
                if (found != null) return found;
            }

            return null;
        }

        public static ParseTreeNode GetChild(this ParseTreeNode node, string childName)
        {
            return node.ChildNodes.Find(p => p.GetName() == childName);
        }

        public static ParseTreeNode GetDescendant(this ParseTreeNode node, string childName)
        {
            return node.FirstOrDefault(p => p.GetName() == childName);
        }

        public static string GetName(this ParseTreeNode node)
        {
            return node.Term.Name;
        }

        public static string GetValue(this ParseTreeNode node)
        {
            return node.Token.Text;
        }

        public static object GetObject(this ParseTreeNode node)
        {
            return node.Token.Value;
        }

        public static bool HasChild(this ParseTreeNode node, string childName)
        {
            return node.GetChild(childName) != null;
        }

        public static Type[] GetGenericArguments(this ParseTreeNode node)
        {
            var typeArgs = node.GetDescendant("type_argument_list");
            if (typeArgs == null)
                return null;
            return (from typeChild in node.GetChild("type_ref_list").ChildNodes
                    select typeChild.GetClrType()).ToArray();
        }

        private static Type GetClrType(this ParseTreeNode node)
        {
            if (node == null)
                return null;
            if (node.GetName() != "qual_name_with_targs")
                node = node.GetDescendant("qual_name_with_targs");
            var isNullable = node.GetChild("qmark_opt").FindTokenAndGetText() == "?";
            var typeName = node.FindTokenAndGetText();
            var type = ExpressionParser.GetType(typeName);
            var typeArguments = node.GetGenericArguments();
            if (typeArguments != null)
                type.MakeGenericType(typeArguments);
            if (isNullable)
                return typeof(Nullable<>).MakeGenericType(type);
            return type;
        }

        public static Type GetNodeClrType(this ParseTreeNode node)
        {
            return node.GetDescendant("qual_name_with_targs").GetClrType();
        }

    }

    internal static class TypeExtensions
    {
        private class ParameterComparer : IEqualityComparer<Type>
        {

            #region IEqualityComparer<Type> Members

            public bool Equals(Type x, Type y)
            {
                if (x.IsGenericParameter && y.IsGenericParameter)
                    return true;

                if (x.IsGenericType && y.IsGenericType)
                {
                    if (x.Name != y.Name) return false;
                    if (x.BaseType != y.BaseType) return false;

                    var xa = x.GetGenericArguments();
                    var ya = y.GetGenericArguments();
                    if (xa.Length != ya.Length) return false;

                    var ret = xa.SequenceEqual(ya, this);
                    return ret;
                }

                if (x.IsGenericType) return true;
                if (!y.IsGenericType) return true;

                return x.Equals(y);
            }

            public int GetHashCode(Type obj)
            {
                return obj.GetHashCode();
            }

            #endregion
        }

        public static MethodInfo GetExtensionMethod(this Type type, string name, params Type[] arguments)
        {
            var query = from method in type.GetMethods()
                        let parameters = from p in method.GetParameters() select p.ParameterType
                        where method.Name == name
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where parameters.SequenceEqual(arguments, new ParameterComparer())
                        select method;

            return query.FirstOrDefault();
        }
    }

}
