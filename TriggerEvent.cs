using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class TriggerEvent : MonoBehaviour
{
    [SerializeField]
    UnityEvent onTriggerEnter;
    [SerializeField]
    UnityEvent onTriggerExit;
    [SerializeField]
    UnityEvent onTriggerStay;
    void OnTriggerEnter(Collider col)
    {
        onTriggerEnter.Invoke();
    }
    void OnTriggerStay(Collider col)
    {
        onTriggerStay.Invoke();
    }
    void OnTriggerExit(Collider col)
    {
        onTriggerExit.Invoke();
    }
}
