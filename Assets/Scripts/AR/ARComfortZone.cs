using UnityEngine;

public class ARComfortZone : MonoBehaviour
{
    private ARSanityManager manager;

    void Start()
    {
        manager = FindObjectOfType<ARSanityManager>();
        if (manager != null)
        {
            manager.RegisterZone(this);
        }
    }

    void OnDestroy()
    {
        if (manager != null)
        {
            manager.UnregisterZone(this);
        }
    }
}
