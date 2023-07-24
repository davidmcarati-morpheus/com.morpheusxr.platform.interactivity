using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectsPresenceSystem : MonoBehaviour
{
    public HashSet<ObjectsPresenceZone> PresenceZones = new();

    static ObjectsPresenceSystem m_Instance;
    public static ObjectsPresenceSystem Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new GameObject("ObjectsPresenceSystem").AddComponent<ObjectsPresenceSystem>();
            }

            return m_Instance;
        }
    }

    private void Start()
    {
        var parametersOverride = FindObjectOfType<ObjectsPresenceParametersOverride>();

        foreach (var zone in PresenceZones)
        {
            if (zone == null)
                continue;

            InitZone(zone, parametersOverride);
        }

        SceneManager.sceneUnloaded += SceneUnloadedCallback;
    }

    private void SceneUnloadedCallback(Scene arg0)
    {
        SceneManager.sceneUnloaded -= SceneUnloadedCallback;
        m_Instance = null;
        Destroy(gameObject);
    }

    private void InitZone(ObjectsPresenceZone zone, ObjectsPresenceParametersOverride parametersOverride)
    {
        if (zone.AffectedVfx == null)
            return;

        if (!zone.TryGetComponent(out BoxCollider boxCollider))
        {
            boxCollider = zone.gameObject.AddComponent<BoxCollider>();
        }

        boxCollider.isTrigger = true;
        boxCollider.center = zone.TargetBounds.center;
        boxCollider.size = zone.TargetBounds.size;

        if (zone.TryGetComponent(out ObjectsPresenceMapper mapper))
        {
            Destroy(mapper);
        }

        zone.gameObject.AddComponent<ObjectsPresenceMapper>().OverrideParameters(zone, parametersOverride);
    }
}
