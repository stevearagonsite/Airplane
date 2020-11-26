using Consts;
using UnityEngine;

public class CollisionChecker : MonoBehaviour {
    public bool IsColliding { get; private set; }
    
    private void OnTriggerEnter(Collider c) {
        if (c.gameObject.layer == Layers.TERRAIN_NUM_LAYER) {
            IsColliding = true;
        }
    }

    private void OnTriggerExit(Collider c) {
        if (c.gameObject.layer == Layers.TERRAIN_NUM_LAYER) {
            IsColliding = false;
        }
    }

    void OnDrawGizmos() {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }
}