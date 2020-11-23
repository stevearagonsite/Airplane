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
    public class TransformToolsWindow : BaseToolWindow
    {
        #region MAIN
        private bool _alignGroupOpen = true;
        private bool _distributeGroupOpen = true;
        private bool _arrangeGroupOpen = true;
        private bool _randomizeGroupOpen = true;
        private bool _progressiveGroupOpen = true;
        private bool _editPivotGroupOpen = true;
        private bool _miscellaneousGroupOpen = true;

        private Vector2 _scrollPosition = Vector2.zero;
        private GameObject _pivot = null;

        private GUIStyle _buttonStyle = null;

        [MenuItem("Tools/Plugin Master/Transform Tools/Transform Tools", false, 1001)]
        public static void ShowWindow()
        {
            GetWindow<TransformToolsWindow>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _buttonStyle = _skin.GetStyle("AlignButton");
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            minSize = new Vector2(250, 240);

            titleContent = new GUIContent("Transform Tools", null, "Transform Tools");
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none);
            {
#if UNITY_2019_1_OR_NEWER
                _alignGroupOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_alignGroupOpen, "Align");
#else
                _alignGroupOpen = EditorGUILayout.Foldout(_alignGroupOpen, "Align");
#endif
                if (_alignGroupOpen)
                {
                    OnGuiAlignGroup();
                }
#if UNITY_2019_1_OR_NEWER
                EditorGUILayout.EndFoldoutHeaderGroup();
#endif
#if UNITY_2019_1_OR_NEWER
                _distributeGroupOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_distributeGroupOpen, "Distribute");
#else
                _distributeGroupOpen = EditorGUILayout.Foldout(_distributeGroupOpen, "Distribute");
#endif
                if (_distributeGroupOpen)
                {
                    OnGuiDistributeGroup();
                }
#if UNITY_2019_1_OR_NEWER
                EditorGUILayout.EndFoldoutHeaderGroup();
#endif

#if UNITY_2019_1_OR_NEWER
                _arrangeGroupOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_arrangeGroupOpen, "Arrange");
#else
                _arrangeGroupOpen = EditorGUILayout.Foldout(_arrangeGroupOpen, "Arrange");
#endif
                if (_arrangeGroupOpen)
                {
                    OnGuiArrangeGroup();
                }
#if UNITY_2019_1_OR_NEWER
                EditorGUILayout.EndFoldoutHeaderGroup();
#endif
#if UNITY_2019_1_OR_NEWER
                _progressiveGroupOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_progressiveGroupOpen, "Incremental Transform");
#else
                _progressiveGroupOpen = EditorGUILayout.Foldout(_progressiveGroupOpen, "Incremental Transform");
#endif
                if (_progressiveGroupOpen)
                {
                    OnGuiProgressiveGroup();
                }
#if UNITY_2019_1_OR_NEWER
                EditorGUILayout.EndFoldoutHeaderGroup();
#endif
#if UNITY_2019_1_OR_NEWER
                _randomizeGroupOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_randomizeGroupOpen, "Randomize");
#else
                _randomizeGroupOpen = EditorGUILayout.Foldout(_randomizeGroupOpen, "Randomize");
#endif
                if (_randomizeGroupOpen)
                {
                    OnGuiRandomizeGroup();
                }
#if UNITY_2019_1_OR_NEWER
                EditorGUILayout.EndFoldoutHeaderGroup();
#endif
#if UNITY_2019_1_OR_NEWER
                _editPivotGroupOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_editPivotGroupOpen, "Edit Pivot");
#else
                _editPivotGroupOpen = EditorGUILayout.Foldout(_editPivotGroupOpen, "Edit Pivot");
#endif
                if (_editPivotGroupOpen)
                {
                    OnGuiEditPivotGroup();
                }
#if UNITY_2019_1_OR_NEWER
                EditorGUILayout.EndFoldoutHeaderGroup();
#endif
#if UNITY_2019_1_OR_NEWER
                _miscellaneousGroupOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_miscellaneousGroupOpen, "Miscellaneous");
#else
                _miscellaneousGroupOpen = EditorGUILayout.Foldout(_miscellaneousGroupOpen, "Miscellaneous");
#endif
                if (_miscellaneousGroupOpen)
                {
                    OnGuiPlaceOnSurfaceGroup();
                }
