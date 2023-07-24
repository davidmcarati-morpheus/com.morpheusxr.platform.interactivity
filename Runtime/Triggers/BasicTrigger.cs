using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BasicTrigger : TriggerBase
{
    [SerializeField] UnityEvent<GameObject> onTriggerEntered;
    [SerializeField] UnityEvent<GameObject> onTriggerExited;
    [SerializeField] UnityEvent onHasObjectsInTrigger;
    [SerializeField] UnityEvent onNoObjectsInTrigger;

    public UnityEvent<GameObject> OnTriggerEntered => onTriggerEntered;
    public UnityEvent<GameObject> OnTriggerExited => onTriggerExited;
    public UnityEvent OnHasObjectsInTrigger => onHasObjectsInTrigger;
    public UnityEvent OnNoObjectsInTrigger => onNoObjectsInTrigger;

    private void Start()
    {
        if (HasObjectsInTrigger)
            onHasObjectsInTrigger.Invoke();
        else
            onNoObjectsInTrigger.Invoke();
    }

    protected override void TriggerEnterCallback(GameObject go)
    {
        bool hadObjects = HasObjectsInTrigger;

        onTriggerEntered.Invoke(go);

        if (!hadObjects && HasObjectsInTrigger)
            onHasObjectsInTrigger.Invoke();
    }

    protected override void TriggerExitCallback(GameObject go)
    {
        bool hadObjects = HasObjectsInTrigger;

        onTriggerExited.Invoke(go);

        if (hadObjects && !HasObjectsInTrigger)
            onNoObjectsInTrigger.Invoke();
    }
}