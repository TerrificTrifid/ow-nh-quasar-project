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
        
        var playerPos = origin.InverseTransformPoint(Locator.GetPlayerTransform().position);
        scaled.localPosition = playerPos;
        transform.localPosition = -playerPos;
    }
}
