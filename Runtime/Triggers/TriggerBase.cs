using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class TriggerBase : MonoBehaviour
{
    [SerializeField] KeywordsFilter keywordsFilter = new KeywordsFilter();

    private readonly HashSet<GameObject> objectsInTrigger = new();
    public IReadOnlyCollection<GameObject> ObjectsInTrigger { get { return objectsInTrigger; } }

    public bool HasObjectsInTrigger => objectsInTrigger.Count > 0;

    protected abstract void TriggerEnterCallback(GameObject go);

    protected abstract void TriggerExitCallback(GameObject go);

    private void OnTriggerEnter(Collider other)
    {
        if (!keywordsFilter.Check(other.gameObject))
            return;

        objectsInTrigger.Add(other.gameObject);

        if (!other.gameObject.TryGetComponent(out KeywordsDestroyOrDisabledNotification destroyNotification))
            destroyNotification = other.gameObject.AddComponent<KeywordsDestroyOrDisabledNotification>();

        destroyNotification.OnDestroyedOrDisabled.AddListener(OnObjectDisabledOrDestroyed);

        TriggerEnterCallback(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!keywordsFilter.Check(other.gameObject))
            return;

        if (other.gameObject.TryGetComponent(out KeywordsDestroyOrDisabledNotification destroyNotification))
            destroyNotification.OnDestroyedOrDisabled.RemoveListener(OnObjectDisabledOrDestroyed);

        objectsInTrigger.Remove(other.gameObject);

        TriggerExitCallback(other.gameObject);
    }

    void OnObjectDisabledOrDestroyed(KeywordsDestroyOrDisabledNotification keywordsSource)
    {
        keywordsSource.OnDestroyedOrDisabled.RemoveListener(OnObjectDisabledOrDestroyed);

        objectsInTrigger.Remove(keywordsSource.gameObject);
        TriggerExitCallback(keywordsSource.gameObject);
    }

    private void OnDisable()
    {
        OnZoneDisabledOrDestroyed();
    }

    private void OnDestroy()
    {
        OnZoneDisabledOrDestroyed();
    }

    void OnZoneDisabledOrDestroyed()
    {
        GameObject[] objectsArray = new GameObject[objectsInTrigger.Count];
        objectsInTrigger.CopyTo(objectsArray);

        foreach (var item in objectsArray)
        {
            if (item.TryGetComponent(out KeywordsDestroyOrDisabledNotification destroyNotification))
                destroyNotification.OnDestroyedOrDisabled.RemoveListener(OnObjectDisabledOrDestroyed);

            objectsInTrigger.Remove(item);

            TriggerExitCallback(item);
        }

        objectsInTrigger.Clear();
    }
}

class KeywordsDestroyOrDisabledNotification : MonoBehaviour
{
    public UnityEvent<KeywordsDestroyOrDisabledNotification> OnDestroyedOrDisabled = new ();

    private void OnDisable()
    {
        OnDestroyedOrDisabled.Invoke(this);
    }

    private void OnDestroy()
    {
        OnDestroyedOrDisabled.Invoke(this);
    }
}
