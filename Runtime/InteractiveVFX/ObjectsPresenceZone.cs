using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObjectsPresenceZone : MonoBehaviour
{
    [Tooltip("Keywords to filter incoming objects (can be empty)")]
    [SerializeField] KeywordsFilter m_KeywordsFilter = new KeywordsFilter();
    [Tooltip("Size and center of the interaction zone")]
    [SerializeField] private Bounds m_bounds = new Bounds(Vector3.zero, Vector3.one * 2);
    [Tooltip("Objects detection radius")]
    [SerializeField] private float m_ObjectsRadius = 0f;
    [Tooltip("The radius of influence of the objects on the presence map")]
    [SerializeField] private float m_PresenceRadius = 1f;
    [Tooltip("Resolution of the presence map")]
    [SerializeField] private Vector2Int m_MapResolution = new(16, 16);
    [Tooltip("Vfx inside this zone")]
    [SerializeField] private VisualEffect[] affectedVfx;

    public float ObjectsRadius => m_ObjectsRadius;
    public float PresenceRadius => m_PresenceRadius;
    public Vector2Int MapResolution => m_MapResolution;
    public KeywordsFilter KeywordsFilter => m_KeywordsFilter;

    public IReadOnlyCollection<VisualEffect> AffectedVfx => affectedVfx;

    public Bounds TargetBounds => m_bounds;

    private void Reset()
    {
        affectedVfx = GetComponentsInChildren<VisualEffect>();
    }

    void OnValidate()
    {
        m_bounds.size = new Vector3(Mathf.Clamp(m_bounds.size.x, 0, float.MaxValue), Mathf.Clamp(m_bounds.size.y, 0, float.MaxValue), Mathf.Clamp(m_bounds.size.z, 0, float.MaxValue));
        m_ObjectsRadius = Mathf.Clamp(m_ObjectsRadius, 0, float.MaxValue);
        m_PresenceRadius = Mathf.Clamp(m_PresenceRadius, m_ObjectsRadius, float.MaxValue);
        m_MapResolution = new Vector2Int(Mathf.Clamp(m_MapResolution.x, 0, int.MaxValue), Mathf.Clamp(m_MapResolution.y, 0, int.MaxValue));
    }

    private void Awake()
    {
        ObjectsPresenceSystem.Instance.PresenceZones.Add(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(m_bounds.center, m_bounds.size);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ObjectsPresenceZone))]
public class TestInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var affectedVfxSerialized = serializedObject.FindProperty("affectedVfx");

        for (int i = 0; i < affectedVfxSerialized.arraySize; i++)
        {
            if ((VisualEffect)affectedVfxSerialized.GetArrayElementAtIndex(i).objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox($"Affected Vfx element null on {i} index!", MessageType.Error);
            }
            else
            {
                var vfx = (VisualEffect)affectedVfxSerialized.GetArrayElementAtIndex(i).objectReferenceValue;

                if (!vfx.HasTexture("PresenceMap"))
                    EditorGUILayout.HelpBox($"Affected Vfx element has no PresenceMap field on {i} index!", MessageType.Error);
                if (!vfx.HasMatrix4x4("WorldToUVMatrix"))
                    EditorGUILayout.HelpBox($"Affected Vfx element has no WorldToUVMatrix field on {i} index!", MessageType.Error);
            }
        }
    }
}
#endif
