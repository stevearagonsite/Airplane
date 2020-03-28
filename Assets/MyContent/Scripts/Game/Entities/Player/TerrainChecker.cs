using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Consts;
public class TerrainChecker : MonoBehaviour
{
    public static TerrainChecker Instance;
    public bool isTerrein { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            Instance = this;
        }
        else
        {
            Instance = this;
        }
    }

    private void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.layer == Layers.TERRAIN_NUM_LAYER)
        {
            isTerrein = true;
        }
    }
    
    private void OnTriggerExit(Collider c)
    {
        if (c.gameObject.layer == Layers.TERRAIN_NUM_LAYER)
        {
            isTerrein = false;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0, 0, 1, 0.5f);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }
}
