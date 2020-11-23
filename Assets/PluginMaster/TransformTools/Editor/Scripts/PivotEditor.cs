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
using UnityEditor;
using UnityEngine;

namespace PluginMaster
{
    [CustomEditor(typeof(Pivot))]
    public class PivotEditor : Editor
    {
        private void OnSceneGUI()
        {
            if (target == null) return;
            Handles.color = Color.yellow;
            Handles.matrix = Matrix4x4.identity;
            var transform = (target as Pivot).transform;
            Handles.SphereHandleCap(0, transform.position, Quaternion.identity, HandleUtility.GetHandleSize(transform.position) * 0.3f, EventType.Repaint);
           
            var e = Event.current;
            if (e != null && e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                Selection.activeObject = transform.parent.gameObject;
                DestroyImmediate(transform.gameObject);
                e.Use();
            }
        }
    }
}
