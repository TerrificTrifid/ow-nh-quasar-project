using NewHorizons;
using NewHorizons.Utility.OWML;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace QuasarProject;

[UsedInUnityProject]
public class DiskGuardPassController : MonoBehaviour
{
	public Transform[] Points;
	public AnimationCurve DistanceCurve;

	private DiskGuardPassEffect[] _effects;
	// for disabling shadows behind sun
	private Renderer[] _shadowRenderers;

	// do in start to wait for other objects to be built
	private void Start()
	{
		// delay to get the player one
		enabled = false;
		Delay.FireOnNextUpdate(() =>
		{
			enabled = true;
			_effects = FindObjectsOfType<DiskGuardPassEffect>();
		});

		_shadowRenderers = Points
			.SelectMany(x => x.GetComponentsInChildren<Renderer>(true))
			.Where(x => x.shadowCastingMode == ShadowCastingMode.On)
			.ToArray();
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

		var starPos = transform.root.position;
		var toPlayer = Locator.GetPlayerTransform().position - starPos;
		foreach (var renderer in _shadowRenderers)
		{
			var behind = Vector3.Dot(renderer.transform.position - starPos, toPlayer) < 0;
			renderer.shadowCastingMode = behind ? ShadowCastingMode.Off : ShadowCastingMode.On;
		}
	}
}
