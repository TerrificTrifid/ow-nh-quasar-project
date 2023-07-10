using NewHorizons;
using UnityEngine;

namespace QuasarProject;

/// <summary>
/// parent is scaled to fake increase of far plane
/// should be draw as SkyboxRenderer so it renders behind everything else (does not do lighting properly)
///
/// the idea is to be at the same position as the camera, by scaled by some amount to appear larger
///
/// oh yeah this completely breaks with the probe LOL
/// 
/// first parent is scaled
/// second parent is origin
/// </summary>
[UsedInUnityProject]
public class MovingSkyboxRenderer : MonoBehaviour
{
    private void Update()
    {
        var origin = transform.parent.parent;
        var playerPos = origin.InverseTransformPoint(Locator.GetPlayerTransform().position);
        transform.position = playerPos;
    }
}
