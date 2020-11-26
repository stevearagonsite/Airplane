using Consts;
using UnityEngine;

public class TerrainChecker : MonoBehaviour {
    public bool isTerrain { get; private set; }


    private void OnTriggerEnter(Collider c) {
        if (c.gameObject.layer == Layers.TERRAIN_NUM_LAYER) {
            isTerrain = true;
        }
    }

    private void OnTriggerExit(Collider c) {
        if (c.gameObject.layer == Layers.TERRAIN_NUM_LAYER) {
            isTerrain = false;
        }
    }

    void OnDrawGizmos() {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0, 0, 1, 0.5f);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }
}