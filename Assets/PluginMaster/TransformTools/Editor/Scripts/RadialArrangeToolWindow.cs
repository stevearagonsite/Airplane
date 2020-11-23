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
using System.Collections.Generic;

namespace PluginMaster
{
    public class RadialArrangeToolWindow : BaseToolWindow
    {
        private TransformTools.RadialArrangeData _data = new TransformTools.RadialArrangeData();
        private static readonly Vector3[] _axes =
        {
            Vector3.right, Vector3.left,
            Vector3.up, Vector3.down,
            Vector3.forward, Vector3.back
        };
        private static readonly string[] _axesOptions = { "+X", "-X", "+Y", "-Y", "+Z", "-Z" };

        private int _axisIdx = 4;
        private int _orientDirIdx = 0;
        private List<Vector3> _parallelAxes = null;
        private int _parallelDirIdx = 0;
        private List<string> _parallelAxesOptions = null;

        [MenuItem("Tools/Plugin Master/Transform Tools/Radial Arrangement", false, 1201)]
        public static void ShowWindow()
        {
            GetWindow<RadialArrangeToolWindow>();
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            titleContent = new GUIContent("Radial Arrangement", null, "Radial Arrangement");
            minSize = new Vector2(250, 305);
            GUILayout.BeginVertical();
            {
                EditorGUIUtility.labelWidth = 74;
                EditorGUIUtility.fieldWidth = 110;

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    _data.arrangeBy = (TransformTools.ArrangeBy)EditorGUILayout.Popup("Arrange by:", (int)_data.arrangeBy, new string[] { "Selection order", "Hierarchy order" });
                }
                GUILayout.EndHorizontal();

                EditorGUIUtility.labelWidth = 90;
                EditorGUIUtility.fieldWidth = 140;
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.BeginHorizontal();
                    {
                        _data.rotateAround = (TransformTools.RotateAround)EditorGUILayout.Popup("Rotate around:", (int)_data.rotateAround, new string[] { "Selection Center", "Transform position", "Object bounds center", "Custom position" });
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();

                    var disableCenterField = _data.rotateAround != TransformTools.RotateAround.CUSTOM_POSITION;
                    if (_data.rotateAround == TransformTools.RotateAround.TRANSFORM_POSITION
                        || _data.rotateAround == TransformTools.RotateAround.OBJECT_BOUNDS_CENTER)
                    {
                        minSize += new Vector2(0, 20);
                        GUILayout.BeginHorizontal();
                        {
                            _data.centerTransform = (Transform)EditorGUILayout.ObjectField("Transform:", _data.centerTransform, typeof(Transform), true);
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();
                    }
                    else if (_data.rotateAround == TransformTools.RotateAround.SELECTION_CENTER)
                    {
                        _data.UpdateCenter(_selectionOrderedTopLevel);
                    }
                    EditorGUI.BeginDisabledGroup(disableCenterField);
                    {
                        _data.center = EditorGUILayout.Vector3Field("Center", _data.center);
                    }
                    EditorGUI.EndDisabledGroup();
                    _axisIdx = EditorGUILayout.Popup("Rotation axis:", _axisIdx, _axesOptions, GUILayout.Width(235));
                    _data.axis = _axes[_axisIdx];
                    
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Ovewrite:", GUILayout.Width(87));
                        EditorGUIUtility.labelWidth = 10;
                        _data.overwriteX = EditorGUILayout.Toggle("X", _data.overwriteX);
                        _data.overwriteY = EditorGUILayout.Toggle("Y", _data.overwriteY);
                        _data.overwriteZ = EditorGUILayout.Toggle("Z", _data.overwriteZ);
                        GUILayout.FlexibleSpace();
                        EditorGUIUtility.labelWidth = 90;
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.BeginHorizontal();
                    {
                        _data.shape = (TransformTools.Shape)EditorGUILayout.Popup("Shape:", (int)_data.shape, new string[] { "Circle", "Circular Spiral", "Ellipse", "Elliptical Spiral" });
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                    switch (_data.shape)
                    {
                        case TransformTools.Shape.CIRCLE:
                            _data.endEllipseAxes = _data.startEllipseAxes = Vector2.one * EditorGUILayout.FloatField("Radius:", _data.startEllipseAxes.x, GUILayout.Width(235));
                            break;
                        case TransformTools.Shape.CIRCULAR_SPIRAL:
                            minSize += new Vector2(0, 20);
                            _data.startEllipseAxes = Vector2.one * EditorGUILayout.FloatField("Start Radius:", _data.startEllipseAxes.x, GUILayout.Width(235));
                            _data.endEllipseAxes = Vector2.one * EditorGUILayout.FloatField("End Radius:", _data.endEllipseAxes.x, GUILayout.Width(235));
                            break;
                        case TransformTools.Shape.ELLIPSE:
                            minSize += new Vector2(0, 20);
                            _data.endEllipseAxes = _data.startEllipseAxes = EditorGUILayout.Vector2Field("Ellipse axes:", _data.startEllipseAxes, GUILayout.Width(235));
                            break;
                        case TransformTools.Shape.ELLIPTICAL_SPIRAL:
                            _data.startEllipseAxes = EditorGUILayout.Vector2Field("Start ellipse axes:", _data.startEllipseAxes, GUILayout.Width(235));
                            _data.endEllipseAxes = EditorGUILayout.Vector2Field("End ellipse axes:", _data.endEllipseAxes, GUILayout.Width(235));
                            minSize += new Vector2(0, 60);
                            break;
                    }
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    _data.startAngle = EditorGUILayout.FloatField("Start angle:", _data.startAngle, GUILayout.Width(235));
                    _data.maxArcAngle = EditorGUILayout.FloatField("Max arc angle:", _data.maxArcAngle, GUILayout.Width(235));
                    EditorGUIUtility.labelWidth = 170;
                    _data.LastSpotEmpty = EditorGUILayout.ToggleLeft("Add an empty spot at the end", _data.LastSpotEmpty);
                    EditorGUIUtility.labelWidth = 90;
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    _data.orientToRadius = EditorGUILayout.BeginToggleGroup("Orient to the center", _data.orientToRadius);
                    {
                        EditorGUI.BeginChangeCheck();
                        _orientDirIdx = EditorGUILayout.Popup("Radial axis:", _orientDirIdx, _axesOptions, GUILayout.Width(235));
                        _data.orientDirection = _axes[_orientDirIdx];
                        if (EditorGUI.EndChangeCheck() || _parallelAxes == null)
                        {
                            _parallelAxes = new List<Vector3>(_axes);
                            _parallelAxesOptions = new List<string>(_axesOptions);
                            if (_orientDirIdx < 2)
                            {
                                _parallelAxes.RemoveAt(0);
                                _parallelAxes.RemoveAt(0);
                                _parallelAxesOptions.RemoveAt(0);
                                _parallelAxesOptions.RemoveAt(0);
                            }
                            else if (_orientDirIdx < 4)
                            {
                                _parallelAxes.RemoveAt(2);
                                _parallelAxes.RemoveAt(2);
                                _parallelAxesOptions.RemoveAt(2);
                                _parallelAxesOptions.RemoveAt(2);
                            }
                            else
                            {
                                _parallelAxes.RemoveAt(4);
                                _parallelAxes.RemoveAt(4);
                                _parallelAxesOptions.RemoveAt(4);
                                _parallelAxesOptions.RemoveAt(4);
                            }
                            _parallelDirIdx = 0;
                        }
                        _parallelDirIdx = EditorGUILayout.Popup("Parallel axis:", _parallelDirIdx, _parallelAxesOptions.ToArray(), GUILayout.Width(235));
                        _data.parallelDirection = _parallelAxes[_parallelDirIdx];
                    }
                    EditorGUILayout.EndToggleGroup();
                }
                GUILayout.EndVertical();

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
                        TransformTools.RadialArrange(_selectionOrderedTopLevel, _data);
                    }
                    EditorGUI.EndDisabledGroup();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        protected override void OnSelectionChange()
        {
            base.OnSelectionChange();
            _data.UpdateCenter(_selectionOrderedTopLevel);
        }

        private void Update()
        {
            _data.UpdateCenter();
        }
    }
}
