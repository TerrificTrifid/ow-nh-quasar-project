using NewHorizons;
using UnityEngine;

namespace QuasarProject;

[UsedInUnityProject]
public class MovingSkyboxRenderer : MonoBehaviour
{
    private void Update()
    {
        var scaled = transform.parent;
        var origin = scaled.parent;
        
        var cameraPos = origin.InverseTransformPoint(Locator.GetActiveCamera().transform.position);
        scaled.localPosition = cameraPos;
        transform.localPosition = -cameraPos;
    }
}
