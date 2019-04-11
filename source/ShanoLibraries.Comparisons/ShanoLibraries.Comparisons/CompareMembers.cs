using System;
using System.Collections;
using System.Collections.Generic;

namespace ShanoLibraries.Comparisons
{
    internal class CompareMembers
    {
        public static int GenericEnumerable<T>(IEnumerable<T> x, IEnumerable<T> y, Comparison<T> comparison)
        {
            if (TryCompareNullness(x, y, out int result)) return result;
            if (TryCompareCount(x, y, out result)) return result;

            result = ComparisonResult.Equality;
            IEnumerator<T> xEnumerator = x.GetEnumerator();
            IEnumerator<T> yEnumerator = y.GetEnumerator();

            while (
                xEnumerator.MoveNext() &&
                yEnumerator.MoveNext() &&
                result == ComparisonResult.Equality
            ) result = comparison(xEnumerator.Current, yEnumerator.Current);

            if (result == ComparisonResult.Equality)
            {
                if (xEnumerator.MoveNext()) return ComparisonResult.RightFirst;
                if (yEnumerator.MoveNext()) return ComparisonResult.LeftFirst;
            }

            return result;
        }

        public static int NonGenericEnumerable(IEnumerable x, IEnumerable y)
        {
            if (TryCompareNullness(x, y, out int result)) return result;
            if (TryCompareCount(x, y, out result)) return result;

            result = ComparisonResult.Equality;
            IEnumerator xEnumerator = x.GetEnumerator();
            IEnumerator yEnumerator = y.GetEnumerator();

            while (
                xEnumerator.MoveNext() &&
                yEnumerator.MoveNext() &&
                result == ComparisonResult.Equality
            ) result = Objects(xEnumerator.Current, yEnumerator.Current);

            if(result == ComparisonResult.Equality)
            {
                if (xEnumerator.MoveNext()) return ComparisonResult.RightFirst;
                if (yEnumerator.MoveNext()) return ComparisonResult.LeftFirst;
            }

            return result;
        }

        public static int Objects(object x, object y)
        {
            if(TryCompareNullness(x, y, out int nullness))return nullness;
            if (x is IEnumerable xEnumerable && y is IEnumerable yEnumerable) return NonGenericEnumerable(xEnumerable, yEnumerable);
            return Comparer.Default.Compare(x, y);
        }

        public static int ComparableClasses<T>(T x, T y) where T : class, IComparable =>
            CompareNullness(x, y) ?? x.CompareTo(y);

        public static int GenericComparableClasses<T>(T x, T y) where T : class, IComparable<T> =>
            CompareNullness(x, y) ?? x.CompareTo(y);

        public static int ComparableStructs<T>(T x, T y) where T : struct, IComparable =>
            x.CompareTo(y);

        public static int GenericComparableStructs<T>(T x, T y) where T : struct, IComparable<T> =>
            x.CompareTo(y);

        public static int ComparableNullables<T>(T? x, T? y) where T : struct, IComparable =>
            CompareNullness(x, y) ?? x.Value.CompareTo(y.Value);

        public static int GenericComparableNullables<T>(T? x, T? y) where T : struct, IComparable<T> =>
            CompareNullness(x, y) ?? x.Value.CompareTo(y.Value);


        private static bool TryCompareCount(IEnumerable x, IEnumerable y, out int countComparison) =>
            (countComparison = CompareCount(x, y) ?? ComparisonResult.Equality) != ComparisonResult.Equality;

        private static bool TryCompareNullness<T>(T x, T y, out int nullnessComparison) where T : class =>
            (nullnessComparison = CompareNullness(x, y) ?? ComparisonResult.Equality) != ComparisonResult.Equality;

        private static bool TryCompareNullness<T>(T? x, T? y, out int nullnessComparison) where T : struct =>
            (nullnessComparison = CompareNullness(x, y) ?? ComparisonResult.Equality) != ComparisonResult.Equality;

        private static int? CompareCount(IEnumerable x, IEnumerable y)
        {
            if (x is ICollection xCollection && y is ICollection yCollection)
            {
                int lengthComparison = xCollection.Count.CompareTo(yCollection.Count);
                if (lengthComparison != ComparisonResult.Equality) return lengthComparison;
            }

            return null;
        }

        private static int? CompareNullness<T>(T x, T y) where T : class
        {
            if (x is null) return y is null ? ComparisonResult.Equality : ComparisonResult.LeftFirst;
            else if (y is null) return ComparisonResult.RightFirst;
            else return null;
        }

        private static int? CompareNullness<T>(T? x, T? y) where T : struct
        {
            if (x is null) return y is null ? ComparisonResult.Equality : ComparisonResult.LeftFirst;
            else if (y is null) return ComparisonResult.RightFirst;
            else return null;
        }
    }
}
