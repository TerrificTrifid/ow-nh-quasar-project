using NewHorizons;
using UnityEngine;
using UnityEngine.Rendering;

namespace QuasarProject;

/// <summary>
/// disable shadows when renderer is behind sun
/// </summary>
// copied from PerSectorShadowCastingState
[UsedInUnityProject]
[RequireComponent(typeof(Renderer))]
public class ShadowDisableHack : MonoBehaviour
{
	public Transform Sun;
	private Renderer _renderer;
	private bool _overridden;
	private ShadowCastingMode _prevShadowCastingMode;

	private void Awake()
	{
		_renderer = GetComponent<Renderer>();
	}

	private void Update()
	{
		var behind = Vector3.Dot(transform.position - Sun.position, Locator.GetPlayerTransform().position - Sun.position) < 0;
		if (behind)
		{
			if (!_overridden)
			{
				_prevShadowCastingMode = _renderer.shadowCastingMode;
				_renderer.shadowCastingMode = ShadowCastingMode.Off;
				_overridden = true;
			}
		}
		else
		{
			if (_overridden)
			{
				_renderer.shadowCastingMode = _prevShadowCastingMode;
				_overridden = false;
			}
		}
	}
}
