using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTrigger : TriggerBase
{
    [Tooltip("If enabled, the objects will turn off when the trigger enters.")]
    [SerializeField] bool invert;
    [SerializeField] GameObject[] objectsToSwitch;

    private void Start()
    {
        ApplyVisibility();
    }

    protected override void TriggerEnterCallback(GameObject go)
    {
        ApplyVisibility();
    }

    protected override void TriggerExitCallback(GameObject go)
    {
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        foreach (var item in objectsToSwitch)
        {
            if (item != null)
                item.SetActive(invert ? !HasObjectsInTrigger : HasObjectsInTrigger);
        }
    }
}
