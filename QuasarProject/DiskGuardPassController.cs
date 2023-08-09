using NewHorizons;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace QuasarProject;

[UsedInUnityProject]
public class DiskGuardPassController : MonoBehaviour
{
	public Transform[] Points;
	public AnimationCurve DistanceCurve;

	private DiskGuardPassEffect[] _effects;

	// do in start to wait for other objects to be built
	private void Start()
	{
		enabled = false;
		// delay to get the player one
		Delay.FireOnNextUpdate(() =>
		{
			enabled = true;
			_effects = FindObjectsOfType<DiskGuardPassEffect>();
		});
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
