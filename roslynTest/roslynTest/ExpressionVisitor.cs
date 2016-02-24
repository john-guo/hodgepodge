using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq.Expressions;

namespace roslynTest
{
    internal class ExpressionVisitor : CSharpSyntaxWalker
    {
        private static Dictionary<string, ExpressionType> binaryOps = new Dictionary<string, ExpressionType>();
        private static Dictionary<string, ExpressionType> unaryOps = new Dictionary<string, ExpressionType>();

        static ExpressionVisitor()
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
            #endregion
        }

        public Dictionary<string, ParameterExpression> parameters;
        public Expression expression;
        public List<Type> paramTypes;
        public Dictionary<string, object> knownVariables;

        public ExpressionVisitor()
        {
            expression = null;
            parameters = new Dictionary<string, ParameterExpression>();
            paramTypes = new List<Type>();
            knownVariables = new Dictionary<string, object>();
        }

        private void Clear()
        {
            expression = null;
            parameters.Clear();
        }

        public override void VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            Clear();
            base.VisitExpressionStatement(node);
        }

        public override void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            expression = Expression.Constant(node.Token.Value);
        }

        public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            base.VisitPostfixUnaryExpression(node);

            var op = node.OperatorToken.ValueText;
            if (op == "++" || op == "--")
            {
                op = "post" + op; 
            }
            expression = Expression.MakeUnary(unaryOps[op], expression, null);
        }

        public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            base.VisitPrefixUnaryExpression(node);
            
            var op = node.OperatorToken.ValueText;
            expression = Expression.MakeUnary(unaryOps[op], expression, null);
        }

        public override void VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            base.VisitTypeOfExpression(node);
        }

        public override void VisitCastExpression(CastExpressionSyntax node)
        {
            base.VisitCastExpression(node);
        }

        public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            base.VisitAnonymousMethodExpression(node);
        }

        public override void VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
        {
            base.VisitAnonymousObjectCreationExpression(node);
        }

        public override void VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
        {
            base.VisitArrayCreationExpression(node);
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            Visit(node.Left);
            var left = expression;

            Visit(node.Right);
            var right = expression;

            expression = Expression.Assign(left, right);
        }

        public override void VisitBaseExpression(BaseExpressionSyntax node)
        {
            base.VisitBaseExpression(node);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            Visit(node.Left);
            var left = expression;

            Visit(node.Right);
            var right = expression;

            var op = binaryOps[node.OperatorToken.ValueText];
            expression = Expression.MakeBinary(op, left, right);
        }

        public override void VisitDefaultExpression(DefaultExpressionSyntax node)
        {
            base.VisitDefaultExpression(node);
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            Visit(node.Condition);
            var cond = expression;

            Visit(node.WhenTrue);
            var wt = expression;

            Visit(node.WhenFalse);
            var wf = expression;

            expression = Expression.Condition(cond, wt, wf);
        }

        public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
        {
            base.VisitConditionalAccessExpression(node);
        }

        public override void VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
        {
            base.VisitImplicitArrayCreationExpression(node);
        }

        public override void VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            base.VisitInitializerExpression(node);
        }

        public override void VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
        {
            base.VisitInterpolatedStringExpression(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            base.VisitInvocationExpression(node);
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            base.VisitMemberAccessExpression(node);
        }

        public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            base.VisitElementAccessExpression(node);
        }

        public override void VisitElementBindingExpression(ElementBindingExpressionSyntax node)
        {
            base.VisitElementBindingExpression(node);
        }

        public override void VisitMakeRefExpression(MakeRefExpressionSyntax node)
        {
            base.VisitMakeRefExpression(node);
        }

        public override void VisitMemberBindingExpression(MemberBindingExpressionSyntax node)
        {
            base.VisitMemberBindingExpression(node);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            base.VisitObjectCreationExpression(node);
        }

        public override void VisitOmittedArraySizeExpression(OmittedArraySizeExpressionSyntax node)
        {
            base.VisitOmittedArraySizeExpression(node);
        }

        public override void VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            base.VisitParenthesizedExpression(node);
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            for (int i = 0; i < node.ParameterList.Parameters.Count; ++i)
            {
                var name = node.ParameterList.Parameters[i].Identifier.ValueText;
                parameters[name] = Expression.Parameter(paramTypes[i], name);
            }

            base.VisitParenthesizedLambdaExpression(node);

            expression = Expression.Lambda(expression, parameters.Values);
        }

        public override void VisitQueryExpression(QueryExpressionSyntax node)
        {
            base.VisitQueryExpression(node);
        }

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            var name = node.Parameter.Identifier.ValueText;
            parameters[name] = Expression.Parameter(paramTypes[0], name);

            base.VisitSimpleLambdaExpression(node);

            expression = Expression.Lambda(expression, parameters.Values);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            base.VisitIdentifierName(node);

            var id = node.Identifier.ValueText;
            if (parameters.ContainsKey(id))
            {
                expression = parameters[id];
            }
            else if (knownVariables.ContainsKey(id))
            {
                expression = Expression.Constant(knownVariables[id]);
            }
        }
    }
}
