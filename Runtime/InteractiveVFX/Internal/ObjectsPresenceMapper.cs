using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class ObjectsPresenceMapper : MonoBehaviour
{
    private const float k_TriggerSizeApplier = 8f;

    [SerializeField] private Transform m_Player;
    [SerializeField] private float m_RenderSqrDistance = 10000f;
    [SerializeField] private float m_ObjectsRadius = 0f;
    [Header("Map Params")]
    [SerializeField] private float m_PresenceRadius = 1f;
    [SerializeField] private Vector2Int m_MapResolution = new(16, 16);
    [Space]
    [Header("Filer Params")]
    [SerializeField] private LayerMask m_IgnoreMask;
    [HideInInspector] [SerializeField] private bool m_FilterDisabledObjects;
    [HideInInspector] [SerializeField] private bool m_FilterFarObjects;
    [HideInInspector] [SerializeField] private float m_FilterSqrDistance = 10f;
    private BoxCollider m_TriggerBox;
    private Texture2D m_PresenceMap;
    private Color[] m_PresenceData;
    private bool m_NeedToClearData = true;

    private float m_InversePresenceRadius;
    private Vector3 m_UStep;
    private Vector3 m_VStep;
    private Vector3 m_StartPoint;
    private Matrix4x4 m_worldToUVMatrix;

    private Vector3[] m_CachedPositions = new Vector3[8];
    private Plane m_MapPlane;
    private ObjectsPresenceZone m_presenceZone;

    private readonly HashSet<Transform> m_TargetTransforms = new();

    public Texture2D PresenceMap => m_PresenceMap;

    const string PresenceMapParamName = "PresenceMap";
    const string WorldToPresenceUVMatrixParamName = "WorldToUVMatrix";

    private void Awake()
    {
        CalculateParameters();
    }

    void CalculateParameters()
    {
        m_TriggerBox = GetComponent<BoxCollider>();

        if (m_PresenceMap != null)
        {
            Destroy(m_PresenceMap);
            m_PresenceMap = null;
        }

        m_PresenceMap = new Texture2D(m_MapResolution.x, m_MapResolution.y, TextureFormat.RGBAFloat, false);
        m_PresenceMap.wrapMode = TextureWrapMode.Clamp;

        m_PresenceData = new Color[m_MapResolution.x * m_MapResolution.y];

        m_InversePresenceRadius = 1f / m_PresenceRadius;

        Vector3 boxExtents = m_TriggerBox.size * 0.5f;

        m_StartPoint = transform.TransformPoint(m_TriggerBox.center - boxExtents);
        m_MapPlane = new Plane(transform.up, m_StartPoint);

        m_UStep = transform.TransformVector(Vector3.right * m_TriggerBox.size.x / (m_MapResolution.x - 1));
        m_VStep = transform.TransformVector(Vector3.forward * m_TriggerBox.size.z / (m_MapResolution.y - 1));

        m_worldToUVMatrix = (transform.localToWorldMatrix *
            Matrix4x4.TRS(
                m_TriggerBox.center - boxExtents,
                Quaternion.identity,
                m_TriggerBox.size
                )).inverse;

        if (TryGetComponent(out ObjectsPresenceZone presenceZone))
        {
            foreach (var vfx in presenceZone.AffectedVfx)
            {
                if (vfx != null)
                {
                    vfx.SetTexture(PresenceMapParamName, m_PresenceMap);
                    vfx.SetMatrix4x4(WorldToPresenceUVMatrixParamName, m_worldToUVMatrix);
                }
            }
        }

        m_TriggerBox.size += (m_PresenceRadius + m_ObjectsRadius) * 2.0f * Vector3.one;
    }

    public void OverrideParameters(ObjectsPresenceZone objectsPresenceZone, ObjectsPresenceParametersOverride parametersOverride = null)
    {
        if (parametersOverride != null)
        {
            m_RenderSqrDistance = parametersOverride.m_RenderSqrDistance;
            m_IgnoreMask = parametersOverride.m_IgnoreMask;
            m_FilterDisabledObjects = parametersOverride.m_FilterDisabledObjects;
            m_FilterFarObjects = parametersOverride.m_FilterFarObjects;
            m_FilterSqrDistance = parametersOverride.m_FilterSqrDistance;
        }

        if (objectsPresenceZone != null)
        {
            m_ObjectsRadius = objectsPresenceZone.ObjectsRadius;
            m_PresenceRadius = objectsPresenceZone.PresenceRadius;
            m_MapResolution = objectsPresenceZone.MapResolution;

            m_presenceZone = objectsPresenceZone;
        }

        CalculateParameters();
    }

    private void Update()
    {
        if (m_TargetTransforms.Count > 0)
        {
            UpdatePresenceData();
            SetCurrentPresenceData();
            m_NeedToClearData = true;
        }
        else if (m_NeedToClearData)
        {
            ClearPresenceData();
            m_NeedToClearData = false;
        }
    }

    private void UpdatePresenceData()
    {
        if (m_Player != null && (m_Player.position - transform.position).sqrMagnitude > m_RenderSqrDistance) return;

        if (m_CachedPositions.Length < m_TargetTransforms.Count)
            m_CachedPositions = new Vector3[m_TargetTransforms.Count];

        var positionsCount = 0;
        foreach (var target in m_TargetTransforms)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                m_TargetTransforms.Remove(target);
                break;
            }

            if (IsFiltering(target))
            {
                continue;
            }

            m_CachedPositions[positionsCount] = target.position;
            positionsCount++;
        }

        for (var y = 0; y < m_MapResolution.y; y++)
        {
            for (var x = 0; x < m_MapResolution.x; x++)
            {
                var position = m_StartPoint + m_UStep * x + m_VStep * y;

                var minSqrDelta = float.MaxValue;
                int closestIndex = 0;

                for (int p = 0; p < positionsCount; p++)
                {
                    var projectedObjectPosition = m_MapPlane.ClosestPointOnPlane(m_CachedPositions[p]);

                    var sqrDelta = (projectedObjectPosition - position).sqrMagnitude;

                    if (sqrDelta < minSqrDelta)
                    {
                        minSqrDelta = sqrDelta;
                        closestIndex = p;
                    }
                }

                var maxPresencePosition = m_CachedPositions[closestIndex];
                var maxPresence = Mathf.Clamp01(1 - m_InversePresenceRadius * (Mathf.Sqrt(minSqrDelta) - m_ObjectsRadius));

                var index = x + y * m_MapResolution.x;
                m_PresenceData[index] = new Color(maxPresencePosition.x, maxPresencePosition.y, maxPresencePosition.z, maxPresence);
            }
        }
    }

    private bool IsFiltering(Transform target)
    {
        return m_FilterDisabledObjects && !target.gameObject.activeInHierarchy
                || m_FilterFarObjects && (transform.position - target.position).sqrMagnitude > m_FilterSqrDistance;
    }

    private void ClearPresenceData()
    {
        System.Array.Clear(m_PresenceData, 0, m_PresenceData.Length);
        SetCurrentPresenceData();
    }

    private void SetCurrentPresenceData()
    {
        m_PresenceMap.SetPixelData(m_PresenceData, 0);
        m_PresenceMap.Apply(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((m_IgnoreMask.value & (1 << other.gameObject.layer)) > 0) return;

        if (m_presenceZone == null || !m_presenceZone.KeywordsFilter.Check(other.gameObject))
            return;

        m_TargetTransforms.Add(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if ((m_IgnoreMask.value & (1 << other.gameObject.layer)) > 0) return;

        if (m_presenceZone == null || !m_presenceZone.KeywordsFilter.Check(other.gameObject))
            return;

        m_TargetTransforms.Remove(other.transform);
    }
}