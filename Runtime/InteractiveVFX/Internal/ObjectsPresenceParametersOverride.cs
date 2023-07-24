using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsPresenceParametersOverride : MonoBehaviour
{
    public float m_RenderSqrDistance = 10000f;
    [Header("Filter Params")]
    public LayerMask m_IgnoreMask;
    [HideInInspector] public bool m_FilterDisabledObjects;
    [HideInInspector] public bool m_FilterFarObjects;
    [HideInInspector] public float m_FilterSqrDistance = 10f;
}
