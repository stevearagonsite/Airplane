using System;
using Consts;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerWinner : MonoBehaviour, IObservableEventDefeated, IObservableEventWinner {
    public static TriggerWinner Instance;
    public event Action OnEventWinner = delegate { };
    public event Action OnEventDefeated = delegate { };

    private void Awake() {
        if (Instance != null) {
            Destroy(this);
            Instance = this;
        }
        else {
            Instance = this;
        }
    }

    private void OnTriggerEnter(Collider c) {
        var layer = c.gameObject.layer;
        if (layer != Layers.PLAYERS_NUM_LAYER) return;
        var entityPlayer = c.gameObject.GetComponent<EntityPlayer>();

        if (!entityPlayer) return;
        
        if (entityPlayer.isMime) {
            OnEventWinner();
            return;
        }
        
        OnEventDefeated();
    }
    
    private void OnDrawGizmos() {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }

    public void SubscribeEventDefeated(IObserverEventDefeated observer) {
        OnEventDefeated += observer.EventDefeated;
    }

    public void UnSubscribeEventDefeated(IObserverEventDefeated observer) {
        OnEventDefeated -= observer.EventDefeated;
    }

    public void SubscribeEventWinner(IObserverEventWinner observer) {
        OnEventWinner += observer.EventWinner;
    }

    public void UnSubscribeEventWinner(IObserverEventWinner observer) {
        OnEventWinner -= observer.EventWinner;
    }
}