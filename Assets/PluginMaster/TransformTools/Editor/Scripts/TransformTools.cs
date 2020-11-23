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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace PluginMaster
{
    public static class TransformTools
    {
        #region BOUNDS
        private static readonly Vector3 MIN_VECTOR3 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        private static readonly Vector3 MAX_VECTOR3 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

        public enum Bound { MIN, CENTER, MAX }

        public enum RelativeTo
        {
            LAST_SELECTED,
            FIRST_SELECTED,
            BIGGEST_OBJECT,
            SMALLEST_OBJECT,
            SELECTION
        }
        public enum Axis { X, Y, Z }

        public enum ObjectProperty
        {
            BOUNDING_BOX,
            CENTER,
            PIVOT
        }

        public static Bounds GetBounds(Transform transform, ObjectProperty property = ObjectProperty.BOUNDING_BOX)
        {
            var renderer = transform.GetComponent<Renderer>();
            var rectTransform = transform.GetComponent<RectTransform>();
            
            if(rectTransform == null)
            {
                if(renderer == null || property == ObjectProperty.PIVOT)
                {
                    return new Bounds(transform.position, Vector3.zero);
                }
                if(property == ObjectProperty.CENTER)
                {
                    return new Bounds(renderer.bounds.center, Vector3.zero);
                }
                return renderer.bounds;
            }
            else
            {
                return new Bounds(rectTransform.TransformPoint(rectTransform.rect.center), rectTransform.TransformVector(rectTransform.rect.size));
            }
        }

        public static Bounds GetBoundsRecursive(Transform transform, bool recursive = true, ObjectProperty property = ObjectProperty.BOUNDING_BOX)
        {
            if(!recursive)
            {
                return GetBounds(transform, property);
            }
            var children = transform.GetComponentsInChildren<Transform>(true);
            var min = MAX_VECTOR3;
            var max = MIN_VECTOR3;
            var emptyHierarchy = true;
            foreach (var child in children)
            {
                if (child.GetComponent<Renderer>() == null && child.GetComponent<RectTransform>() == null) continue;
                emptyHierarchy = false;
                var bounds = GetBounds(child, property);
                min = Vector3.Min(bounds.min, min);
                max = Vector3.Max(bounds.max, max);
            }
            if (emptyHierarchy)
            {
                return new Bounds(transform.position, Vector3.zero);
            }
            var size = max - min;
            var center = min + size / 2f;
            return new Bounds(center, size);
        }
        private static Vector3 GetBound(Bounds bounds, Bound bound)
        {
            switch (bound)
            {
                case Bound.MIN:
                    return bounds.min;
                case Bound.CENTER:
                    return bounds.center;
                case Bound.MAX:
                    return bounds.max;
            }
            return bounds.center;
        }

        private static GameObject GetAnchorObject(List<GameObject> selection, RelativeTo relativeTo, Axis axis, bool recursive = true)
        {
            if (selection.Count == 0) return null;
            switch (relativeTo)
            {
                case RelativeTo.LAST_SELECTED:
                    return selection.Last<GameObject>();
                case RelativeTo.FIRST_SELECTED:
                    return selection[0];
                case RelativeTo.BIGGEST_OBJECT:
                    GameObject biggestObject = null;
                    var maxSize = float.MinValue;
                    foreach (var obj in selection)
                    {

                        var bounds = GetBoundsRecursive(obj.transform, recursive);
                        switch (axis)
                        {
                            case Axis.X:
                                if (bounds.size.x > maxSize)
                                {
                                    maxSize = bounds.size.x;
                                    biggestObject = obj;
                                }
                                break;
                            case Axis.Y:
                                if (bounds.size.y > maxSize)
                                {
                                    maxSize = bounds.size.y;
                                    biggestObject = obj;
                                }
                                break;
                            case Axis.Z:
                                if (bounds.size.z > maxSize)
                                {
                                    maxSize = bounds.size.z;
                                    biggestObject = obj;
                                }
                                break;
                        }
                    }
                    return biggestObject;
                case RelativeTo.SMALLEST_OBJECT:
                    GameObject smallestObject = null;
                    var minSize = float.MaxValue;
                    foreach (var obj in selection)
                    {
                        var bounds = GetBoundsRecursive(obj.transform, recursive);
                        switch (axis)
                        {
                            case Axis.X:
                                if (bounds.size.x < minSize)
                                {
                                    minSize = bounds.size.x;
                                    smallestObject = obj;
                                }
                                break;
                            case Axis.Y:
                                if (bounds.size.y < minSize)
                                {
                                    minSize = bounds.size.y;
                                    smallestObject = obj;
                                }
                                break;
                            case Axis.Z:
                                if (bounds.size.z < minSize)
                                {
                                    minSize = bounds.size.z;
                                    smallestObject = obj;
                                }
                                break;
                        }
                    }
                    return smallestObject;
                default:
                    return null;
            }
        }

        private static Bounds GetSelectionBounds(List<GameObject> selection, bool recursive = true)
        {
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            foreach (var obj in selection)
            {
                var bounds = TransformTools.GetBoundsRecursive(obj.transform, recursive);
                max = Vector3.Max(bounds.max, max);
                min = Vector3.Min(bounds.min, min);
            }
            var size = max - min;
            var center = min + size / 2f;
            return new Bounds(center, size);
        }

        private static Tuple<GameObject, Bounds> GetSelectionBounds(List<GameObject> selection, RelativeTo relativeTo, Axis axis, bool recursive = true, ObjectProperty property = ObjectProperty.BOUNDING_BOX)
        {
            if (selection.Count == 0) return new Tuple<GameObject, Bounds>(null, new Bounds());
            var anchor = GetAnchorObject(selection, relativeTo, axis);
            if (anchor != null)
            {
                return new Tuple<GameObject, Bounds>(anchor, GetBoundsRecursive(anchor.transform, recursive));
            }
            return new Tuple<GameObject, Bounds>(null, GetSelectionBounds(selection));
        }
        #endregion
        #region ALIGN
        public static void Align(List<GameObject> selection, RelativeTo relativeTo, Axis axis, Bound bound, bool AlignToAnchor, bool filterByTopLevel = true, ObjectProperty property = ObjectProperty.BOUNDING_BOX)
        {
            if (selection.Count == 0) return;
            if (bound == Bound.CENTER && AlignToAnchor) return;

            var selectionBoundsTuple = GetSelectionBounds(selection, relativeTo, axis, filterByTopLevel);
            var selectionBound = GetBound(selectionBoundsTuple.Item2, AlignToAnchor ? (bound == Bound.MAX ? Bound.MIN : Bound.MAX) : bound);
            var anchor = selectionBoundsTuple.Item1;

            for (int i = 0; i < selection.Count; ++i)
            {
                var obj = selection[i];
                if (obj == anchor && relativeTo != RelativeTo.SELECTION) continue;

                UnityEditor.Undo.RecordObject(obj.transform, "Align");

                var objBound = GetBound( GetBoundsRecursive(obj.transform, filterByTopLevel, property), bound);
                var alignedPosition = obj.transform.position;
                
                switch (axis)
                {
                    case Axis.X:
                        alignedPosition.x = obj.transform.position.x + selectionBound.x - objBound.x;
                        break;
                    case Axis.Y:
                        alignedPosition.y = obj.transform.position.y + selectionBound.y - objBound.y;
                        break;
                    case Axis.Z:
                        alignedPosition.z = obj.transform.position.z + selectionBound.z - objBound.z;
                        break;
                }
                var delta = alignedPosition - obj.transform.position;
                obj.transform.position = alignedPosition;
                if(anchor != null && anchor.transform.parent == obj.transform)
                {
                    UnityEditor.Undo.RecordObject(anchor.transform, "Align");
                    anchor.transform.position -= delta;
                }
            }
        }
        #endregion
        #region DISTRIBUTE
        public static void Distribute(List<GameObject> selection, Axis axis, Bound bound)
        {
            if (selection.Count < 2) return;
            var sortedList = new List<GameObject>(selection);
            switch (axis)
            {
                case Axis.X:
                    sortedList.Sort((obj1, obj2) => GetBound(GetBoundsRecursive(obj1.transform), bound).x.CompareTo(GetBound(GetBoundsRecursive(obj2.transform), bound).x));
                    break;
                case Axis.Y:
                    sortedList.Sort((obj1, obj2) => GetBound(GetBoundsRecursive(obj1.transform), bound).y.CompareTo(GetBound(GetBoundsRecursive(obj2.transform), bound).y));
                    break;
                case Axis.Z:
                    sortedList.Sort((obj1, obj2) => GetBound(GetBoundsRecursive(obj1.transform), bound).z.CompareTo(GetBound(GetBoundsRecursive(obj2.transform), bound).z));
                    break;
            }

            var min = GetBound(GetBoundsRecursive(sortedList.First<GameObject>().transform), bound);
            var max = GetBound(GetBoundsRecursive(sortedList.Last<GameObject>().transform), bound);

            var objDistance = 0f;
            switch (axis)
            {
                case Axis.X:
                    objDistance = (max.x - min.x) / (float)(selection.Count - 1);
                    break;
                case Axis.Y:
                    objDistance = (max.y - min.y) / (float)(selection.Count - 1);
                    break;
                case Axis.Z:
                    objDistance = (max.z - min.z) / (float)(selection.Count - 1);
                    break;
            }

            int i = 0;
            foreach (var obj in sortedList)
            {
                Undo.RecordObject(obj.transform, "Distribute");

                var distributedPosition = obj.transform.position;
                var objBound = GetBound(GetBoundsRecursive(obj.transform), bound);

                switch (axis)
                {
                    case Axis.X:
                        distributedPosition.x = min.x + obj.transform.position.x - objBound.x + objDistance * i;
                        break;
                    case Axis.Y:
                        distributedPosition.y = min.y + obj.transform.position.y - objBound.y + objDistance * i;
                        break;
                    case Axis.Z:
                        distributedPosition.z = min.z + obj.transform.position.z - objBound.z + objDistance * i;
                        break;
                }
                obj.transform.position = distributedPosition;
                ++i;
            }
        }

        public static void DistributeGaps(List<GameObject> selection, Axis axis)
        {
            if (selection.Count < 2) return;

            var selectionBounds = GetSelectionBounds(selection, RelativeTo.SELECTION, axis).Item2;
            var gapSize = selectionBounds.size;
            foreach (var obj in selection)
            {
                gapSize -= GetBoundsRecursive(obj.transform).size;
            }
            gapSize /= (float)(selection.Count - 1);

            var sortedList = new List<GameObject>(selection);
            switch (axis)
            {
                case Axis.X:
                    sortedList.Sort((obj1, obj2) => GetBoundsRecursive(obj1.transform).center.x.CompareTo(GetBoundsRecursive(obj2.transform).center.x));
                    break;
                case Axis.Y:
                    sortedList.Sort((obj1, obj2) => GetBoundsRecursive(obj1.transform).center.y.CompareTo(GetBoundsRecursive(obj2.transform).center.y));
                    break;
                case Axis.Z:
                    sortedList.Sort((obj1, obj2) => GetBoundsRecursive(obj1.transform).center.z.CompareTo(GetBoundsRecursive(obj2.transform).center.z));
                    break;
            }

            var prevMax = GetBoundsRecursive(sortedList.First<GameObject>().transform).min - gapSize;

            foreach (var obj in sortedList)
            {
                Undo.RecordObject(obj.transform, "Distribute Gaps");
                var distributedPosition = obj.transform.position;
                var objBounds = GetBoundsRecursive(obj.transform);
                var objMin = objBounds.min;
                switch (axis)
                {
                    case Axis.X:
                        distributedPosition.x = obj.transform.position.x - objMin.x + prevMax.x + gapSize.x;
                        break;
                    case Axis.Y:
                        distributedPosition.y = obj.transform.position.y - objMin.y + prevMax.y + gapSize.y;
                        break;
                    case Axis.Z:
                        distributedPosition.z = obj.transform.position.z - objMin.z + prevMax.z + gapSize.z;
                        break;
                }
                obj.transform.position = distributedPosition;
                prevMax = GetBoundsRecursive(obj.transform).max;
            }
        }
        #endregion
        #region RECTANGULAR ARRANGE
        public class ArrangeAxisData
        {
            private bool _overwrite = true;
            private int _direction = 1;
            private int _priority = 0;
            private int _cells = 1;
            private CellSizeType _cellSizeType = CellSizeType.BIGGEST_OBJECT;
            private float _cellSize = 0f;
            private TransformTools.Bound _aligment = TransformTools.Bound.CENTER;
            private float _spacing = 0f;

            public ArrangeAxisData(int priority)
            {
                _priority = priority;
            }

            public int direction { get => _direction; set => _direction = value; }
            public int priority { get => _priority; set => _priority = value; }
            public int cells { get => _cells; set => _cells = value; }
            public Bound aligment { get => _aligment; set => _aligment = value; }
            public float spacing { get => _spacing; set => _spacing = value; }
            public CellSizeType cellSizeType { get => _cellSizeType; set => _cellSizeType = value; }
            public float cellSize
            {
                get => _cellSize;
                set
                {
                    if (value < 0 || _cellSize == value) return;
                    _cellSize = value;
                }
            }
            public bool overwrite
            {
                get => _overwrite;
                set
                {
                    if (_overwrite == value) return;
                    _overwrite = value;
                    if (!_overwrite)
                    {
                        _cells = 1;
                        _priority = 2;
                    }
                }
            }
        }

        public enum SortBy
        {
            SELECTION,
            POSITION,
            HIERARCHY
        }

        public enum CellSizeType
        {
            BIGGEST_OBJECT_PER_GROUP,
            BIGGEST_OBJECT,
            CUSTOM
        }

        public class ArrangeData
        {
            private ArrangeAxisData _x = new ArrangeAxisData(0);
            private ArrangeAxisData _y = new ArrangeAxisData(1);
            private ArrangeAxisData _z = new ArrangeAxisData(2);
            private SortBy _sortBy = SortBy.POSITION;
            private List<TransformTools.Axis> _priorityList = new List<TransformTools.Axis> { TransformTools.Axis.X, TransformTools.Axis.Y, TransformTools.Axis.Z };

            public ArrangeAxisData x { get => _x; set => _x = value; }
            public ArrangeAxisData y { get => _y; set => _y = value; }
            public ArrangeAxisData z { get => _z; set => _z = value; }
            public SortBy sortBy
            {
                get => _sortBy;
                set
                {
                    if (_sortBy == value) return;
                    _sortBy = value;
                    if (_sortBy == SortBy.POSITION)
                    {
                        x.priority = 0;
                        y.priority = 1;
                        z.priority = 2;
                        z.direction = y.direction = x.direction = +1;
                    }
                }
            }

            public ArrangeAxisData GetData(Axis axis)
            {
                return axis == Axis.X ? x : axis == Axis.Y ? y : z;
            }
            public void UpdatePriorities(Axis axis)
            {
                var activeAxes = Convert.ToInt32(x.overwrite) + Convert.ToInt32(y.overwrite) + Convert.ToInt32(z.overwrite);
                if (activeAxes > 0)
                {
                    if (x.overwrite)
                    {
                        x.priority = Mathf.Min(x.priority, activeAxes - 1);
                    }
                    if (y.overwrite)
                    {
                        y.priority = Mathf.Min(y.priority, activeAxes - 1);
                    }
                    if (z.overwrite)
                    {
                        z.priority = Mathf.Min(z.priority, activeAxes - 1);
                    }
                }
                _priorityList.Remove(axis);
                _priorityList.Insert(GetData(axis).priority, axis);


                for (int priority = 0; priority < 3; ++priority)
                {
                    switch (_priorityList[priority])
                    {
                        case TransformTools.Axis.X:
                            x.priority = priority;
                            break;
                        case TransformTools.Axis.Y:
                            y.priority = priority;
                            break;
                        case TransformTools.Axis.Z:
                            z.priority = priority;
                            break;
                    }
                }
            }

        }

        private static int GetNextCellIndex(int currentIndex, int direction, int cellCount)
        {
            return IsLastCell(currentIndex, direction, cellCount) ? (direction > 0 ? 0 : cellCount - 1) : currentIndex + direction;
        }

        private static bool IsFirstCell(int currentIndex, int direction, int cellCount)
        {
            return direction > 0 ? currentIndex == 0 : currentIndex == cellCount - 1;
        }

        private static bool IsLastCell(int currentIndex, int direction, int cellCount)
        {
            return IsFirstCell(currentIndex, -direction, cellCount);
        }

        private static Dictionary<(int i, int j, int k), GameObject> SortBySelectionOrder(List<GameObject> selection, ArrangeData data)
        {
            int i = data.x.direction == 1 ? 0 : data.x.cells - 1;
            int j = data.y.direction == 1 ? 0 : data.y.cells - 1;
            int k = data.z.direction == 1 ? 0 : data.z.cells - 1;

            var dataList = new List<ArrangeAxisData>() { data.x, data.y, data.z };
            dataList.Sort((data1, data2) => data1.priority.CompareTo(data2.priority));

            var p0 = dataList[0] == data.x ? i : dataList[0] == data.y ? j : k;
            var p1 = dataList[1] == data.x ? i : dataList[1] == data.y ? j : k;
            var p2 = dataList[2] == data.x ? i : dataList[2] == data.y ? j : k;

            var objDictionary = new Dictionary<(int i, int j, int k), GameObject>();

            foreach (var obj in selection)
            {
                objDictionary.Add((
                    dataList[0] == data.x ? p0 : dataList[1] == data.x ? p1 : p2,
                    dataList[0] == data.y ? p0 : dataList[1] == data.y ? p1 : p2,
                    dataList[0] == data.z ? p0 : dataList[1] == data.z ? p1 : p2), obj);

                p0 = GetNextCellIndex(p0, dataList[0].direction, dataList[0].cells);
                if (!IsFirstCell(p0, dataList[0].direction, dataList[0].cells)) continue;
                p1 = GetNextCellIndex(p1, dataList[1].direction, dataList[1].cells);
                if (!IsFirstCell(p1, dataList[1].direction, dataList[1].cells)) continue;
                p2 = GetNextCellIndex(p2, dataList[2].direction, dataList[2].cells);
            }
            return objDictionary;
        }

        private static Dictionary<(int i, int j, int k), GameObject> SortByPosition(List<GameObject> selection, ArrangeData data, Bounds selectionBounds)
        {
            var maxSize = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            var averageSize = Vector3.zero;
            foreach (var obj in selection)
            {
                var objBounds = GetBoundsRecursive(obj.transform);
                maxSize = Vector3.Max(maxSize, objBounds.size);
                averageSize += objBounds.size;
            }
            averageSize /= selection.Count;
            var cellSize = new Vector3(
                data.x.cellSizeType == CellSizeType.BIGGEST_OBJECT ? maxSize.x : data.x.cellSizeType == CellSizeType.BIGGEST_OBJECT_PER_GROUP ? averageSize.x : data.x.cellSize,
                data.y.cellSizeType == CellSizeType.BIGGEST_OBJECT ? maxSize.y : data.y.cellSizeType == CellSizeType.BIGGEST_OBJECT_PER_GROUP ? averageSize.y : data.y.cellSize,
                data.z.cellSizeType == CellSizeType.BIGGEST_OBJECT ? maxSize.z : data.z.cellSizeType == CellSizeType.BIGGEST_OBJECT_PER_GROUP ? averageSize.z : data.z.cellSize);

            var firstCellCenter = selectionBounds.min + cellSize / 2f;

            var cellDict = new Dictionary<(int i, int j, int k), Bounds>();

            for (int k = 0; k < data.z.cells; ++k)
            {
                for (int j = 0; j < data.y.cells; ++j)
                {
                    for (int i = 0; i < data.x.cells; ++i)
                    {
                        var cellCenter = firstCellCenter + new Vector3(cellSize.x * i, cellSize.y * j, cellSize.z * k);
                        var cellBounds = new Bounds(cellCenter, cellSize);
                        cellDict.Add((i, j, k), cellBounds);
                    }
                }
            }
            var unsorted = new List<GameObject>(selection);
            var objDict = new Dictionary<(int i, int j, int k), GameObject>();

            while (unsorted.Count > 0)
            {
                var cellObjectsDict = new Dictionary<(int i, int j, int k), List<(GameObject obj, float sqrDistanceToCorner, float sqrDistanceToCenter)>>();
                foreach (var obj in unsorted)
                {
                    var objBounds = GetBoundsRecursive(obj.transform);
                    var minSqrDistanceToCorner = float.MaxValue;
                    var minSqrDistanceToCenter = float.MaxValue;
                    var closestCell = new KeyValuePair<(int i, int j, int k), Bounds>();
                    foreach (var cell in cellDict)
                    {
                        var objToCorner = new Vector3(
                            objBounds.min.x - cell.Value.min.x,
                            objBounds.min.y - cell.Value.min.y,
                            objBounds.min.z - cell.Value.min.z);
                        var sqrDistanceToCorner = Vector3.SqrMagnitude(objToCorner);
                        var sqrDistanceToCenter = Vector3.SqrMagnitude(objBounds.center - cell.Value.center);
                        if (sqrDistanceToCorner < minSqrDistanceToCorner)
                        {
                            minSqrDistanceToCorner = sqrDistanceToCorner;
                            minSqrDistanceToCenter = sqrDistanceToCenter;
                            closestCell = cell;
                        }
                        else if (minSqrDistanceToCorner == sqrDistanceToCorner && sqrDistanceToCenter < minSqrDistanceToCenter)
                        {
                            minSqrDistanceToCenter = sqrDistanceToCenter;
                            closestCell = cell;
                        }
                    }
                    if (cellObjectsDict.ContainsKey((closestCell.Key)))
                    {
                        cellObjectsDict[closestCell.Key].Add((obj, minSqrDistanceToCorner, minSqrDistanceToCenter));
                    }
                    else
                    {
                        cellObjectsDict.Add(closestCell.Key, new List<(GameObject obj, float sqrDistanceToCorner, float sqrDistanceToCenter)>());
                        cellObjectsDict[closestCell.Key].Add((obj, minSqrDistanceToCorner, minSqrDistanceToCenter));
                    }
                }

                foreach (var cellObjs in cellObjectsDict)
                {
                    var minSqrDistanceToCorner = cellObjs.Value[0].sqrDistanceToCorner;
                    var minSqrDistanceToCenter = cellObjs.Value[0].sqrDistanceToCenter;
                    GameObject closestObj = cellObjs.Value[0].obj;
                    for (int i = 1; i < cellObjs.Value.Count; ++i)
                    {
                        var objData = cellObjs.Value[i];
                        if (objData.sqrDistanceToCorner < minSqrDistanceToCorner)
                        {
                            minSqrDistanceToCorner = objData.sqrDistanceToCorner;
                            minSqrDistanceToCenter = objData.sqrDistanceToCenter;
                            closestObj = objData.obj;
                        }
                        else if (minSqrDistanceToCorner == objData.sqrDistanceToCorner && objData.sqrDistanceToCenter < minSqrDistanceToCenter)
                        {
                            minSqrDistanceToCenter = objData.sqrDistanceToCenter;
                            closestObj = objData.obj;
                        }
                    }
                    objDict.Add(cellObjs.Key, closestObj);
                    unsorted.Remove(closestObj);
                    cellDict.Remove(cellObjs.Key);
                }
            }
            return objDict;
        }

        public static bool Arrange(List<GameObject> selection, ArrangeData data)
        {
            if (selection.Count < 2 || selection.Count > data.x.cells * data.y.cells * data.z.cells) return false;
            var selectionBounds = GetSelectionBounds(selection);
            if (data.sortBy == SortBy.HIERARCHY)
            {
                selection = SortByHierarchy(selection);
            }
            Dictionary<(int i, int j, int k), GameObject> objDictionary = data.sortBy == SortBy.POSITION
                ? SortByPosition(selection, data, selectionBounds) : SortBySelectionOrder(selection, data);

            var maxSize = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (var obj in selection)
            {
                var objBounds = GetBoundsRecursive(obj.transform);
                maxSize = Vector3.Max(objBounds.size, maxSize);
            }

            maxSize = new Vector3(
                data.x.cellSizeType == CellSizeType.CUSTOM ? data.x.cellSize : maxSize.x,
                data.y.cellSizeType == CellSizeType.CUSTOM ? data.y.cellSize : maxSize.y,
                data.z.cellSizeType == CellSizeType.CUSTOM ? data.z.cellSize : maxSize.z);

            /////// X
            if (data.x.overwrite)
            {
                var prevMaxX = selectionBounds.min.x - data.x.spacing;
                for (int i = 0; i < data.x.cells; ++i)
                {
                    List<GameObject> objList = new System.Collections.Generic.List<GameObject>();
                    for (int j = 0; j < data.y.cells; ++j)
                    {
                        for (int k = 0; k < data.z.cells; ++k)
                        {
                            if (objDictionary.ContainsKey((i, j, k)))
                            {
                                objList.Add(objDictionary[(i, j, k)]);
                            }
                        }
                    }
                    Align(objList, RelativeTo.SELECTION, Axis.X, data.x.aligment, false);
                    var columnBounds = GetSelectionBounds(objList, RelativeTo.SELECTION, Axis.X).Item2;
                    foreach (var obj in objList)
                    {
                        var arrangedPosition = obj.transform.position;
                        var objBounds = GetBoundsRecursive(obj.transform);
                        var objMin = objBounds.min;
                        arrangedPosition.x = obj.transform.position.x - columnBounds.min.x + prevMaxX + data.x.spacing;
                        if (data.x.cellSizeType != CellSizeType.BIGGEST_OBJECT_PER_GROUP && i > 0)
                        {
                            switch (data.x.aligment)
                            {
                                case Bound.CENTER:
                                    arrangedPosition.x += (maxSize.x - columnBounds.size.x) / 2f;
                                    break;
                                case Bound.MAX:
                                    arrangedPosition.x += maxSize.x - columnBounds.size.x;
                                    break;
                                default: break;
                            }
                        }
                        obj.transform.position = arrangedPosition;
                    }
                    columnBounds = GetSelectionBounds(objList, RelativeTo.SELECTION, Axis.X).Item2;
                    if (data.x.cellSizeType != CellSizeType.BIGGEST_OBJECT_PER_GROUP)
                    {
                        prevMaxX += maxSize.x + data.x.spacing;
                        if (i != 0) continue;
                        switch (data.x.aligment)
                        {
                            case Bound.CENTER:
                                prevMaxX -= (maxSize.x - columnBounds.size.x) / 2f;
                                break;
                            case Bound.MAX:
                                prevMaxX -= (maxSize.x - columnBounds.size.x);
                                break;
                            default: break;
                        }
                    }
                    else
                    {
                        prevMaxX = columnBounds.max.x;
                    }
                }
            }
            ///// Y
            if (data.y.overwrite)
            {
                var prevMaxY = selectionBounds.min.y - data.y.spacing;
                for (int j = 0; j < data.y.cells; ++j)
                {
                    List<GameObject> objList = new System.Collections.Generic.List<GameObject>();
                    for (int i = 0; i < data.x.cells; ++i)
                    {
                        for (int k = 0; k < data.z.cells; ++k)
                        {
                            if (objDictionary.ContainsKey((i, j, k)))
                            {
                                objList.Add(objDictionary[(i, j, k)]);
                            }
                        }
                    }
                    Align(objList, RelativeTo.SELECTION, Axis.Y, data.y.aligment, false);
                    var rowBounds = GetSelectionBounds(objList, RelativeTo.SELECTION, Axis.Y).Item2;

                    foreach (var obj in objList)
                    {
                        var arrangedPosition = obj.transform.position;
                        var objBounds = GetBoundsRecursive(obj.transform);
                        var objMin = objBounds.min;
                        arrangedPosition.y = obj.transform.position.y - rowBounds.min.y + prevMaxY + data.y.spacing;
                        if (data.y.cellSizeType != CellSizeType.BIGGEST_OBJECT_PER_GROUP && j > 0)
                        {
                            switch (data.y.aligment)
                            {
                                case Bound.CENTER:
                                    arrangedPosition.y += (maxSize.y - rowBounds.size.y) / 2f;
                                    break;
                                case Bound.MAX:
                                    arrangedPosition.y += maxSize.y - rowBounds.size.y;
                                    break;
                                default: break;
                            }
                        }
                        obj.transform.position = arrangedPosition;
                    }
                    rowBounds = GetSelectionBounds(objList, RelativeTo.SELECTION, Axis.Y).Item2;
                    if (data.y.cellSizeType != CellSizeType.BIGGEST_OBJECT_PER_GROUP)
                    {
                        prevMaxY += maxSize.y + data.y.spacing;
                        if (j != 0) continue;
                        switch (data.y.aligment)
                        {
                            case Bound.CENTER:
                                prevMaxY -= (maxSize.y - rowBounds.size.y) / 2f;
                                break;
                            case Bound.MAX:
                                prevMaxY -= (maxSize.y - rowBounds.size.y);
                                break;
                            default: break;
                        }
                    }
                    else
                    {
                        prevMaxY = rowBounds.max.y;
                    }
                }
            }
            ///// Z
            if (data.z.overwrite)
            {
                var prevMaxZ = selectionBounds.min.z - data.z.spacing;
                for (int k = 0; k < data.z.cells; ++k)
                {
                    List<GameObject> objList = new System.Collections.Generic.List<GameObject>();
                    for (int j = 0; j < data.y.cells; ++j)
                    {
                        for (int i = 0; i < data.x.cells; ++i)
                        {
                            if (objDictionary.ContainsKey((i, j, k)))
                            {
                                objList.Add(objDictionary[(i, j, k)]);
                            }
                        }
                    }
                    Align(objList, RelativeTo.SELECTION, Axis.Z, data.z.aligment, false);
                    var columnBounds = GetSelectionBounds(objList, RelativeTo.SELECTION, Axis.Z).Item2;

                    foreach (var obj in objList)
                    {
                        var arrangedPosition = obj.transform.position;
                        var objBounds = GetBoundsRecursive(obj.transform);
                        var objMin = objBounds.min;
                        arrangedPosition.z = obj.transform.position.z - columnBounds.min.z + prevMaxZ + data.z.spacing;
                        if (data.z.cellSizeType != CellSizeType.BIGGEST_OBJECT_PER_GROUP && k > 0)
                        {
                            switch (data.z.aligment)
                            {
                                case Bound.CENTER:
                                    arrangedPosition.z += (maxSize.z - columnBounds.size.z) / 2f;
                                    break;
                                case Bound.MAX:
                                    arrangedPosition.z += maxSize.z - columnBounds.size.z;
                                    break;
                                default: break;
                            }
                        }
                        obj.transform.position = arrangedPosition;
                    }
                    columnBounds = GetSelectionBounds(objList, RelativeTo.SELECTION, Axis.Z).Item2;
                    if (data.z.cellSizeType != CellSizeType.BIGGEST_OBJECT_PER_GROUP)
                    {
                        prevMaxZ += maxSize.z + data.z.spacing;
                        if (k != 0) continue;
                        switch (data.z.aligment)
                        {
                            case Bound.CENTER:
                                prevMaxZ -= (maxSize.z - columnBounds.size.z) / 2f;
                                break;
                            case Bound.MAX:
                                prevMaxZ -= (maxSize.z - columnBounds.size.z);
                                break;
                            default: break;
                        }
                    }
                    else
                    {
                        prevMaxZ = columnBounds.max.z;
                    }
                }
            }
            return true;
        }
        #endregion
        #region REARRANGE
        public static void Rearrange(List<GameObject> selection, ArrangeBy arrangeBy)
        {
            if (selection.Count < 2) return;
            if (arrangeBy == ArrangeBy.HIERARCHY_ORDER)
            {
                selection = SortByHierarchy(selection);
            }
            var firstPosition = selection[0].transform.position;
            for (int i = 0; i < selection.Count - 1; ++i)
            {
                selection[i].transform.position = selection[i + 1].transform.position;
            }
            selection[selection.Count - 1].transform.position = firstPosition;
        }
        #endregion
        #region RADIAL ARRANGE
        public enum RotateAround
        {
            SELECTION_CENTER,
            TRANSFORM_POSITION,
            OBJECT_BOUNDS_CENTER,
            CUSTOM_POSITION
        }

        public enum Shape
        {
            CIRCLE,
            CIRCULAR_SPIRAL,
            ELLIPSE,
            ELLIPTICAL_SPIRAL
        }

        public class RadialArrangeData
        {
            private ArrangeBy _arrangeBy = ArrangeBy.SELECTION_ORDER;
            private RotateAround _rotateAround = RotateAround.SELECTION_CENTER;
            private Transform _centerTransform = null;
            private Vector3 _center = Vector3.zero;
            private Vector3 _axis = Vector3.forward;
            private Shape _shape = Shape.CIRCLE;
            private Vector2 _startEllipseAxes = Vector2.one;
            private Vector2 _endEllipseAxes = Vector2.one;
            private float _startAngle = 0f;
            private float _maxArcAngle = 360f;
            private bool _orientToRadius = false;
            private Vector3 _orientDirection = Vector3.right;
            private Vector3 _parallelDirection = Vector3.up;
            private bool _overwriteX = true;
            private bool _overwriteY = true;
            private bool _overwriteZ = true;
            private bool _lastSpotEmpty = false;

            public ArrangeBy arrangeBy { get => _arrangeBy; set => _arrangeBy = value; }
            public Vector3 axis { get => _axis; set => _axis = value; }
            public Shape shape { get => _shape; set => _shape = value; }
            public Vector2 startEllipseAxes { get => _startEllipseAxes; set => _startEllipseAxes = value; }
            public Vector2 endEllipseAxes { get => _endEllipseAxes; set => _endEllipseAxes = value; }
            public float startAngle { get => _startAngle; set => _startAngle = value; }
            public float maxArcAngle { get => _maxArcAngle; set => _maxArcAngle = value; }
            public bool orientToRadius { get => _orientToRadius; set => _orientToRadius = value; }
            public Vector3 center { get => _center; set => _center = value; }
            public Vector3 orientDirection { get => _orientDirection; set => _orientDirection = value; }
            public Vector3 parallelDirection { get => _parallelDirection; set => _parallelDirection = value; }
            public Transform centerTransform
            {
                get => _centerTransform;
                set
                {
                    if (_centerTransform == value) return;
                    _centerTransform = value;
                    UpdateCenter();
                }
            }
            public RotateAround rotateAround
            {
                get => _rotateAround;
                set
                {
                    if (_rotateAround == value) return;
                    _rotateAround = value;
                    UpdateCenter();
                }
            }

            public bool overwriteX { get => _overwriteX; set => _overwriteX = value; }
            public bool overwriteY { get => _overwriteY; set => _overwriteY = value; }
            public bool overwriteZ { get => _overwriteZ; set => _overwriteZ = value; }
            public bool LastSpotEmpty { get => _lastSpotEmpty; set => _lastSpotEmpty = value; }

            public void UpdateCenter()
            {
                if (_centerTransform == null &&
                    (_rotateAround == RotateAround.TRANSFORM_POSITION
                    || _rotateAround == RotateAround.OBJECT_BOUNDS_CENTER))
                {
                    _center = Vector3.zero;
                }
                else if (_rotateAround == RotateAround.TRANSFORM_POSITION)
                {
                    _center = _centerTransform.transform.position;
                }
                else if (_rotateAround == RotateAround.OBJECT_BOUNDS_CENTER)
                {
                    _center = GetBoundsRecursive(_centerTransform).center;
                }
            }

            public void UpdateCenter(List<GameObject> selection)
            {
                if (_rotateAround != RotateAround.SELECTION_CENTER) return;
                if (selection.Count == 0)
                {
                    _center = Vector3.zero;
                }
                else
                {
                    _center = GetSelectionBounds(selection).center;
                }
            }
        }

        private static float GetEllipseRadius(Vector2 ellipseAxes, float angle)
        {
            if (ellipseAxes.x == ellipseAxes.y) return ellipseAxes.x;
            var a = ellipseAxes.x;
            var b = ellipseAxes.y;
            var sin = Mathf.Sin(angle * Mathf.Deg2Rad);
            var cos = Mathf.Cos(angle * Mathf.Deg2Rad);
            return a * b / Mathf.Sqrt(a * a * sin * sin + b * b * cos * cos);
        }

        private static Vector3 GetRadialPosition(Vector3 center, Vector3 axis, float radius, float angle)
        {
            var radiusDirection = Vector3.right;
            if (axis.x > 0 || axis.y < 0)
            {
                radiusDirection = Vector3.forward;
            }
            else if (axis.x < 0 || axis.z > 0)
            {
                radiusDirection = Vector3.up;
            }
            return center + Quaternion.AngleAxis(angle, axis) * radiusDirection * radius;
        }

        public static void RadialArrange(List<GameObject> selection, RadialArrangeData data)
        {
            if (data.arrangeBy == ArrangeBy.HIERARCHY_ORDER)
            {
                selection = SortByHierarchy(selection);
            }
            data.UpdateCenter();
            var angle = data.startAngle;

            var deltaAngle = data.maxArcAngle / ((float)selection.Count - (data.LastSpotEmpty ? 0f : 1f));
            var ellipseAxes = data.startEllipseAxes;
            var deltaEllipseAxes = (data.endEllipseAxes - data.startEllipseAxes) / ((float)selection.Count - 1);
            foreach (var obj in selection)
            {
                UnityEditor.Undo.RecordObject(obj.transform, "Radial Arrange");
                var radius = GetEllipseRadius(ellipseAxes, angle);
                var position = GetRadialPosition(data.center, data.axis, radius, angle);
                obj.transform.position = new Vector3(
                    data.overwriteX ? position.x : obj.transform.position.x,
                    data.overwriteY ? position.y : obj.transform.position.y,
                    data.overwriteZ ? position.z : obj.transform.position.z);
                if (data.orientToRadius)
                {
                    obj.transform.rotation = Quaternion.identity;
                    LookAtCenter(obj.transform, data.center, data.axis, data.orientDirection, data.parallelDirection);
                }
                angle += deltaAngle;
                ellipseAxes += deltaEllipseAxes;
            }
        }
        #endregion
        #region PROGRESSIVE TRANSFORM
        public enum IncrementalDataType
        {
            CONSTANT_DELTA,
            CURVE,
            OBJECT_SIZE
        }

        public enum ArrangeBy
        {
            SELECTION_ORDER,
            HIERARCHY_ORDER
        }

        public class ProgressiveAxisData
        {
            private float _constantDelta = 0f;
            private AnimationCurve _curve = AnimationCurve.Constant(0, 1, 0);
            private float _curveRangeMin = 0f;
            private float _curveRangeSize = 0f;
            private Rect _curveRange = new Rect(0, 0, 1, 1);
            private bool _overwrite = true;

            public float constantDelta { get => _constantDelta; set => _constantDelta = value; }
            public AnimationCurve curve { get => _curve; set => _curve = value; }
            public float curveRangeMin { get => _curveRangeMin; set => _curveRangeMin = value; }
            public float curveRangeSize { get => _curveRangeSize; set => _curveRangeSize = value; }
            public Rect curveRange { get => _curveRange; set => _curveRange = value; }
            public bool overwrite { get => _overwrite; set => _overwrite = value; }
        }

        public class IncrementalData
        {
            private ArrangeBy _arrangeOrder = ArrangeBy.HIERARCHY_ORDER;
            private IncrementalDataType _type = IncrementalDataType.CONSTANT_DELTA;
            private ProgressiveAxisData _x = new ProgressiveAxisData();
            private ProgressiveAxisData _y = new ProgressiveAxisData();
            private ProgressiveAxisData _z = new ProgressiveAxisData();

            public ArrangeBy arrangeOrder { get => _arrangeOrder; set => _arrangeOrder = value; }
            public IncrementalDataType type { get => _type; set => _type = value; }
            public Vector3 constantDelta
            {
                get => new Vector3(_x.constantDelta, _y.constantDelta, _z.constantDelta);
                set
                {
                    _x.constantDelta = value.x;
                    _y.constantDelta = value.y;
                    _z.constantDelta = value.z;
                }
            }
            public Vector3 curveRangeMin
            {
                get => new Vector3(_x.curveRangeMin, _y.curveRangeMin, _z.curveRangeMin);
                set
                {
                    if (new Vector3(_x.curveRangeMin, _y.curveRangeMin, _z.curveRangeMin) == value) return;
                    var rangeX = _x.curveRange;
                    rangeX.yMin = _x.curveRangeMin = value.x;
                    _x.curveRange = rangeX;
                    var rangeY = _y.curveRange;
                    rangeY.yMin = _y.curveRangeMin = value.y;
                    _y.curveRange = rangeY;
                    var rangeZ = _z.curveRange;
                    rangeZ.yMin = _z.curveRangeMin = value.z;
                    _z.curveRange = rangeZ;
                    UpdateRanges();
                }
            }
            public Vector3 curveRangeSize
            {
                get => new Vector3(_x.curveRangeSize, _y.curveRangeSize, _z.curveRangeSize);
                set
                {
                    if (new Vector3(_x.curveRangeSize, _y.curveRangeSize, _z.curveRangeSize) == value) return;
                    _x.curveRangeSize = value.x;
                    _y.curveRangeSize = value.y;
                    _z.curveRangeSize = value.z;
                    UpdateRanges();
                }
            }

            public ProgressiveAxisData x { get => _x; set => _x = value; }
            public ProgressiveAxisData y { get => _y; set => _y = value; }
            public ProgressiveAxisData z { get => _z; set => _z = value; }

            private void UpdateRanges()
            {
                var rangeX = _x.curveRange;
                rangeX.yMax = _x.curveRangeMin + _x.curveRangeSize;
                _x.curveRange = rangeX;
                var rangeY = _y.curveRange;
                rangeY.yMax = _y.curveRangeMin + _y.curveRangeSize;
                _y.curveRange = rangeY;
                var rangeZ = _z.curveRange;
                rangeZ.yMax = _z.curveRangeMin + _z.curveRangeSize;
                _z.curveRange = rangeZ;
            }

            public Vector3 EvaluateCurve(float t)
            {
                return new Vector3(
                    _x.overwrite ? _x.curve.Evaluate(t) : 0f,
                    _y.overwrite ? _y.curve.Evaluate(t) : 0f,
                    _z.overwrite ? _z.curve.Evaluate(t) : 0f);
            }

            public Rect GetRect(Axis axis)
            {
                switch (axis)
                {
                    case Axis.X:
                        return _x.curveRange;
                    case Axis.Y:
                        return _y.curveRange;
                    default:
                        return _z.curveRange;
                }
            }
        }
        private static int[] GetHierarchyIndex(GameObject obj)
        {
            var idxList = new List<int>();
            var parent = obj.transform;
            do
            {
                idxList.Insert(0, parent.transform.GetSiblingIndex());
                parent = parent.transform.parent;
            }
            while (parent != null);
            return idxList.ToArray();
        }

        public static void IncrementalPosition(List<GameObject> selection, IncrementalData data, bool orientToPath, Vector3 orientation)
        {
            if (selection.Count < 2) return;
            if (data.arrangeOrder == ArrangeBy.HIERARCHY_ORDER)
            {
                selection = SortByHierarchy(selection);
            }
            var position = selection[0].transform.position;
            var t = 0f;
            var delta = 1f / ((float)selection.Count - 1f);
            var i = 0;
            GameObject prevObj = null;
            foreach (var obj in selection)
            {
                var bounds = GetBoundsRecursive(obj.transform);
                var centerLocalPos = obj.transform.TransformVector(obj.transform.InverseTransformPoint(bounds.center));

                if (i > 0 && data.type == IncrementalDataType.OBJECT_SIZE)
                {
                    position += bounds.size / 2f - centerLocalPos;
                }
                ++i;
                if (!orientToPath || (orientToPath && data.type != IncrementalDataType.OBJECT_SIZE))
                {
                    UnityEditor.Undo.RecordObject(obj.transform, "Progressive Position");
                }
                obj.transform.position = new Vector3(
                    data.x.overwrite ? position.x : obj.transform.position.x,
                    data.y.overwrite ? position.y : obj.transform.position.y,
                    data.z.overwrite ? position.z : obj.transform.position.z);
                t += delta;

                position = data.type == IncrementalDataType.CONSTANT_DELTA
                    ? position + data.constantDelta
                    : data.type == IncrementalDataType.CURVE
                        ? selection[0].transform.position + data.EvaluateCurve(t)
                        : position + centerLocalPos + bounds.size / 2f;

                if (!orientToPath) continue;
                if (data.type != IncrementalDataType.OBJECT_SIZE)
                {
                    LookAtNext(obj.transform, position, orientation);
                }
                else if(i > 1)
                {
                    UnityEditor.Undo.RecordObject(prevObj.transform, "Progressive Position");
                    LookAtNext(prevObj.transform, obj.transform.position, orientation);
                }
                if (data.type == IncrementalDataType.OBJECT_SIZE && i == selection.Count)
                {
                    UnityEditor.Undo.RecordObject(obj.transform, "Progressive Position");
                    obj.transform.eulerAngles = prevObj.transform.eulerAngles;
                }
                prevObj = obj;
            }
        }

        private static void LookAtNext(Transform transform, Vector3 next, Vector3 orientation)
        {
            var objToCenter = next - transform.position;
            transform.rotation = Quaternion.FromToRotation(orientation, objToCenter);
        }

        public static void IncrementalRotation(List<GameObject> selection, IncrementalData data)
        {
            if (selection.Count < 2) return;
            if (data.arrangeOrder == ArrangeBy.HIERARCHY_ORDER)
            {
                selection = SortByHierarchy(selection);
            }
            var eulerAngles = selection[0].transform.rotation.eulerAngles;
            var firstObjEulerAngles = eulerAngles;
            var t = 0f;
            foreach (var obj in selection)
            {
                UnityEditor.Undo.RecordObject(obj.transform, "Progressive Rotation");
                if (data.type == IncrementalDataType.CURVE)
                {
                    eulerAngles = firstObjEulerAngles + data.EvaluateCurve(t);
                    t += 1f / ((float)selection.Count - 1f);
                }
                obj.transform.rotation = Quaternion.Euler(
                    data.x.overwrite ? eulerAngles.x : obj.transform.rotation.eulerAngles.x,
                    data.y.overwrite ? eulerAngles.y : obj.transform.rotation.eulerAngles.y,
                    data.z.overwrite ? eulerAngles.z : obj.transform.rotation.eulerAngles.z);
                if (data.type == IncrementalDataType.CONSTANT_DELTA)
                {
                    eulerAngles += data.constantDelta;
                }
            }
        }

        public static void IncrementalScale(List<GameObject> selection, IncrementalData data)
        {
            if (selection.Count < 2) return;
            if (data.arrangeOrder == ArrangeBy.HIERARCHY_ORDER)
            {
                selection = SortByHierarchy(selection);
            }
            var scale = selection[0].transform.localScale;
            var firstObjScale = scale;
            var t = 0f;
            foreach (var obj in selection)
            {
                UnityEditor.Undo.RecordObject(obj.transform, "Progressive Rotation");

                if(data.type == IncrementalDataType.CURVE)
                {
                    scale = firstObjScale + data.EvaluateCurve(t);
                    t += 1f / ((float)selection.Count - 1f);
                }
                
                obj.transform.localScale = new Vector3(
                    data.x.overwrite ? scale.x : obj.transform.localScale.x,
                    data.y.overwrite ? scale.y : obj.transform.localScale.y,
                    data.z.overwrite ? scale.z : obj.transform.localScale.z);

                if(data.type == IncrementalDataType.CONSTANT_DELTA)
                {
                    scale += data.constantDelta;
                }
            }
        }
        #endregion
        #region RANDOMIZE
        public class Range
        {
            private float _min = -1f;
            private float _max = 1f;
            public float min
            {
                get => _min;
                set
                {
                    if (_min == value) return;
                    _min = value;
                    if (_min > _max)
                    {
                        _max = _min;
                    }
                }
            }
            public float max
            {
                get => _max;
                set
                {
                    if (_max == value) return;
                    _max = value;
                    if (_max < _min)
                    {
                        _min = _max;
                    }
                }
            }

            public float randomValue
            {
                get
                {
                    return UnityEngine.Random.Range(min, max);
                }
            }
        }
        public class RandomizeAxisData
        {
            private bool _randomizeAxis = true;
            private Range _offset = new Range();
            public bool randomizeAxis { get => _randomizeAxis; set => _randomizeAxis = value; }
            public Range offset { get => _offset; set => _offset = value; }

        }
        public class RandomizeData
        {
            private RandomizeAxisData _x = new RandomizeAxisData();
            private RandomizeAxisData _y = new RandomizeAxisData();
            private RandomizeAxisData _z = new RandomizeAxisData();

            public RandomizeAxisData x { get => _x; set => _x = value; }
            public RandomizeAxisData y { get => _y; set => _y = value; }
            public RandomizeAxisData z { get => _z; set => _z = value; }
        }
        public static void RandomizePositions(GameObject[] selection, RandomizeData data)
        {
            foreach (var obj in selection)
            {
                Undo.RecordObject(obj.transform, "Randomize Position");
                obj.transform.position += new Vector3(
                    data.x.randomizeAxis ? data.x.offset.randomValue : 0f,
                    data.y.randomizeAxis ? data.y.offset.randomValue : 0f,
                    data.z.randomizeAxis ? data.z.offset.randomValue : 0f);
            }
        }

        public static void RandomizeRotations(GameObject[] selection, RandomizeData data)
        {
            foreach (var obj in selection)
            {
                Undo.RecordObject(obj.transform, "Randomize Rotation");
                obj.transform.Rotate(
                    data.x.randomizeAxis ? data.x.offset.randomValue : 0f,
                    data.y.randomizeAxis ? data.y.offset.randomValue : 0f,
                    data.z.randomizeAxis ? data.z.offset.randomValue : 0f);
            }
        }

        public static void RandomizeScales(GameObject[] selection, RandomizeData data, bool separateAxes)
        {
            foreach (var obj in selection)
            {
                Undo.RecordObject(obj.transform, "Randomize Rotation");
                if (separateAxes)
                {
                    obj.transform.localScale += new Vector3(
                        data.x.randomizeAxis ? data.x.offset.randomValue : obj.transform.localScale.x,
                        data.y.randomizeAxis ? data.y.offset.randomValue : obj.transform.localScale.y,
                        data.z.randomizeAxis ? data.z.offset.randomValue : obj.transform.localScale.z);
                }
                else
                {
                    var value = data.x.offset.randomValue;
                    obj.transform.localScale += new Vector3(value, value, value);
                }
            }
        }
        #endregion
        #region UNOVERLAP
        public class UnoverlapAxisData
        {
            private bool _unoverlap = true;
            private float _minDistance = 0f;
            public bool unoverlap { get => _unoverlap; set => _unoverlap = value; }
            public float minDistance { get => _minDistance; set => _minDistance = value; }
            public UnoverlapAxisData(bool unoverlap = true, float minDistance = 0f)
            {
                _unoverlap = unoverlap;
                _minDistance = minDistance;
            }
        }

        public class UnoverlapData
        {
            private UnoverlapAxisData _x = new UnoverlapAxisData();
            private UnoverlapAxisData _y = new UnoverlapAxisData();
            private UnoverlapAxisData _z = new UnoverlapAxisData();

            public UnoverlapAxisData x { get => _x; set => _x = value; }
            public UnoverlapAxisData y { get => _y; set => _y = value; }
            public UnoverlapAxisData z { get => _z; set => _z = value; }

            public UnoverlapAxisData GetData(Axis axis)
            {
                return axis == Axis.X ? _x : axis == Axis.Y ? _y : _z;
            }
        }

        private class OverlapData
        {
            public Vector3 size = Vector3.zero;
            public float volume = 0f;
            public Vector3 solution = Vector3.zero;
            public float solutionVolume = 0f;
        }

        private class OverlapedObject
        {
            public Bounds bounds;
            public int objId = -1;
            public Vector3 transformPosition = Vector3.zero;
            public List<OverlapData> _dataList = new List<OverlapData>();
            public int moveCount = 0;
            public bool isOverlaped
            {
                get
                {
                    foreach (var data in _dataList)
                    {
                        if (data.volume != 0)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            public float overlapedVolume
            {
                get
                {
                    var retVal = 0f;
                    foreach (var data in _dataList)
                    {
                        retVal += data.volume;
                    }
                    return retVal;
                }
            }

            public float solutionVolume
            {
                get
                {
                    var retVal = 0f;
                    foreach (var data in _dataList)
                    {
                        retVal += data.solutionVolume;
                    }
                    return retVal;
                }
            }

            public List<Vector3> solutions
            {
                get
                {
                    var retVal = _dataList.Select(data => data.solution).ToList();
                    retVal.Sort((s1, s2) => s1.magnitude.CompareTo(s2.magnitude));
                    return retVal;
                }
            }

            private void MoveTest(Axis axis, bool unoverlap, Vector3 solution, ref float minVol, ref Vector3 bestMove)
            {
                var moveSize = GetVectorComponent(solution, axis);
                if (!unoverlap || moveSize == 0) return;
                var move = GetMoveVector(moveSize, axis);
                var obj = new OverlapedObject();
                obj.bounds = bounds;
                obj.bounds.center += move;
                var vol = obj.solutionVolume;
                if (vol < minVol && Mathf.Abs(moveSize) < bestMove.magnitude)
                {
                    minVol = vol;
                    bestMove = move;
                }
            }
            public bool ExecuteBestSolution(OverlapedObject[] selection, UnoverlapData unoverlapData)
            {
                var bestMove = new Vector3(100000, 100000, 100000);
                var minVol = overlapedVolume;
                foreach (var solution in solutions)
                {
                    MoveTest(Axis.X, unoverlapData.x.unoverlap, solution, ref minVol, ref bestMove);
                    MoveTest(Axis.Y, unoverlapData.y.unoverlap, solution, ref minVol, ref bestMove);
                    MoveTest(Axis.Z, unoverlapData.z.unoverlap, solution, ref minVol, ref bestMove);
                }
                if (overlapedVolume > minVol)
                {
                    bounds.center += bestMove;
                    transformPosition = transformPosition + bestMove;
                    ++moveCount;
                    return true;
                }
                return false;
            }
        }

        private static float GetVectorComponent(Vector3 v, Axis axis)
        {
            return axis == Axis.X ? v.x : axis == Axis.Y ? v.y : v.z;
        }

        private static Vector3 GetMoveVector(float move, Axis axis)
        {
            return new Vector3(axis == Axis.X ? move : 0f, axis == Axis.Y ? move : 0f, axis == Axis.Z ? move : 0f);
        }

        private static void GetOverlapedDataAxis(Axis axis, UnoverlapData unoverlapData, OverlapedObject[] selection, int index, Bounds b1, Bounds b2,
            bool getSolutionVolumen, ref OverlapData retVal, ref float minVol, ref Vector3 bestMove)
        {
            var retValSize = GetVectorComponent(retVal.size, axis);
            if (!unoverlapData.GetData(axis).unoverlap || retValSize <= 0) return;
            var tempObj = new OverlapedObject();
            var tempSelection = selection.ToList();
            tempSelection.RemoveAt(index);
            tempSelection.Insert(0, tempObj);

            var b1Min = GetVectorComponent(b1.min, axis);
            var b1Max = GetVectorComponent(b1.max, axis);
            var b2Min = GetVectorComponent(b2.min, axis);
            var b2Max = GetVectorComponent(b2.max, axis);

            var pSol = b2Max - b1Min;
            var nSol = b2Min - b1Max;

            var moveSize = pSol < -nSol ? pSol : nSol;
            var move = GetMoveVector(moveSize, axis);
            if (getSolutionVolumen)
            {
                tempObj.bounds = b1;
                tempObj.bounds.center += move;

                tempObj._dataList = GetOverlapedData(tempSelection.ToArray(), 0, unoverlapData, false);
                var vol = tempObj.overlapedVolume;
                if (vol < minVol || (vol == minVol && Mathf.Abs(moveSize) < bestMove.magnitude))
                {
                    minVol = vol;
                    bestMove = move;
                    retVal.solution = bestMove;
                    retVal.solutionVolume = tempObj.overlapedVolume;
                }
            }
            else
            {
                retVal.solution = move;
                retVal.solutionVolume = retVal.volume;
            }
        }

        private static OverlapData GetOverlapedData(OverlapedObject[] selection, int index, Bounds b2, UnoverlapData unoverlapData, bool getSolutionVolumen)
        {
            Bounds b1 = selection[index].bounds;
            var min = Vector3.Max(b1.min, b2.min);
            var max = Vector3.Min(b1.max, b2.max);

            var retVal = new OverlapData();
            retVal.size = Vector3.Max(max - min, Vector3.zero);

            retVal.volume = (unoverlapData.x.unoverlap ? retVal.size.x : 1f) * (unoverlapData.y.unoverlap ? retVal.size.y : 1f) * (unoverlapData.z.unoverlap ? retVal.size.z : 1f);

            if (retVal.volume > 0)
            {
                var bestMove = new Vector3(100000, 100000, 100000);
                var minVol = float.MaxValue;
                GetOverlapedDataAxis(Axis.X, unoverlapData, selection, index, b1, b2, getSolutionVolumen, ref retVal, ref minVol, ref bestMove);
                GetOverlapedDataAxis(Axis.Y, unoverlapData, selection, index, b1, b2, getSolutionVolumen, ref retVal, ref minVol, ref bestMove);
                GetOverlapedDataAxis(Axis.Z, unoverlapData, selection, index, b1, b2, getSolutionVolumen, ref retVal, ref minVol, ref bestMove);
            }
            return retVal;
        }

        private static List<OverlapData> GetOverlapedData(OverlapedObject[] selection, int index, UnoverlapData unoverlapData, bool getSolutionVolumen)
        {
            var retVal = new List<OverlapData>();
            var target = selection[index];
            foreach (var obj in selection)
            {
                if (obj == target) continue;
                var data = GetOverlapedData(selection, index, obj.bounds, unoverlapData, getSolutionVolumen);
                if (data.size != Vector3.zero && data.solution != Vector3.zero)
                {
                    retVal.Add(data);
                    if (retVal.Count >= 3) break;
                }
            }
            return retVal;
        }

        private static float GetBoundsVolume(Bounds bounds)
        {
            var size = Vector3.Max(bounds.size, new Vector3(0.001f, 0.001f, 0.001f));
            return size.x * size.y * size.z;
        }
        private static int CompareOverlapedObjects(OverlapedObject obj1, OverlapedObject obj2)
        {

            if (obj1.moveCount < obj2.moveCount)
            {
                return -1;
            }
            else if (obj1.moveCount > obj2.moveCount)
            {
                return 1;
            }
            else
            {
                float obj1Vol = GetBoundsVolume(obj1.bounds);
                float obj2Vol = GetBoundsVolume(obj2.bounds);
                float v1 = obj1.overlapedVolume / obj1Vol;
                float v2 = obj2.overlapedVolume / obj2Vol;
                if (v1 == v2)
                {
                    var r = obj1Vol.CompareTo(obj2Vol);
                    if (r != 0) return r;
                    v1 = obj1.solutionVolume / obj1Vol;
                    v2 = obj2.solutionVolume / obj2Vol;
                    return v1.CompareTo(v2);
                }
                else if (v1 == 0)
                {
                    return 1;
                }
                else if (v2 == 0)
                {
                    return -1;
                }
                else
                {
                    var r = v1.CompareTo(v2);
                    if (r != 0) return r;
                    return obj1Vol.CompareTo(obj2Vol);
                }
            }
        }

        public class Unoverlapper
        {
            private readonly (int objId, Bounds bounds)[] _selection;
            private readonly UnoverlapData _unoverlapData;
            private bool _cancel = false;
            public Unoverlapper((int objId, Bounds bounds)[] selection, UnoverlapData unoverlapData)
            {
                _selection = selection;
                _unoverlapData = unoverlapData;
            }

            public event Action<float> progressChanged;
            public event Action<(int objId, Vector3 offset)[]> OnDone;

            public void RemoveOverlaps()
            {
                if (!_unoverlapData.x.unoverlap && !_unoverlapData.y.unoverlap && !_unoverlapData.z.unoverlap) return;
                var minSize = new Vector3(0.001f, 0.001f, 0.001f);

                var overlapedList = new List<OverlapedObject>();

                foreach (var obj in _selection)
                {
                    var overlapedObj = new OverlapedObject();
                    overlapedObj.bounds = obj.bounds;
                    overlapedObj.objId = obj.objId;
                    overlapedObj.bounds.center = new Vector3
                        (_unoverlapData.x.unoverlap ? overlapedObj.bounds.center.x : 0f,
                        _unoverlapData.y.unoverlap ? overlapedObj.bounds.center.y : 0f,
                        _unoverlapData.z.unoverlap ? overlapedObj.bounds.center.z : 0f);
                    overlapedObj.bounds.size = Vector3.Max(overlapedObj.bounds.size, minSize) + new Vector3(_unoverlapData.x.minDistance, _unoverlapData.y.minDistance, _unoverlapData.z.minDistance);
                    overlapedList.Add(overlapedObj);
                }

                var i = 0;
                foreach (var obj in overlapedList)
                {
                    obj._dataList = GetOverlapedData(overlapedList.ToArray(), i, _unoverlapData, true);
                    ++i;
                }

                overlapedList.Sort((obj1, obj2) => CompareOverlapedObjects(obj1, obj2));
                var prevProgress = 0f;
                var overlapedObjects = 0;
                do
                {
                    if (_cancel) return;
                    var first = overlapedList[0];
                    if (!first.isOverlaped)
                    {
                        overlapedList.RemoveAt(0);
                        overlapedList.Add(first);
                        continue;
                    }
                    else
                    {
                        var executed = first.ExecuteBestSolution(overlapedList.ToArray(), _unoverlapData);
                        if (!executed)
                        {
                            overlapedList.RemoveAt(0);
                            overlapedList.Add(first);
                            continue;
                        }
                        else
                        {
                            overlapedList.Sort((obj1, obj2) => CompareOverlapedObjects(obj1, obj2));
                            if (overlapedList[0] == first)
                            {
                                overlapedList.RemoveAt(0);
                                overlapedList.Add(first);
                            }
                        }
                    }
                    overlapedObjects = 0;
                    i = 0;
                    foreach (var obj in overlapedList)
                    {
                        obj._dataList = GetOverlapedData(overlapedList.ToArray(), i, _unoverlapData, true);
                        ++i;
                        if (obj.isOverlaped)
                        {
                            ++overlapedObjects;
                        }
                    }

                    var progress = Mathf.Max(1f - (float)overlapedObjects / (float)_selection.Length, prevProgress);
                    if (prevProgress != progress)
                    {
                        progressChanged(progress);
                    }
                    prevProgress = progress;
                } while (overlapedObjects > 0);
                var boundsArray = overlapedList.Select(obj => (obj.objId, obj.transformPosition)).ToArray();
                OnDone(boundsArray);
            }

            public void Cancel()
            {
                _cancel = true;
            }
        }
        #endregion
        #region EDIT PIVOT
        private const string UNDO_EDIT_PIVOT = "Edit Pivot";
        public static GameObject StartEditingPivot(GameObject target)
        {
            if (target == null || target.scene.rootCount == 0) return null;
            var pivot = new GameObject("Pivot", typeof(Pivot));
            pivot.transform.parent = target.transform;
            pivot.transform.localPosition = Vector3.zero;
            pivot.transform.localRotation = Quaternion.identity;
            pivot.transform.localScale = Vector3.one;
            Selection.activeGameObject = pivot;
            return pivot;
        }
        public static void SaveMesh(MeshFilter meshFilter, string savePath, Transform pivot)
        {
            Mesh mesh = meshFilter.sharedMesh;
            var otherFilters = new List<MeshFilter>();
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(mesh)))
            {
                mesh = UnityEngine.Object.Instantiate(mesh);
                var allFilters = UnityEngine.Object.FindObjectsOfType<MeshFilter>();
                foreach (var filter in allFilters)
                {
                    if (filter == meshFilter) continue;
                    var filterPath = AssetDatabase.GetAssetPath(filter.sharedMesh);
                    if (filterPath != savePath) continue;
                    otherFilters.Add(filter);
                }
            }
            AssetDatabase.CreateAsset(mesh, savePath);
            AssetDatabase.SaveAssets();
            Undo.RecordObject(meshFilter, UNDO_EDIT_PIVOT);
            meshFilter.sharedMesh = mesh;
            ApplyPivot(pivot);
            foreach (var filter in otherFilters)
            {
                Undo.RecordObject(filter, UNDO_EDIT_PIVOT);
                filter.mesh = null;
                filter.mesh = mesh;
                var target = filter.transform;
                EditColliders(target, pivot);
                EditNavMeshObject(target, pivot);
                EditPivotPositionAndRotation(target, pivot);
            }
        }

        private static void EditSprite(SpriteRenderer renderer,  Transform pivot)
        {
            var rect = renderer.sprite.rect;
            var pixelsPerUnit =  renderer.sprite.pixelsPerUnit;
            var min = renderer.transform.InverseTransformPoint(renderer.bounds.min);
            var pivot2D = new Vector2((pivot.localPosition.x - min.x) * pixelsPerUnit / rect.width, (pivot.localPosition.y - min.y) * pixelsPerUnit / rect.height);

            var path = AssetDatabase.GetAssetPath(renderer.sprite);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            Undo.RecordObject(textureImporter, UNDO_EDIT_PIVOT);
            var settings = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot2D;
            textureImporter.SetTextureSettings(settings);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var allRenderers = UnityEngine.Object.FindObjectsOfType<SpriteRenderer>();
            foreach(var r in allRenderers)
            {
                if (r == renderer) continue;
                if (r.sprite != renderer.sprite) continue;
                EditColliders(r.transform, pivot);
                EditPivotPositionAndRotation(r.transform, pivot);
            }
        }

        private static void EditRectTransform(RectTransform transform, Transform pivot)
        {
            var localPivot = transform.InverseTransformPoint(pivot.position);
            var rect = transform.rect;
            transform.pivot = new Vector2((localPivot.x - rect.min.x)/rect.width, (localPivot.y - rect.min.y)/rect.height);
        }

        private static void EditMesh(Mesh mesh, Transform pivot)
        {
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var tangents = mesh.tangents;

            if (pivot.localPosition != Vector3.zero)
            {
                for (int i = 0; i < vertices.Length; ++i)
                {
                    vertices[i] -= pivot.localPosition;
                }
            }

            if (pivot.localEulerAngles != Vector3.zero)
            {
                var invRot = Quaternion.Inverse(pivot.localRotation);
                for (int i = 0; i < vertices.Length; ++i)
                {
                    vertices[i] = invRot * vertices[i];
                    normals[i] = invRot * normals[i];
                    var tanDir = invRot * tangents[i];
                    tangents[i] = new Vector4(tanDir.x, tanDir.y, tanDir.z, tangents[i].w);
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.tangents = tangents;
            mesh.RecalculateBounds();
        }

        private static void EditColliders(Transform target, Transform pivot)
        {
            var meshFilter = pivot.parent.GetComponent<MeshFilter>();
            var meshColliders = target.GetComponents<MeshCollider>();
            foreach (var collider in meshColliders)
            {
                if (collider.sharedMesh == null) continue;
                Undo.RecordObject(collider, UNDO_EDIT_PIVOT);
                collider.sharedMesh = null;
                collider.sharedMesh = meshFilter.sharedMesh;
            }
            var boxColliders = target.GetComponents<BoxCollider>();
            foreach (var collider in boxColliders)
            {
                Undo.RecordObject(collider, UNDO_EDIT_PIVOT);
                collider.center -= pivot.localPosition;
            }
            var capsuleColliders = target.GetComponents<CapsuleCollider>();
            foreach (var collider in capsuleColliders)
            {
                Undo.RecordObject(collider, UNDO_EDIT_PIVOT);
                collider.center -= pivot.localPosition;
            }
            var sphereColliders = target.GetComponents<SphereCollider>();
            foreach (var collider in sphereColliders)
            {
                Undo.RecordObject(collider, UNDO_EDIT_PIVOT);
                collider.center -= pivot.localPosition;
            }
            var wheelColliders = target.GetComponents<WheelCollider>();
            foreach (var collider in wheelColliders)
            {
                Undo.RecordObject(collider, UNDO_EDIT_PIVOT);
                collider.center -= pivot.localPosition;
            }
            var colliders2D = target.GetComponents<Collider2D>();
            foreach (var collider in colliders2D)
            {
                Undo.RecordObject(collider, UNDO_EDIT_PIVOT);
                collider.offset -= (Vector2)pivot.localPosition;
            }
        }

        private static void EditPivotPositionAndRotation(Transform target, Transform pivot)
        {
            var children = target.GetComponentsInChildren<Transform>();
            var childrenPosAndRot = children.Select(child => (child, child.position, child.rotation)).ToArray();
            Undo.RecordObject(target, UNDO_EDIT_PIVOT);
            target.position += target.TransformVector(pivot.localPosition);
            target.eulerAngles += pivot.localEulerAngles;
            for (int i = 0; i < childrenPosAndRot.Length; ++i)
            {
                var child = childrenPosAndRot[i].child;
                if (child == target || child == pivot) continue;
                Undo.RecordObject(child, UNDO_EDIT_PIVOT);
                child.position = childrenPosAndRot[i].position;
                child.rotation = childrenPosAndRot[i].rotation;
            }
        }

        private static void EditNavMeshObject(Transform target, Transform pivot)
        {
            var obstacle = target.GetComponent<NavMeshObstacle>();
            if (obstacle == null) return;
            Undo.RecordObject(obstacle, UNDO_EDIT_PIVOT);
            obstacle.center -= pivot.localPosition;
        }

        public static void ApplyPivot(Transform pivot)
        {
            var target = pivot.parent;

            var meshFilter = target.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                Undo.RecordObject(meshFilter.sharedMesh, UNDO_EDIT_PIVOT);
                EditMesh(meshFilter.sharedMesh, pivot);
            }

            var spriteRenderer = target.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                EditSprite(spriteRenderer, pivot);
            }

            var rectTransform = target.GetComponent<RectTransform>();
            if(rectTransform != null)
            {
                EditRectTransform(rectTransform, pivot);
            }

            EditColliders(target, pivot);
            EditNavMeshObject(target, pivot);
            EditPivotPositionAndRotation(target, pivot);
        }

        public static GameObject CreateCenteredPivot(Transform target)
        {
            if (target == null || target.gameObject.scene.rootCount == 0) return null;
            var bounds = GetBoundsRecursive(target);
            var pivot = new GameObject("Pivot", typeof(Pivot));
            pivot.transform.parent = target.transform;
            pivot.transform.localRotation = Quaternion.identity;
            pivot.transform.localScale = Vector3.one;
            pivot.transform.position = bounds.center;
            return pivot;
        }

        public static void CenterPivot(Transform target, string savePath)
        {
            if (target == null || target.gameObject.scene.rootCount == 0) return;
            var pivot = CreateCenteredPivot(target);
            var meshFilter = target.GetComponent<MeshFilter>();
            SaveMesh(meshFilter, savePath, pivot.transform);
            UnityEngine.Object.DestroyImmediate(pivot);
            Selection.activeObject = target.gameObject;
        }

        public static void CenterPivot(Transform target)
        {
            if (target == null || target.gameObject.scene.rootCount == 0) return;
            var pivot = CreateCenteredPivot(target);
            ApplyPivot(pivot.transform);
            UnityEngine.Object.DestroyImmediate(pivot);
            Selection.activeObject = target.gameObject;
        }
        #endregion
        #region PLACE ON SURFACE

        public class PlaceOnSurfaceData
        {
            private Space _projectionDirectionSpace = Space.Self;
            private Vector3 _projectionDirection = Vector3.down;
            private bool _rotateToSurface = true;
            private Vector3 _objectOrientation = Vector3.down;
            private float _surfaceDistance = 0f;

            public bool rotateToSurface { get => _rotateToSurface; set => _rotateToSurface = value; }
            public Vector3 objectOrientation { get => _objectOrientation; set => _objectOrientation = value; }
            public float surfaceDistance { get => _surfaceDistance; set => _surfaceDistance = value; }
            public Vector3 projectionDirection { get => _projectionDirection; set => _projectionDirection = value; }
            public Space projectionDirectionSpace { get => _projectionDirectionSpace; set => _projectionDirectionSpace = value; }
        }

        private static (Vector3 vertex, Transform transform)[] GetDirectionVertices(Transform target, Vector3 worldProjDir)
        {
            var children = Array.FindAll(target.GetComponentsInChildren<MeshFilter>(), filter => filter != null && filter.sharedMesh != null).Select(filter => (filter.transform, filter.sharedMesh)).ToArray();
            var maxSqrDistance = float.MinValue;
            var bounds = GetBoundsRecursive(target);
            var vertices = new List<(Vector3 vertex, Transform transform)>() { (bounds.center, target) };
            foreach (var child in children)
            {
                foreach (var vertex in child.sharedMesh.vertices)
                {
                    var centerToVertex = child.transform.TransformPoint(vertex) - bounds.center;
                    var projection = Vector3.Project(centerToVertex, worldProjDir);
                    var sqrDistance = projection.sqrMagnitude * (projection.normalized != worldProjDir.normalized ? -1 : 1);
                    var vertexTrans = (vertex, child.transform);
                    if (sqrDistance > maxSqrDistance)
                    {
                        vertices.Clear();
                        maxSqrDistance = sqrDistance;
                        vertices.Add(vertexTrans);
                    }
                    else if (sqrDistance + 0.001 >= maxSqrDistance)
                    {
                        if (vertices.Exists(item => item.vertex == vertexTrans.vertex)) continue;
                        vertices.Add(vertexTrans);
                    }
                }
            }
            return vertices.ToArray();
        }

        private static void PlaceOnSurface(Transform target, PlaceOnSurfaceData data)
        {
            var worldProjDir = (data.projectionDirectionSpace == Space.World
                ? data.projectionDirection
                : target.TransformDirection(data.projectionDirection)).normalized;

            var originalPosition = target.position;
            var originalRotation = target.rotation;
            UnityEditor.Undo.RecordObject(target, "Place On Surface");
            if (data.rotateToSurface)
            {
                var worldOrientDir = target.TransformDirection(data.objectOrientation);
                var orientAngle = Vector3.Angle(worldOrientDir, worldProjDir);
                var cross = Vector3.Cross(worldOrientDir, worldProjDir);
                if (cross == Vector3.zero)
                {
                    cross = target.TransformDirection(data.objectOrientation.y != 0 ? Vector3.forward : data.objectOrientation.z != 0 ? Vector3.right : Vector3.up);
                    orientAngle = worldOrientDir == worldProjDir ? 0 : 180;
                }
                target.Rotate(cross, orientAngle);
            }

            var dirVert = GetDirectionVertices(target, worldProjDir);
            var minDistance = float.MaxValue;
            var closestVertexInfoList = new List<((Vector3 vertex, Transform transform), RaycastHit hitInfo)>();
            foreach (var vertexTransform in dirVert)
            {
                RaycastHit hitInfo;
                var rayOrigin = vertexTransform.transform.TransformPoint(vertexTransform.vertex);

                if (!Physics.Raycast(rayOrigin, worldProjDir, out hitInfo)) continue;
                if (hitInfo.distance < minDistance)
                {
                    minDistance = hitInfo.distance;
                    closestVertexInfoList.Clear();
                    closestVertexInfoList.Add((vertexTransform, hitInfo));
                }
                else if (hitInfo.distance - 0.001 <= minDistance)
                {
                    closestVertexInfoList.Add((vertexTransform, hitInfo));
                }
            }
            if (closestVertexInfoList.Count == 0)
            {
                target.SetPositionAndRotation(originalPosition, originalRotation);
                return;
            }
            var averageWorldVertex = Vector3.zero;
            var averageHitPoint = Vector3.zero;
            var averageNormal = Vector3.zero;
            foreach (var vertInfo in closestVertexInfoList)
            {
                averageWorldVertex += vertInfo.Item1.transform.TransformPoint(vertInfo.Item1.vertex);
                averageHitPoint += vertInfo.hitInfo.point;
                averageNormal += vertInfo.hitInfo.normal;
            }
            averageWorldVertex /= closestVertexInfoList.Count;
            var averageVertex = target.InverseTransformPoint(averageWorldVertex);
            averageHitPoint /= closestVertexInfoList.Count;
            averageNormal /= closestVertexInfoList.Count;

            if (data.rotateToSurface)
            {
                var worldOrientDir = target.TransformDirection(-data.objectOrientation);
                var angle = Vector3.Angle(worldOrientDir, averageNormal);
                var cross = Vector3.Cross(worldOrientDir, averageNormal);
                if (cross != Vector3.zero)
                {
                    target.RotateAround(target.TransformPoint(averageVertex), cross, angle);
                }
            }

            target.position = averageHitPoint - target.TransformVector(averageVertex) - worldProjDir * data.surfaceDistance;
        }

        public static void PlaceOnSurface(GameObject[] selection, PlaceOnSurfaceData data)
        {
            var ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
            var layerDictionary = new Dictionary<GameObject, int>();
            foreach (var obj in selection)
            {
                var children = obj.transform.GetComponentsInChildren<Transform>(true);
                foreach (var child in children)
                {
                    layerDictionary.Add(child.gameObject, child.gameObject.layer);
                    child.gameObject.layer = ignoreRaycast;
                }
            }

            foreach (var obj in selection)
            {
                PlaceOnSurface(obj.transform, data);
            }

            foreach (var item in layerDictionary)
            {
                item.Key.layer = item.Value;
            }
        }
        #endregion
        #region UTILS
        private static int CompareHierarchyIndex(GameObject obj1, GameObject obj2)
        {
            var idx1 = GetHierarchyIndex(obj1);
            var idx2 = GetHierarchyIndex(obj2);
            var depth = 0;
            do
            {
                if (idx1.Length <= depth)
                {
                    return -1;
                }
                if (idx2.Length <= depth)
                {
                    return 1;
                }
                var result = idx1[depth].CompareTo(idx2[depth]);
                if (result != 0)
                {
                    return result;
                }
                ++depth;
            }
            while (true);
        }

        private static List<GameObject> SortByHierarchy(List<GameObject> selection)
        {
            selection.Sort((obj1, obj2) => CompareHierarchyIndex(obj1, obj2));
            return selection;
        }

        private static void LookAtCenter(Transform transform, Vector3 center, Vector3 axis,  Vector3 orientation, Vector3 parallelAxis)
        {
            transform.rotation = Quaternion.FromToRotation(parallelAxis, axis);
            var worldOrientation = transform.TransformDirection(orientation);
            var objToCenter = center - transform.position;
            var angle = Vector3.Angle(worldOrientation, objToCenter);
            var cross = Vector3.Cross(worldOrientation, objToCenter);
            if(cross == Vector3.zero)
            {
                cross = axis;
            }
            transform.Rotate(cross, angle, Space.World);
        }

        #endregion
    }
}
