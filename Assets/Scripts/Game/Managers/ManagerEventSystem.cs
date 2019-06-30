using System;
using UnityEngine;
using UnityEngine.EventSystems;


public class ManagerEventSystem : MonoBehaviour
{
    private void Start()
    {
        VerifyAndSpawn();
    }

    /// <summary>
    /// Prevent error that event system spawn in more of a scene.
    /// </summary>
    private static void VerifyAndSpawn()
    {
        if (FindObjectOfType<EventSystem>()) return;
        
        var eventSystem = new GameObject("EventSystem");

        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }
}