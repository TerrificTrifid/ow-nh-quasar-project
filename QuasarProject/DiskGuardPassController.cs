using NewHorizons;
using UnityEngine;

namespace QuasarProject;

[UsedInUnityProject]
public class DiskGuardPassController : MonoBehaviour
{
	public Transform[] Points;
	public AnimationCurve DistanceCurve;

	private DiskGuardPassEffect[] _effects;

	private void Awake()
	{
		_effects = FindObjectsOfType<DiskGuardPassEffect>();
	}

	private void Update()
	{
		foreach (var effect in _effects)
		{
			var minDistance = float.PositiveInfinity;

			foreach (var point in Points)
			{
				var distance = Vector3.Distance(effect.transform.position, point.position);
				if (distance < minDistance) minDistance = distance;
			}

			var t = DistanceCurve.Evaluate(minDistance);
			effect.UpdateValues(t);
		}
	}
}
