using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LINQExtension
{
    public static class DoubleOperations
    {
        public static double Median(this IEnumerable<double> source)
        {
            if (source.Count() == 0)
            {
                Debug.LogError("Cannot compute median for an empty set.");
            }

            var sortedList = from number in source
                orderby number
                select number;

            var itemIndex = (int)sortedList.Count() / 2;

            if (sortedList.Count() % 2 == 0)
            {
                // Even number of items.
                return (sortedList.ElementAt(itemIndex) + sortedList.ElementAt(itemIndex - 1)) / 2;
            }
            else
            {
                // Odd number of items.
                return sortedList.ElementAt(itemIndex);
            }
        }
    }
}