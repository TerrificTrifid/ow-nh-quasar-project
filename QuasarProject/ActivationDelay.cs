using NewHorizons;
using System.Collections;
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
            StartCoroutine(Go());
    }

    private IEnumerator Go()
    {
        // unscaled cuz music plays while paused
        yield return new WaitForSecondsRealtime(Seconds);

        Object.SetActive(true);
    }
}
