using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour {

    public Waypoint next;

    [HideInInspector]
    public Waypoint last;

    public bool patrolWaypoint;

	void Start ()
    {
        if(next != null)
            next.last = this;
	}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

}
