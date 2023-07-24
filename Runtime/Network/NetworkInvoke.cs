using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Mirror;

public class NetworkInvoke : NetworkBehaviour
{
    [SerializeField] EventContainer[] events;

    Dictionary<string, EventContainer> containers;

    private void Reset()
    {
        events = new EventContainer[] { new EventContainer() };
    }

    private void Awake()
    {
        containers = new Dictionary<string, EventContainer>();
        foreach (EventContainer c in events)
            containers[c.Key] = c;
    }

    [Command(requiresAuthority = false)]
    public void InvokeEvent(string key)
    {
        if (containers != null && containers.TryGetValue(key, out EventContainer c))
            c.OnServerInvoke.Invoke();

        CallClients(key);
    }

    [ClientRpc(includeOwner = true)]
    void CallClients(string key)
    {
        if (containers != null && containers.TryGetValue(key, out EventContainer c))
            c.OnClientsInvoke.Invoke();
    }

    [System.Serializable]
    public class EventContainer
    {
        public string Key = "run";
        public UnityEvent OnServerInvoke;
        public UnityEvent OnClientsInvoke;
    }
}