#if UNITY_2019_1_OR_NEWER
                EditorGUILayout.EndFoldoutHeaderGroup();
#endif
            }
            EditorGUILayout.EndScrollView();
        }

        private void OnDestroy()
        {
            if (_pivot == null) return;
            Selection.activeGameObject = _pivot.transform.parent.gameObject;
            DestroyImmediate(_pivot);
        }

        protected override void OnSelectionChange()
        {
            base.OnSelectionChange();
            if (_pivot == null) return;
            if (Selection.activeObject != _pivot)
            {
                CancelEditPivot(false);
            }
        }
        #endregion

        #region ALIGN
        private readonly string[] _relativeToPopupOptions = new string[]
        {
            "Last Selected",
            "First Selected",
            "Biggest Object",
            "Smallest Object",
            "Selection"
        };

        private readonly string[] _alingObjPropOptions = new string[]
        {
            "Bounding Box",
            "Center",
            "Pivot",
        };

        private TransformTools.RelativeTo _relativeTo = TransformTools.RelativeTo.LAST_SELECTED;
        private bool _filteredByTopLevel = true;
        private TransformTools.ObjectProperty _alignObjectProperty = TransformTools.ObjectProperty.BOUNDING_BOX;
        private void OnGuiAlignGroup()
        {
            using (new GUILayout.VerticalScope(_skin.box))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Relative to:", _skin.label, GUILayout.Width(88), GUILayout.Height(18));
                    _relativeTo = (TransformTools.RelativeTo)EditorGUILayout.Popup((int)_relativeTo, _relativeToPopupOptions, GUILayout.Width(143));
                    GUILayout.FlexibleSpace();
                }
                GUILayout.Space(4);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Align property:", _skin.label, GUILayout.Width(88), GUILayout.Height(18));
                    _alignObjectProperty = (TransformTools.ObjectProperty)EditorGUILayout.Popup((int)_alignObjectProperty, _alingObjPropOptions, GUILayout.Width(143));
                    GUILayout.FlexibleSpace();
                }
                GUILayout.Space(4);
                using (new GUILayout.HorizontalScope())
                {
                    _filteredByTopLevel = EditorGUILayout.Toggle(_filteredByTopLevel, GUILayout.Width(14));
                    GUILayout.Label("Filter by topmost transform", _skin.label, GUILayout.Height(18));
                    GUILayout.FlexibleSpace();
                }
                GUILayout.Space(8);
                ///// X
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/AlignRightToAnchorLeft"), "Align right edges of objects to the left edge of the anchor"), _skin.GetStyle("AlignButton")))
                    {
                        TransformTools.Align(GetSelection(_filteredByTopLevel), _relativeTo, TransformTools.Axis.X, TransformTools.Bound.MAX, true, _filteredByTopLevel, _alignObjectProperty);
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/AlignLeft"), "Align left edges"), _skin.GetStyle("AlignButton")))
                    {
                        TransformTools.Align(GetSelection(_filteredByTopLevel), _relativeTo, TransformTools.Axis.X, TransformTools.Bound.MIN, false, _filteredByTopLevel, _alignObjectProperty);
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/AlignCenterX"), "Center on X axis"), _skin.GetStyle("AlignButton")))
                    {
                        TransformTools.Align(GetSelection(_filteredByTopLevel), _relativeTo, TransformTools.Axis.X, TransformTools.Bound.CENTER, false, _filteredByTopLevel, _alignObjectProperty);
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/AlignRight"), "Align right edges"), _skin.GetStyle("AlignButton")))
                    {
                        TransformTools.Align(GetSelection(_filteredByTopLevel), _relativeTo, TransformTools.Axis.X, TransformTools.Bound.MAX, false, _filteredByTopLevel, _alignObjectProperty);
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/AlignLeftToAnchorRight"), "Align left edges of objects to the right edge of the anchor"), _skin.GetStyle("AlignButton")))
                    {
                        TransformTools.Align(GetSelection(_filteredByTopLevel), _relativeTo, TransformTools.Axis.X, TransformTools.Bound.MIN, true, _filteredByTopLevel, _alignObjectProperty);
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.Space(4);
                ///// Y
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/AlignTopToAnchorBottom"), "Align top edges of objects to the bottom edge of the anchor"), _skin.GetStyle("AlignButton")))
                    {
                        TransformTools.Align(GetSelection(_filteredByTopLevel), _relativeTo, TransformTools.Axis.Y, TransformTools.Bound.MAX, true, _filteredByTopLevel, _alignObjectProperty);
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/AlignBottom"), "Align bottom edges"), _skin.GetStyle("AlignButton")))
                    {
                        TransformTools.Align(GetSelection(_filteredByTopLevel), _relativeTo, TransformTools.Axis.Y, TransformTools.Bound.MIN, false, _filteredByTopLevel, _alignObjectProperty);
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/AlignCenterY"), "Center on Y axis"), _skin.GetStyle("AlignButton")))
                    {
                        TransformTools.Align(GetSelection(_filteredByTopLevel), _relativeTo, TransformTools.Axis.Y, TransformTools.Bound.CENTER, false, _filteredByTopLevel, _alignObjectProperty);
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/AlignTop"), "Align top edges"), _skin.GetStyle("AlignButton")))
                    {
                        TransformTools.Align(GetSelection(_filteredByTopLevel), _relativeTo, TransformTools.Axis.Y, TransformTools.Bound.MAX, false, _filteredByTopLevel, _alignObjectProperty);
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/AlignBottomToAnchorTop"), "Align bottom edges of objects to the top edge of the anchor"), _skin.GetStyle("AlignButton")))
                    {
                        TransformTools.Align(GetSelection(_filteredByTopLevel), _relativeTo, TransformTools.Axis.Y, TransformTools.Bound.MIN, true, _filteredByTopLevel, _alignObjectProperty);
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.Space(4);
                ///// Z
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/AlignFrontToAnchorBack"), "Align front edges of objects to the back edge of the anchor"), _skin.GetStyle("AlignButton")))
                    {
                        TransformTools.Align(GetSelection(_filteredByTopLevel), _relativeTo, TransformTools.Axis.Z, TransformTools.Bound.MAX, true, _filteredByTopLevel, _alignObjectProperty);
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/AlignBack"), "Align back edges"), _skin.GetStyle("AlignButton")))
                    {
                        TransformTools.Align(GetSelection(_filteredByTopLevel), _relativeTo, TransformTools.Axis.Z, TransformTools.Bound.MIN, false, _filteredByTopLevel, _alignObjectProperty);
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/AlignCenterZ"), "Center on Z axis"), _skin.GetStyle("AlignButton")))
                    {
                        TransformTools.Align(GetSelection(_filteredByTopLevel), _relativeTo, TransformTools.Axis.Z, TransformTools.Bound.CENTER, false, _filteredByTopLevel, _alignObjectProperty);
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/AlignFront"), "Align front edges"), _skin.GetStyle("AlignButton")))
                    {
                        TransformTools.Align(GetSelection(_filteredByTopLevel), _relativeTo, TransformTools.Axis.Z, TransformTools.Bound.MAX, false, _filteredByTopLevel, _alignObjectProperty);
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/AlignBackToAnchorFront"), "Align back edges of objects to the front edge of the anchor"), _skin.GetStyle("AlignButton")))
                    {
                        TransformTools.Align(GetSelection(_filteredByTopLevel), _relativeTo, TransformTools.Axis.Z, TransformTools.Bound.MIN, true, _filteredByTopLevel, _alignObjectProperty);
                    }
                    GUILayout.FlexibleSpace();
                }
            }
        }
        #endregion

        #region DISTRIBUTE
        private void OnGuiDistributeGroup()
        {
            GUILayout.BeginVertical(_skin.box);
            {
                ///// X
                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    {
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/DistributeLeft"), "Distribute left edges equidistantly"), _buttonStyle))
                            {
                                TransformTools.Distribute(_selectionOrderedTopLevel, TransformTools.Axis.X, TransformTools.Bound.MIN);
                            }
                            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/DistributeCenterX"), "Distribute centers equidistantly on the X axis"), _buttonStyle))
                            {
                                TransformTools.Distribute(_selectionOrderedTopLevel, TransformTools.Axis.X, TransformTools.Bound.CENTER);
                            }
                            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/DistributeRight"), "Distribute right edges equidistantly"), _buttonStyle))
                            {
                                TransformTools.Distribute(_selectionOrderedTopLevel, TransformTools.Axis.X, TransformTools.Bound.MAX);
                            }
                            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/DistributeGapX"), "Make equal gaps between objects on the X axis"), _buttonStyle))
                            {
                                TransformTools.DistributeGaps(_selectionOrderedTopLevel, TransformTools.Axis.X);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(4);
                        ///// Y
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/DistributeBottom"), "Distribute bottom edges equidistantly"), _buttonStyle))
                            {
                                TransformTools.Distribute(_selectionOrderedTopLevel, TransformTools.Axis.Y, TransformTools.Bound.MIN);
                            }
                            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/DistributeCenterY"), "Distribute centers equidistantly on the Y axis"), _buttonStyle))
                            {
                                TransformTools.Distribute(_selectionOrderedTopLevel, TransformTools.Axis.Y, TransformTools.Bound.CENTER);
                            }
                            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/DistributeTop"), "Distribute top edges equidistantly"), _buttonStyle))
                            {
                                TransformTools.Distribute(_selectionOrderedTopLevel, TransformTools.Axis.Y, TransformTools.Bound.MAX);
                            }
                            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/DistributeGapY"), "Make equal gaps between objects on the Y axis"), _buttonStyle))
                            {
                                TransformTools.DistributeGaps(_selectionOrderedTopLevel, TransformTools.Axis.Y);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(4);
                        ///// Y
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/DistributeBack"), "Distribute back edges equidistantly"), _buttonStyle))
                            {
                                TransformTools.Distribute(_selectionOrderedTopLevel, TransformTools.Axis.Z, TransformTools.Bound.MIN);
                            }
                            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/DistributeCenterZ"), "Distribute centers equidistantly on the Z axis"), _buttonStyle))
                            {
                                TransformTools.Distribute(_selectionOrderedTopLevel, TransformTools.Axis.Z, TransformTools.Bound.CENTER);
                            }
                            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/DistributeFront"), "Distribute front edges equidistantly"), _buttonStyle))
                            {
                                TransformTools.Distribute(_selectionOrderedTopLevel, TransformTools.Axis.Z, TransformTools.Bound.MAX);
                            }
                            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/DistributeGapZ"), "Make equal gaps between objects on the Z axis"), _buttonStyle))
                            {
                                TransformTools.DistributeGaps(_selectionOrderedTopLevel, TransformTools.Axis.Z);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();
        }
        #endregion

        #region ARRANGE
        private void OnGuiArrangeGroup()
        {
            GUILayout.BeginVertical(_skin.box);
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/GridArrange"), "Grid Arrangement"), _buttonStyle))
                    {
                        var arrangeWindow = (GridArrangementToolWindow)GetWindow<GridArrangementToolWindow>();
                        arrangeWindow.Show();
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/RadialArrange"), "Radial Arrangement"), _buttonStyle))
                    {
                        var arrangeWindow = (RadialArrangeToolWindow)GetWindow<RadialArrangeToolWindow>();
                        arrangeWindow.Show();
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/RearrangeSelectionOrder"), "Exchange positions - Selection Order"), _buttonStyle))
                    {
                        TransformTools.Rearrange(_selectionOrderedTopLevel, TransformTools.ArrangeBy.SELECTION_ORDER);
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/RearrangeHierarchyOrder"), "Exchange positions - Hierarchy Order"), _buttonStyle))
                    {
                        TransformTools.Rearrange(_selectionOrderedTopLevel, TransformTools.ArrangeBy.HIERARCHY_ORDER);
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        #endregion

        #region INCREMENTAL TRANSFORM
        private void OnGuiProgressiveGroup()
        {
            GUILayout.BeginVertical(_skin.box);
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/IncrementalPosition"), "Place objects incrementally"), _buttonStyle))
                    {
                        var progressivePositionWindow = (IncrementalPositionWindow)GetWindow<IncrementalPositionWindow>();
                        progressivePositionWindow.Show();
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/IncrementalRotation"), "Rotate objects incrementally"), _buttonStyle))
                    {
                        var progressiveRotationWindow = (IncrementalRotationWindow)GetWindow<IncrementalRotationWindow>();
                        progressiveRotationWindow.Show();
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/IncrementalScale"), "Scale objects incrementally"), _buttonStyle))
                    {
                        var progressiveScaleWindow = (IncrementalScaleWindow)GetWindow<IncrementalScaleWindow>();
                        progressiveScaleWindow.Show();
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        #endregion

        #region RANDOMIZE
        private void OnGuiRandomizeGroup()
        {
            GUILayout.BeginVertical(_skin.box);
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/RandomizePosition"), "Randomize Positions"), _buttonStyle))
                    {
                        var randomizeWindow = (RandomizePositionsWindow)GetWindow<RandomizePositionsWindow>();
                        randomizeWindow.Show();
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/RandomizeRotation"), "Randomize Rotations"), _buttonStyle))
                    {
                        var randomizeWindow = (RandomizeRotationsWindow)GetWindow<RandomizeRotationsWindow>();
                        randomizeWindow.Show();
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/RandomizeScale"), "Randomize Scales"), _buttonStyle))
                    {
                        var randomizeWindow = (RandomizeScalesWindow)GetWindow<RandomizeScalesWindow>();
                        randomizeWindow.Show();
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        #endregion

        #region EDIT PIVOT
        private void OnGuiEditPivotGroup()
        {
            GUILayout.BeginVertical(_skin.box);
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/CenterPivot"), "Center pivot"), _buttonStyle))
                    {
                        var meshFilter = Selection.activeGameObject.GetComponent<MeshFilter>();
                        if (meshFilter == null)
                        {
                            TransformTools.CenterPivot(Selection.activeGameObject.transform);
                        }
                        else if (MeshChangeWarning(Selection.activeGameObject))
                        {
                            if (meshFilter.sharedMesh != null)
                            {
                                string savePath = EditorUtility.SaveFilePanelInProject("Save As", meshFilter.sharedMesh.name, "asset", string.Empty);
                                if (!string.IsNullOrEmpty(savePath))
                                {
                                    TransformTools.CenterPivot(Selection.activeGameObject.transform, savePath);
                                }
                            }
                        }
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/EditPivot"), "Edit pivot position and rotation"), _buttonStyle))
                    {
                        _pivot = TransformTools.StartEditingPivot(Selection.activeGameObject);
                    }
                    if (_pivot != null)
                    {
                        if (GUILayout.Button("Apply", GUILayout.Width(60), GUILayout.Height(42)))
                        {
                            var meshFilter = _pivot.transform.parent.GetComponent<MeshFilter>();
                            if (meshFilter != null && meshFilter.sharedMesh != null)
                            {
                                if (MeshChangeWarning(_pivot.transform.parent.gameObject))
                                {
                                    string savePath = EditorUtility.SaveFilePanelInProject("Save As", meshFilter.sharedMesh.name, "asset", string.Empty);
                                    if (!string.IsNullOrEmpty(savePath))
                                    {
                                        TransformTools.SaveMesh(meshFilter, savePath, _pivot.transform);
                                    }
                                }
                            }
                            else
                            {
                                TransformTools.ApplyPivot(_pivot.transform);
                            }
                            CancelEditPivot(true);
                        }
                        if (GUILayout.Button("Cancel", GUILayout.Width(60), GUILayout.Height(42)))
                        {
                            CancelEditPivot(true);
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private bool MeshChangeWarning(GameObject target)
        {
            if (target == null) return false;
            var meshFilter = target.GetComponent<MeshFilter>();

            return EditorUtility.DisplayDialog(
                "Warning: Mesh will be modified",
                "Changing the pivot will modify the mesh.\n" +
                "Would you like to continue and save the mesh as new Asset?",
                "Continue", "Cancel");
        }

        private void CancelEditPivot(bool selectTarget)
        {
            if (_pivot == null) return;
            if (selectTarget)
            {
                Selection.activeObject = _pivot.transform.parent.gameObject;
            }
            DestroyImmediate(_pivot);
            _pivot = null;
            Repaint();
        }
        #endregion

        #region PLACE ON THE SURFACE
        private void OnGuiPlaceOnSurfaceGroup()
        {
            GUILayout.BeginVertical(_skin.box);
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/PlaceOnSurface"), "Place on the surface"), _buttonStyle))
                    {
                        var placeOnSurfaceWindow = (PlaceOnSurfaceWindow)GetWindow<PlaceOnSurfaceWindow>();
                        placeOnSurfaceWindow.Show();
                    }
                    if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("Sprites/Unoverlap"), "Move objects so that their bounding boxes don't overlap"), _buttonStyle))
                    {
                        var unoverlapWindow = (UnoverlapToolWindow)GetWindow<UnoverlapToolWindow>();
                        unoverlapWindow.Show();
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        #endregion
    }
}
