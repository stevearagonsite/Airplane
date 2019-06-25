using System;
using UnityEngine;
using UnityEngine.EventSystems;


public class ManagerEventSystem : MonoBehaviour
{
    void Start()
    {
        VerifyAndSpawn();
    }

    /// <summary>
    /// Prevent error that event system spawn in more of a scene.
    /// </summary>
    private void VerifyAndSpawn()
    {
        EventSystem sceneEventSystem = FindObjectOfType<EventSystem>();
        if (sceneEventSystem == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");

            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
}