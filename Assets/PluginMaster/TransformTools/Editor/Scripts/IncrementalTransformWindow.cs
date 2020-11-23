/*
Copyright (c) 2020 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2020.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using UnityEngine;
using UnityEditor;

namespace PluginMaster
{
    public abstract class IncrementalTransformWindow : BaseToolWindow
    {
        protected TransformTools.IncrementalData _data = new TransformTools.IncrementalData();
        protected enum Attribute
        {
            POSITION,
            ROTATION,
            SCALE
        }
        protected Attribute _attribute = Attribute.POSITION;
        
        protected override void OnGUI()
        {
            base.OnGUI();
            
            var title = "Incremental " + (_attribute == Attribute.POSITION ? "Position" : _attribute == Attribute.ROTATION ? "Rotation" : "Scale");
            titleContent = new GUIContent(title, null, title);
            GUILayout.BeginVertical();
            {
                ToolSettings();
                GUILayout.Space(2);
                GUILayout.BeginHorizontal();
                {
                    var statusStyle = new GUIStyle(EditorStyles.label);

                    GUILayout.Space(8);
                    var statusMessage = "";
                    if (_selectionOrderedTopLevel.Count == 0)
                    {
                        statusMessage = "No objects selected.";
                        GUILayout.Label(new GUIContent(Resources.Load<Texture2D>("Sprites/Warning")), new GUIStyle() { alignment = TextAnchor.LowerLeft });
                    }
                    else
                    {
                        statusMessage = _selectionOrderedTopLevel.Count + " objects selected.";
                    }
                    GUILayout.Label(statusMessage, statusStyle);
                    GUILayout.FlexibleSpace();
                    EditorGUI.BeginDisabledGroup(_selectionOrderedTopLevel.Count == 0);
                    if (GUILayout.Button("Apply", EditorStyles.miniButtonRight))
                    {
                        Apply();
                    }
                    EditorGUI.EndDisabledGroup();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        protected virtual void ToolSettings()
        {
            EditorGUIUtility.labelWidth = 74;
            EditorGUIUtility.fieldWidth = 110;
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                _data.arrangeOrder = (TransformTools.ArrangeBy)EditorGUILayout.Popup("Arrange by:", (int)_data.arrangeOrder, new string[] { "Selection order", "Hierarchy order" });
            }
            GUILayout.EndHorizontal();
            EditorGUIUtility.fieldWidth = 100;
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    _data.type = (TransformTools.IncrementalDataType)EditorGUILayout.Popup("Value Type:", (int)_data.type,
                        _attribute == Attribute.POSITION ? new string[] { "Constant delta", "Curve", "Object Size"}
                        : new string[] { "Constant delta", "Curve"});
                }
                GUILayout.EndHorizontal();
                if (_data.type == TransformTools.IncrementalDataType.CONSTANT_DELTA)
                {
                    minSize += new Vector2(0, 40);
                    GUILayout.BeginHorizontal();
                    {
                        _data.constantDelta = EditorGUILayout.Vector3Field("Value:", _data.constantDelta, GUILayout.Width(200));
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                }
                else if (_data.type == TransformTools.IncrementalDataType.CURVE)
                {
                    minSize  += new Vector2(0, 120);
                    GUILayout.BeginHorizontal();
                    {
                        _data.curveRangeMin = EditorGUILayout.Vector3Field("Min: ", _data.curveRangeMin, GUILayout.Width(200));
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    {
                        _data.curveRangeSize = EditorGUILayout.Vector3Field("Size: ", _data.curveRangeSize, GUILayout.Width(200));
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Label("Value:");
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUIUtility.labelWidth = 10;
                        EditorGUIUtility.fieldWidth = 44;
                        GUILayout.Space(15);
                        _data.x.curve = EditorGUILayout.CurveField("X", _data.x.curve, Color.red, _data.GetRect(TransformTools.Axis.X));
                        GUILayout.Space(1);
                        _data.y.curve = EditorGUILayout.CurveField("Y", _data.y.curve, Color.green, _data.GetRect(TransformTools.Axis.Y));
                        GUILayout.Space(1);
                        _data.z.curve = EditorGUILayout.CurveField("Z", _data.z.curve, Color.blue, _data.GetRect(TransformTools.Axis.Z));
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.Label("Ovewrite:");
                GUILayout.BeginHorizontal();
                {
                    EditorGUIUtility.labelWidth = 10;
                    EditorGUIUtility.fieldWidth = 44;
                    GUILayout.Space(15);
                    _data.x.overwrite = EditorGUILayout.Toggle("X", _data.x.overwrite);
                    GUILayout.Space(31);
                    _data.y.overwrite = EditorGUILayout.Toggle("Y", _data.y.overwrite);
                    GUILayout.Space(31);
                    _data.z.overwrite = EditorGUILayout.Toggle("Z", _data.z.overwrite);
                    GUILayout.FlexibleSpace();
                    
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }
            GUILayout.EndVertical();
        }

        protected abstract void Apply();
    }

    public class IncrementalPositionWindow : IncrementalTransformWindow
    {
        private bool _orientToPath = false;
        private static Vector3[] _directions =
        {
            Vector3.right, Vector3.left,
            Vector3.up, Vector3.down,
            Vector3.forward, Vector3.back
        };
        private int _dirIdx = 0;


        [MenuItem("Tools/Plugin Master/Transform Tools/Incremental Position", false, 1300)]
        public static void ShowWindow()
        {
            GetWindow<RandomizePositionsWindow>();
        }

        protected override void Apply()
        {
            TransformTools.IncrementalPosition(_selectionOrderedTopLevel, _data, _orientToPath, _directions[_dirIdx]);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _attribute = IncrementalTransformWindow.Attribute.POSITION;
        }

        protected override void ToolSettings()
        {
            minSize = new Vector2(220, 155);
            base.ToolSettings();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUIUtility.labelWidth = 74;
                EditorGUIUtility.fieldWidth = 44;
                _orientToPath = EditorGUILayout.BeginToggleGroup("Orient to the path", _orientToPath);
                {
                    GUILayout.BeginHorizontal();
                    {
                        _dirIdx = EditorGUILayout.Popup("Object axis:", _dirIdx, new string[] { "+X", "-X", "+Y", "-Y", "+Z", "-Z" });
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.EndToggleGroup();
            }
            GUILayout.EndVertical();
        }
    }
    public class IncrementalRotationWindow : IncrementalTransformWindow
    {
        [MenuItem("Tools/Plugin Master/Transform Tools/Incremental Rotation", false, 1300)]
        public static void ShowWindow()
        {
            GetWindow<RandomizePositionsWindow>();
        }

        protected override void Apply()
        {
            TransformTools.IncrementalRotation(_selectionOrderedTopLevel, _data);

        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _attribute = IncrementalTransformWindow.Attribute.ROTATION;
        }
        protected override void ToolSettings()
        {
            minSize = new Vector2(220, 110);
            base.ToolSettings();
        }
    }

    public class IncrementalScaleWindow : IncrementalTransformWindow
    {
        [MenuItem("Tools/Plugin Master/Transform Tools/Incremental Scale", false, 1300)]
        public static void ShowWindow()
        {
            GetWindow<RandomizePositionsWindow>();
        }

        protected override void Apply()
        {
            TransformTools.IncrementalScale(_selectionOrderedTopLevel, _data);

        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _attribute = IncrementalTransformWindow.Attribute.SCALE;
        }

        protected override void ToolSettings()
        {
            minSize = new Vector2(220, 110);
            base.ToolSettings();
        }
    }
}
