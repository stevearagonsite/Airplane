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
    public abstract class RandomizeToolWindow : BaseToolWindow
    {
        protected TransformTools.RandomizeData _data = new TransformTools.RandomizeData();

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
            var attributeName = _attribute == Attribute.POSITION ? "Positions" : _attribute == Attribute.ROTATION ? "Rotations" : "Scales";
            titleContent = new GUIContent("Randomize " + attributeName, null, "Randomize " + attributeName);
            EditorGUIUtility.labelWidth = 30;
            EditorGUIUtility.fieldWidth = 70;
            GUILayout.BeginVertical();
            {
                OnGUIValue();
                GUILayout.Space(8);
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
                    if (GUILayout.Button("Randomize", EditorStyles.miniButtonRight))
                    {
                        Randomize();
                    }
                    EditorGUI.EndDisabledGroup();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        protected virtual void OnGUIValue()
        {
            minSize = new Vector2(240, 180);
            GUILayout.BeginVertical(EditorStyles.helpBox); //X
            {
                _data.x.randomizeAxis = EditorGUILayout.BeginToggleGroup("Randomize X", _data.x.randomizeAxis);
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        _data.x.offset.min = EditorGUILayout.FloatField("min:", _data.x.offset.min, EditorStyles.textField);
                        GUILayout.Space(8);
                        _data.x.offset.max = EditorGUILayout.FloatField("max:", _data.x.offset.max, EditorStyles.numberField);

                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.EndToggleGroup();
            }
            GUILayout.EndVertical(); //END X
            GUILayout.Space(8);
            GUILayout.BeginVertical(EditorStyles.helpBox); //Y
            {
                _data.y.randomizeAxis = EditorGUILayout.BeginToggleGroup("Randomize Y", _data.y.randomizeAxis);
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        _data.y.offset.min = EditorGUILayout.FloatField("min:", _data.y.offset.min, EditorStyles.textField);
                        GUILayout.Space(8);
                        _data.y.offset.max = EditorGUILayout.FloatField("max:", _data.y.offset.max, EditorStyles.textField);
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.EndToggleGroup();
            }
            GUILayout.EndVertical(); //END Y
            GUILayout.Space(8);

            GUILayout.BeginVertical(EditorStyles.helpBox); //Z
            {
                _data.z.randomizeAxis = EditorGUILayout.BeginToggleGroup("Randomize Z", _data.z.randomizeAxis);
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        _data.z.offset.min = EditorGUILayout.FloatField("min", _data.z.offset.min, EditorStyles.textField);
                        GUILayout.Space(8);
                        _data.z.offset.max = EditorGUILayout.FloatField("max", _data.z.offset.max, EditorStyles.textField);
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.EndToggleGroup();
            }
            GUILayout.EndVertical(); //END Z
        }

        protected abstract void Randomize();
    }

    public class RandomizePositionsWindow : RandomizeToolWindow
    {
        [MenuItem("Tools/Plugin Master/Transform Tools/Randomize Positions", false, 1400)]
        public static void ShowWindow()
        {
            GetWindow<RandomizePositionsWindow>();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            _attribute = RandomizeToolWindow.Attribute.POSITION;
            _data.z.offset.min = _data.y.offset.min = _data.x.offset.min = -1f;
            _data.z.offset.max = _data.y.offset.max = _data.x.offset.max = 1f;
        }

        protected override void Randomize()
        {
            TransformTools.RandomizePositions(_selectionOrderedTopLevel.ToArray(), _data);
        }
    }

    public class RandomizeRotationsWindow : RandomizeToolWindow
    {
        [MenuItem("Tools/Plugin Master/Transform Tools/Randomize Rotations", false, 1400)]
        public static void ShowWindow()
        {
            GetWindow<RandomizeRotationsWindow>();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            _attribute = RandomizeToolWindow.Attribute.ROTATION;
            _data.z.offset.min = _data.y.offset.min = _data.x.offset.min = -180f;
            _data.z.offset.max = _data.y.offset.max = _data.x.offset.max = 180f;
        }

        protected override void Randomize()
        {
            TransformTools.RandomizeRotations(_selectionOrderedTopLevel.ToArray(), _data);
        }
    }

    public class RandomizeScalesWindow : RandomizeToolWindow
    {
        private bool _separateAxes = false;

        [MenuItem("Tools/Plugin Master/Transform Tools/Randomize Scales", false, 1400)]
        public static void ShowWindow()
        {
            GetWindow<RandomizeScalesWindow>();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            _attribute = RandomizeToolWindow.Attribute.SCALE;
            _data.z.offset.min = _data.y.offset.min = _data.x.offset.min = -0.1f;
            _data.z.offset.max = _data.y.offset.max = _data.x.offset.max = 0.1f;
        }

        protected override void OnGUIValue()
        {
            EditorGUIUtility.labelWidth = 90;
            _separateAxes = EditorGUILayout.Toggle("Separate Axes", _separateAxes);
            EditorGUIUtility.labelWidth = 30;
            
            if (_separateAxes)
            {
                minSize = new Vector2(240, 200);
                base.OnGUIValue();
            }
            else
            {
                minSize = new Vector2(240, 80);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        _data.x.offset.min = _data.y.offset.min = _data.z.offset.min = EditorGUILayout.FloatField("min:", _data.x.offset.min, EditorStyles.textField);
                        GUILayout.Space(8);
                        _data.x.offset.max = _data.y.offset.max = _data.z.offset.max = EditorGUILayout.FloatField("max:", _data.x.offset.max, EditorStyles.numberField);

                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
        }

        protected override void Randomize()
        {
            TransformTools.RandomizeScales(_selectionOrderedTopLevel.ToArray(), _data, _separateAxes);
        }
    }
}
