using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ShanoLibraries.Comparisons
{
    public static class GenerateComparison
    {
        public static Comparison<T> MemberwiseExcept<T>(params string[] excludedMemberNames) =>
            MemberwiseInternal<T>(GetEligibleMemberNames<T>().Except(excludedMemberNames));

        public static Comparison<T> Memberwise<T>(params string[] memberNames) =>
            MemberwiseInternal<T>(memberNames);

        public static Comparison<T> Memberwise<T>() =>
            MemberwiseInternal<T>(GetEligibleMemberNames<T>());

        private static Comparison<T> MemberwiseInternal<T>(IEnumerable<string> memberNames)
        {
            if (!memberNames.Any()) return (x, y) => ComparisonResult.Equality;

            Type type = typeof(T);
            ParameterExpression 
                left = Expression.Parameter(type, "left"),
                right = Expression.Parameter(type, "right");

            var memberComparisons = new List<Expression>(capacity: (memberNames as ICollection)?.Count ?? 10);
            foreach(string memberName in memberNames)
            {
                Expression
                    leftMember = Expression.PropertyOrField(left, memberName),
                    rightMember = Expression.PropertyOrField(right, memberName);

                memberComparisons.Add(GenerateComparisonExpression(left, right));
            }

            Expression result = GenerateExpression.FirstNotEqualToIfSuch(
                type: typeof(int),
                notEqualTo: Expression.Constant(ComparisonResult.Equality, typeof(int)),
                expressions: memberComparisons
            );

            var lambda = Expression.Lambda<Comparison<T>>(body: result, parameters: new[] { left, right });

            return lambda.Compile();
        }

        private static Expression GenerateComparisonExpression(
            Expression left,
            Expression right
        )
        {
            if (left.Type != right.Type) throw new ArgumentException("Left and right must have the same type");

            Type type = left.Type;

            string methodName;
            Type[] typeArguments;
            Expression[] arguments;

            if (type.ImplementsGenericComparable())
            {
                methodName =
                    type.IsValueType ?
                    nameof(CompareMembers.GenericComparableStructs) :
                    nameof(CompareMembers.GenericComparableClasses);

                typeArguments = new[] { type };
                arguments = new[] { left, right };
            }
            else if (type.ImplementsComparable())
            {
                methodName =
                    type.IsValueType ?
                    nameof(CompareMembers.ComparableStructs) :
                    nameof(CompareMembers.ComparableClasses);

                typeArguments = new[] { type };
                arguments = new[] { left, right };
            }
            else if (type.IsNullable(out Type underlyingValueType))
            {
                if (underlyingValueType.ImplementsGenericComparable())
                {
                    methodName = nameof(CompareMembers.GenericComparableNullables);
                    typeArguments = new[] { underlyingValueType };
                    arguments = new[] { left, right };
                }
                else if (underlyingValueType.ImplementsComparable())
                {
                    methodName = nameof(CompareMembers.ComparableNullables);
                    typeArguments = new[] { underlyingValueType };
                    arguments = new[] { left, right };
                }
                else if (underlyingValueType.ImplementsGenericEnumerable(out Type itemType))
                {
                    Type itemComparisonType = typeof(Comparison<>).MakeGenericType(itemType);
                    Delegate itemComparisonValue = GenerateComparisonFunction(itemType);
                    Expression itemComparison = Expression.Constant(itemComparisonValue, itemComparisonType);

                    methodName = nameof(CompareMembers.GenericEnumerable);
                    typeArguments = new[] { itemType };
                    arguments = new[] { left, right, itemComparison };

                }
                else if (underlyingValueType.ImplementsEnumerable())
                {
                    methodName = nameof(CompareMembers.NonGenericEnumerable);
                    typeArguments = null;
                    arguments = new[] { left, right };
                }
                else
                {

                    methodName = nameof(CompareMembers.Objects);
                    typeArguments = null;
                    arguments = new[] { left, right };
                }
            }
            else if (type.ImplementsGenericEnumerable(out Type itemType))
            {
                Type itemComparisonType = typeof(Comparison<>).MakeGenericType(itemType);
                Delegate itemComparisonFunctionValue = GenerateComparisonFunction(type);
                Expression itemComparisonFunction = Expression.Constant(itemComparisonFunctionValue, itemComparisonType);


                methodName = nameof(CompareMembers.GenericEnumerable);
                typeArguments = new[] { itemType };
                arguments = new[] { left, right, itemComparisonFunction };
            }
            else if (type.ImplementsEnumerable())
            {
                methodName = nameof(CompareMembers.NonGenericEnumerable);
                typeArguments = null;
                arguments = new[] { left, right };
            }
            else
            {
                methodName = nameof(CompareMembers.Objects);
                typeArguments = null;
                arguments = new[] { left, right };
            }

            return Expression.Call(
                typeof(CompareMembers),
                methodName,
                typeArguments,
                arguments
            );
        }

        private static Delegate GenerateComparisonFunction(Type type)
        {
            ParameterExpression left = Expression.Parameter(type, "x"), right = Expression.Parameter(type, "y");
            Expression comparison = GenerateComparisonExpression(left, right);
            LambdaExpression lambda = Expression.Lambda(
                delegateType: typeof(Comparison<>).MakeGenericType(type),
                body: comparison,
                parameters: new[] { left, right }
            );
            return lambda.Compile();
        }

        private static IEnumerable<string> GetEligibleMemberNames<T>()
        {
            Type type = typeof(T);

            IEnumerable<string> fieldNames =
                type
                .GetRuntimeFields()
                .Where(x => !x.IsCompilerGenerated())
                .Select(x => x.Name);

            IEnumerable<string> propertyNames =
                type
                .GetRuntimeProperties()
                .Where(x => !x.IsCompilerGenerated())
                .Select(x => x.Name);

            IEnumerable<string> eligibleNames = fieldNames.Concat(propertyNames);
            return eligibleNames;
        }
    }
}
