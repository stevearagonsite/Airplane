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
    public class GridArrangementToolWindow : BaseToolWindow
    {
        #region WINDOW
        private TransformTools.ArrangeData _data = new TransformTools.ArrangeData();
        private GUIStyle _buttonStyle = null;

        [MenuItem("Tools/Plugin Master/Transform Tools/Grid Arrangement", false, 1200)]
        public static void ShowWindow()
        {
            GetWindow<GridArrangementToolWindow>();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            _buttonStyle = _skin.GetStyle("AlignmentToggle");
        }

        protected override void OnGUI()
        {
            base.OnGUI();
#if UNITY_2019_1_OR_NEWER
            maxSize = minSize = new Vector2(536, 270);
#else
            maxSize = minSize = new Vector2(552, 270);
#endif
            titleContent = new GUIContent("Grid Arrangement", null, "Grid Arrangement");
            GUILayout.BeginVertical();
            {
                EditorGUIUtility.labelWidth = 130;
                EditorGUIUtility.fieldWidth = 110;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    _data.sortBy = (TransformTools.SortBy) EditorGUILayout.Popup("Arrange according to:", (int)_data.sortBy, new string[] { "Selection Order", "Current Position", "Hierarchy Order"});
                }
                GUILayout.EndHorizontal();
                
                EditorGUIUtility.labelWidth = 55;
                EditorGUIUtility.fieldWidth = 100;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(4);
                    GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(170)); //X
                    {
                        EditorGUI.BeginChangeCheck();
                        _data.x.overwrite = EditorGUILayout.BeginToggleGroup("X", _data.x.overwrite);
                        if (EditorGUI.EndChangeCheck())
                        {
                            _data.UpdatePriorities(TransformTools.Axis.X);
                        }
                        {
                            EditorGUI.BeginDisabledGroup(_data.sortBy == TransformTools.SortBy.POSITION);
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUI.BeginChangeCheck();
                                _data.x.priority = EditorGUILayout.Popup("Priority:", _data.x.priority, new string[] { "1", "2", "3" });
                                GUILayout.FlexibleSpace();
                                if (EditorGUI.EndChangeCheck())
                                {
                                    _data.UpdatePriorities(TransformTools.Axis.X);
                                }
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            {
                                _data.x.direction = EditorGUILayout.Popup("Direction:", _data.x.direction == 1 ? 0 : 1, new string[] { "+X", "-X" }) == 0 ? 1 : -1;
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();
                            EditorGUI.EndDisabledGroup();

                            GUILayout.BeginHorizontal();
                            {
                                _data.x.cells = Stepper("Columns:", _data.x.cells, 1, _selectionOrderedTopLevel.Count);
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Label("Width:", EditorStyles.label);
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUIUtility.fieldWidth = 100;
                                _data.x.cellSizeType = (TransformTools.CellSizeType)EditorGUILayout.Popup((int)_data.x.cellSizeType, new string[] { "Widest object per column", "Widest object selected", "Custom" });
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            {
                                EditorGUI.BeginDisabledGroup(_data.x.cellSizeType != TransformTools.CellSizeType.CUSTOM);
                                {
                                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                                    {
                                        EditorGUIUtility.labelWidth = 51;
                                        EditorGUIUtility.fieldWidth = 90;
                                        _data.x.cellSize = EditorGUILayout.FloatField("Value:", _data.x.cellSize, EditorStyles.numberField);
                                        GUILayout.FlexibleSpace();
                                    }
                                    GUILayout.EndHorizontal();
                                }
                                EditorGUI.EndDisabledGroup();
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();

                            EditorGUIUtility.labelWidth = 55;
                            EditorGUIUtility.fieldWidth = 100;
                            GUILayout.Label("Alignment:", EditorStyles.label);
                            GUILayout.BeginHorizontal();
                            {
                                if (GUILayout.Toggle(_data.x.aligment == TransformTools.Bound.MIN, Resources.Load<Texture2D>("Sprites/AlignmentLeft"), _buttonStyle))
                                {
                                    _data.x.aligment = TransformTools.Bound.MIN;
                                }
                                if (GUILayout.Toggle(_data.x.aligment == TransformTools.Bound.CENTER, Resources.Load<Texture2D>("Sprites/AlignmentCenterX"), _buttonStyle))
                                {
                                    _data.x.aligment = TransformTools.Bound.CENTER;
                                }
                                if (GUILayout.Toggle(_data.x.aligment == TransformTools.Bound.MAX, Resources.Load<Texture2D>("Sprites/AlignmentRight"), _buttonStyle))
                                {
                                    _data.x.aligment = TransformTools.Bound.MAX;
                                }
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            {
                                _data.x.spacing = EditorGUILayout.FloatField("Spacing:", _data.x.spacing, EditorStyles.numberField);
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.Space(4);
                        }
                        EditorGUILayout.EndToggleGroup();
                    }
                    GUILayout.EndVertical(); //END X
                    GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(170)); //Y
                    {
                        EditorGUI.BeginChangeCheck();
                        _data.y.overwrite = EditorGUILayout.BeginToggleGroup("Y", _data.y.overwrite);
                        if (EditorGUI.EndChangeCheck())
                        {
                            _data.UpdatePriorities(TransformTools.Axis.Y);
                        }
                        {
                            EditorGUI.BeginDisabledGroup(_data.sortBy == TransformTools.SortBy.POSITION);
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUI.BeginChangeCheck();
                                _data.y.priority = EditorGUILayout.Popup("Priority:", _data.y.priority, new string[] { "1", "2", "3" });
                                GUILayout.FlexibleSpace();
                                if (EditorGUI.EndChangeCheck())
                                {
                                    _data.UpdatePriorities(TransformTools.Axis.Y);
                                }
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            {
                                _data.y.direction = EditorGUILayout.Popup("Direction:", _data.y.direction == 1 ? 0 : 1, new string[] { "+Y", "-Y" }) == 0 ? 1 : -1;
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();
                            EditorGUI.EndDisabledGroup();

                            GUILayout.BeginHorizontal();
                            {
                                _data.y.cells = Stepper("Rows:", _data.y.cells, 1, _selectionOrderedTopLevel.Count);
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Label("Height:", EditorStyles.label);
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUIUtility.fieldWidth = 100;
                                _data.y.cellSizeType = (TransformTools.CellSizeType)EditorGUILayout.Popup((int)_data.y.cellSizeType, new string[] { "Tallest object per column", "Tallest object selected", "Custom" });
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            {
                                EditorGUI.BeginDisabledGroup(_data.y.cellSizeType != TransformTools.CellSizeType.CUSTOM);
                                {
                                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                                    {
                                        EditorGUIUtility.labelWidth = 51;
                                        EditorGUIUtility.fieldWidth = 90;
                                        _data.y.cellSize = EditorGUILayout.FloatField("Value:", _data.y.cellSize, EditorStyles.numberField);
                                        GUILayout.FlexibleSpace();
                                    }
                                    GUILayout.EndHorizontal();
                                }
                                EditorGUI.EndDisabledGroup();
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();

                            EditorGUIUtility.labelWidth = 55;
                            EditorGUIUtility.fieldWidth = 100;

                            GUILayout.Label("Alignment:", EditorStyles.label);
                            GUILayout.BeginHorizontal();
                            {
                                if (GUILayout.Toggle(_data.y.aligment == TransformTools.Bound.MIN, Resources.Load<Texture2D>("Sprites/AlignmentBottom"), _buttonStyle, GUILayout.Width(32)))
                                {
                                    _data.y.aligment = TransformTools.Bound.MIN;
                                }
                                if (GUILayout.Toggle(_data.y.aligment == TransformTools.Bound.CENTER, Resources.Load<Texture2D>("Sprites/AlignmentCenterY"), _buttonStyle, GUILayout.Width(32)))
                                {
                                    _data.y.aligment = TransformTools.Bound.CENTER;
                                }
                                if (GUILayout.Toggle(_data.y.aligment == TransformTools.Bound.MAX, Resources.Load<Texture2D>("Sprites/AlignmentTop"), _buttonStyle, GUILayout.Width(32)))
                                {
                                    _data.y.aligment = TransformTools.Bound.MAX;
                                }
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            {
                                _data.y.spacing = EditorGUILayout.FloatField("Spacing:", _data.y.spacing, EditorStyles.numberField);
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.Space(4);
                        }
                        EditorGUILayout.EndToggleGroup();
                    }
                    GUILayout.EndVertical(); //END Y
                    GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(170)); //Z
                    {
                        EditorGUI.BeginChangeCheck();
                        _data.z.overwrite = EditorGUILayout.BeginToggleGroup("Z", _data.z.overwrite);
                        if (EditorGUI.EndChangeCheck())
                        {
                            _data.UpdatePriorities(TransformTools.Axis.Z);
                        }
                        {
                            EditorGUI.BeginDisabledGroup(_data.sortBy == TransformTools.SortBy.POSITION);
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUI.BeginChangeCheck();
                                _data.z.priority = EditorGUILayout.Popup("Priority:", _data.z.priority, new string[] { "1", "2", "3" });
                                GUILayout.FlexibleSpace();
                                if (EditorGUI.EndChangeCheck())
                                {
                                    _data.UpdatePriorities(TransformTools.Axis.Z);
                                }
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            {
                                _data.z.direction = EditorGUILayout.Popup("Direction:", _data.z.direction == 1 ? 0 : 1, new string[] { "+Z", "-Z" }) == 0 ? 1 : -1;
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();
                            EditorGUI.EndDisabledGroup();

                            GUILayout.BeginHorizontal();
                            {
                                _data.z.cells = Stepper("Columns:", _data.z.cells, 1, _selectionOrderedTopLevel.Count);
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Label("Width:", EditorStyles.label);
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUIUtility.fieldWidth = 100;
                                _data.z.cellSizeType = (TransformTools.CellSizeType)EditorGUILayout.Popup((int)_data.z.cellSizeType, new string[] { "Widest object per column", "Widest object selected", "Custom" });
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            {
                                EditorGUI.BeginDisabledGroup(_data.z.cellSizeType != TransformTools.CellSizeType.CUSTOM);
                                {
                                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                                    {
                                        EditorGUIUtility.labelWidth = 51;
                                        EditorGUIUtility.fieldWidth = 90;
                                        _data.z.cellSize = EditorGUILayout.FloatField("Value:", _data.z.cellSize, EditorStyles.numberField);
                                        GUILayout.FlexibleSpace();
                                    }
                                    GUILayout.EndHorizontal();
                                }
                                EditorGUI.EndDisabledGroup();
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();

                            EditorGUIUtility.labelWidth = 55;
                            EditorGUIUtility.fieldWidth = 100;

                            GUILayout.Label("Alignment:", EditorStyles.label);
                            GUILayout.BeginHorizontal();
                            {
                                if (GUILayout.Toggle(_data.z.aligment == TransformTools.Bound.MIN, Resources.Load<Texture2D>("Sprites/AlignmentBack"), _buttonStyle, GUILayout.Width(32)))
                                {
                                    _data.z.aligment = TransformTools.Bound.MIN;
                                }
                                if (GUILayout.Toggle(_data.z.aligment == TransformTools.Bound.CENTER, Resources.Load<Texture2D>("Sprites/AlignmentCenterZ"), _buttonStyle, GUILayout.Width(32)))
                                {
                                    _data.z.aligment = TransformTools.Bound.CENTER;
                                }
                                if (GUILayout.Toggle(_data.z.aligment == TransformTools.Bound.MAX, Resources.Load<Texture2D>("Sprites/AlignmentFront"), _buttonStyle, GUILayout.Width(32)))
                                {
                                    _data.z.aligment = TransformTools.Bound.MAX;
                                }
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            {
                                _data.z.spacing = EditorGUILayout.FloatField("Spacing:", _data.z.spacing, EditorStyles.numberField);
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.Space(4);
                        }
                        EditorGUILayout.EndToggleGroup();
                    }
                    GUILayout.EndVertical(); //END Z
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(2);
                GUILayout.BeginHorizontal();
                {
                    var statusStyle = new GUIStyle(EditorStyles.label);
                    GUILayout.Space(8);
                    var statusMessage = "";
                    if (_selectionOrderedTopLevel.Count < 2)
                    {
                        statusMessage = "No objects selected.";
                    }
                    else if(_selectionOrderedTopLevel.Count > _data.x.cells * _data.y.cells * _data.z.cells)
                    {
                        statusMessage = _selectionOrderedTopLevel.Count + " objects selected. Selection don't fit. Add more rows or columns.";
                    }
                    else
                    {
                        statusMessage = _selectionOrderedTopLevel.Count + " objects selected.";
                    }
                    if (_selectionOrderedTopLevel.Count < 2 || _selectionOrderedTopLevel.Count > _data.x.cells * _data.y.cells * _data.z.cells)
                    {
                        GUILayout.Label(new GUIContent(Resources.Load<Texture2D>("Sprites/Warning")), new GUIStyle() { alignment = TextAnchor.LowerLeft });
                    }
                    GUILayout.Label(statusMessage, statusStyle);
                    GUILayout.FlexibleSpace();
                    EditorGUI.BeginDisabledGroup(_selectionOrderedTopLevel.Count > _data.x.cells * _data.y.cells * _data.z.cells || _selectionOrderedTopLevel.Count < 2);
                    if (GUILayout.Button("Arrange", EditorStyles.miniButtonRight))
                    {
                        TransformTools.Arrange(_selectionOrderedTopLevel, _data);
                    }
                    EditorGUI.EndDisabledGroup();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }        
#endregion //WINDOW

#region UTILS
        private int Stepper(string label, int value, int min, int max)
        {
            var retVal = value;
            GUILayout.BeginHorizontal();
            EditorGUIUtility.fieldWidth = 63;
            retVal = Mathf.Clamp(EditorGUILayout.IntField(label, retVal, EditorStyles.numberField, GUILayout.Height(18)), min, max);
            GUILayout.Space(-4);
            if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                --retVal;
                if (retVal < min)
                {
                    retVal = min;
                }

            }
            GUILayout.Space(-4);
            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                ++retVal;
                if (retVal > max)
                {
                    retVal = max;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return retVal;
        }
#endregion
    }
}
