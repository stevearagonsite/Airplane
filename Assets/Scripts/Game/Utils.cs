using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Utils
{
    public static class Rand
    {
        private static System.Random _rnd = new System.Random();
        public static float uniform
        {
            //Skip the rounding to 1.0 of the floats (0, 8388606) / 8388607f).
            get { return ((float)_rnd.Next(0, 8388606) / 8388607f); }
        }

        public static readonly Func<float, float, float> Range = (minValue, maxValue) => {
            if (minValue > maxValue) Debug.LogError("incorrectly set the number.");

            return uniform * (maxValue - minValue);
        };
    }
    
    public static class ParserData
    {
        public static int ConvertStringToInt(string intString)
        {
            int i = 0;
            return (Int32.TryParse(intString, out i) ? i : -1);
        }
    }
}