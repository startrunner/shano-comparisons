using System;
using System.Collections.Generic;
using System.Text;

namespace ShanoLibraries.Comparisons
{
    internal class ConcurrentAddOnlyList<T>
    {
        readonly object thisLock = new object();
        readonly List<T> items = new List<T>();

        public int Add(T item)
        {
            int index;
            lock(thisLock)
            {
                index = items.Count;
                items.Add(item);
            }
            return index;
        }
    }
}
