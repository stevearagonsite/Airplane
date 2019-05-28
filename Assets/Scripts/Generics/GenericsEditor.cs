using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GenericsMethods
{
    public static class GenericsEditor
    {
        public static void Spaces(int value)
        {
            for (int i = 0; i < value; i++)
            {
                EditorGUILayout.Space();
            }
        }

        public static void Rect(float size,Color color)
        {
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(100, size), color);
        }
    }

    public static class TextStyles
    {
        private static readonly GUIStyle _h1;
        private static readonly GUIStyle _h2;
        private static readonly GUIStyle _h3;
        private static readonly GUIStyle _h4;
        private static readonly GUIStyle _h5;
        private static readonly GUIStyle _h6;
        private static readonly GUIStyle _h7;

        public static GUIStyle h1 { get { return new GUIStyle(_h1); } }
        public static GUIStyle h2 { get { return new GUIStyle(_h2); } }
        public static GUIStyle h3 { get { return new GUIStyle(_h3); } }
        public static GUIStyle h4 { get { return new GUIStyle(_h4); } }
        public static GUIStyle h5 { get { return new GUIStyle(_h5); } }
        public static GUIStyle h6 { get { return new GUIStyle(_h6); } }
        public static GUIStyle h7 { get { return new GUIStyle(_h7); } }

        static TextStyles()
        {
            //Define _h1
            var styleH1 = new GUIStyle();
            styleH1.font = Resources.Load<Font>("Fonts/Roboto/Roboto-Bold");
            styleH1.fontSize = 35;

            _h1 = styleH1;

            //Define _h2
            var styleH2 = new GUIStyle();
            styleH2.font = Resources.Load<Font>("Fonts/Roboto/Roboto-Medium");
            styleH2.fontSize = 30;

            _h2 = styleH2;
            
            //Define _h3
            var styleH3 = new GUIStyle();
            styleH3.font = Resources.Load<Font>("Fonts/Roboto/Roboto-Regular");
            styleH3.fontSize = 25;

            _h3 = styleH3;

            //Define _h4
            var styleH4 = new GUIStyle();
            styleH4.font = Resources.Load<Font>("Fonts/Roboto/Roboto-Regular");
            styleH4.fontSize = 18;

            _h4 = styleH4;

            //Define _h5
            var styleH5 = new GUIStyle();
            styleH5.font = Resources.Load<Font>("Fonts/Roboto/Roboto-Light");
            styleH5.fontSize = 16;

            _h5 = styleH5;
            
            //Define _h6
            var styleH6 = new GUIStyle();
            styleH6.font = Resources.Load<Font>("Fonts/Roboto/Roboto-Light");
            styleH6.fontSize = 13;

            _h6 = styleH6;
            
            //Define _h7
            var styleH7 = new GUIStyle();
            styleH7.font = Resources.Load<Font>("Fonts/Roboto/Roboto-Thin");
            styleH7.fontSize = 10;

            _h7 = styleH7;
        }
    }
}
