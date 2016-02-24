using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace roslynTest
{
    public class ExpressionParser
    {
        private ExpressionVisitor visitor;

        public ExpressionParser()
        {
            visitor = new ExpressionVisitor();
        }

        public Expression<TDelegate> Parse<TDelegate>(string expressionText)
        {
            var tree = CSharpSyntaxTree.ParseText(String.Format(@"_(){{{0}", expressionText));

            visitor.paramTypes.Clear();
            var genericParameters = typeof(TDelegate).GetGenericTypeDefinition().GetGenericArguments();
            var realArguments = typeof(TDelegate).GetGenericArguments();
            visitor.paramTypes.AddRange(
                realArguments.Where((a, i) =>
                    genericParameters.First(p => p.GenericParameterPosition == i).
                    GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant)));

            visitor.Visit(tree.GetRoot());

            var genericlambda = visitor.expression as Expression<TDelegate>;
            if (genericlambda != null)
                return genericlambda;

            var lambda = visitor.expression as LambdaExpression;
            if (lambda != null)
            {
                return Expression.Lambda<TDelegate>(lambda.Body, lambda.Parameters);
            }

            return Expression.Lambda<TDelegate>(visitor.expression);
        }
    }
}
