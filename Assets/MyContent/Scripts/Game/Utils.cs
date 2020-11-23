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
    
    public static class RectTransformExtensions
    {
        public static void SetLeft(this RectTransform rt, float left)
        {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }
 
        public static void SetRight(this RectTransform rt, float right)
        {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }
 
        public static void SetTop(this RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }
 
        public static void SetBottom(this RectTransform rt, float bottom)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }
        
    }
}