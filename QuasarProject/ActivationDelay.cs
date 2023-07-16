using NewHorizons;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace QuasarProject;

// bad
[UsedInUnityProject]
public class ActivationDelay : MonoBehaviour
{
    public OWTriggerVolume Volume;
    public GameObject Object;
    public int Seconds;

    public void Awake()
    {
        Volume.OnEntry += OnEntry;
    }

    public void OnDestroy()
    {
        Volume.OnEntry -= OnEntry;
    }

    private void OnEntry(GameObject hitobj)
    {
        if (hitobj.GetAttachedOWRigidbody().CompareTag("Player"))
            Delay.FireInNUpdates(() => Object.SetActive(true), Seconds * 60);
    }
}
