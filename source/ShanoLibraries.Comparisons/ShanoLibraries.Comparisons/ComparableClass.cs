using System;

namespace ShanoLibraries.Comparisons
{
    public class ComparableClass : IComparable, IComparable<ComparableClass>, IEquatable<ComparableClass>
    {
        static readonly Comparison<ComparableClass> Comparison = GenerateComparison.Memberwise<ComparableClass>();

        public int CompareTo(ComparableClass other) => Comparison(this, other);
        public int CompareTo(object obj) => throw new NotImplementedException();
        public bool Equals(ComparableClass other) => Comparison(this, other) == ComparisonResult.Equality;
    }
}
