using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ShanoLibraries.Comparisons
{
    public static class GenerateExpression
    {
        public static Expression FirstNotEqualToIfSuch(Type type, Expression notEqualTo, IReadOnlyList<Expression> expressions)
        {
            ParameterExpression resultVariable = Expression.Variable(type, "result");

            Expression resultInitialize = Expression.Assign(resultVariable, Expression.Default(type));
            Expression ternary = resultVariable;

            foreach (Expression expression in expressions.Reverse())
            {
                ternary = Expression.Condition(
                    test: Expression.NotEqual(Expression.Assign(resultVariable, expression), notEqualTo),
                    ifTrue: resultVariable,
                    ifFalse: ternary
                );
            }

            BlockExpression body = Expression.Block(
                variables: new[] { resultVariable },
                expressions: new[] {
                    resultInitialize,
                    ternary
                }
            );

            return body;
        }
    }
}
