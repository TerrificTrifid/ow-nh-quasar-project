using NewHorizons;
using UnityEngine;

namespace QuasarProject;

[UsedInUnityProject]
public class DiskGuardPassController : MonoBehaviour
{
	public Transform[] Points;
	public AnimationCurve DistanceCurve;

	private DiskGuardLight[] _lights;

	private void Awake()
	{
		_lights = FindObjectsOfType<DiskGuardLight>();
	}

	private void Update()
	{
		foreach (var light in _lights)
		{
			var minDistance = float.PositiveInfinity;

			foreach (var point in Points)
			{
				var distance = Vector3.Distance(light.transform.position, point.position);
				if (distance < minDistance) minDistance = distance;
			}

			var t = DistanceCurve.Evaluate(minDistance);
			light.UpdateValues(t);
		}
	}
}
