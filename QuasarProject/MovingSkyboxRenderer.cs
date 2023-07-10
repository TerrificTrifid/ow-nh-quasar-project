using NewHorizons;
using UnityEngine;

namespace QuasarProject;

[UsedInUnityProject]
public class MovingSkyboxRenderer : MonoBehaviour
{
    private Transform _origin;

    private void Start()
    {
        _origin = QuasarProject.Instance.NewHorizons.GetPlanet("Ring").transform;
    }

    private void Update()
    {
        // parent is scaled to fake increase of far plane
        // should be draw as SkyboxRenderer so it renders behind everything else (does not do lighting properly)
        var playerPos = _origin.InverseTransformPoint(Locator.GetPlayerTransform().position);
        transform.position = playerPos;
    }
}
