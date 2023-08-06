using NewHorizons;
using UnityEngine;

namespace QuasarProject;

[UsedInUnityProject]
public class DiskGuardPassEffect : MonoBehaviour
{
	private Color _defaultColor;
	private float _defaultIntensity;
	public Color PassColor;
	public float PassIntensity;

	private Light _light;
	private Renderer _renderer;
	private bool _isDefault, _isAmbientLight;

	private void Awake()
	{
		_light = GetComponent<Light>();
		_renderer = GetComponent<Renderer>();
		_isDefault = true;
		_isAmbientLight = _light?.cookie;

		if (_light)
		{
			_defaultColor = _light.color;
			_defaultIntensity = _light.intensity;
		}
		else if (_renderer)
		{
			_defaultColor = _renderer.material.color;
		}
	}

	public void UpdateValues(float t)
	{
		if (t == 0)
		{
			if (_isDefault) return;
			else _isDefault = true;
		}
		else _isDefault = false;

		var color = Color.Lerp(_defaultColor, PassColor, t);
		var intensity = Mathf.Lerp(_defaultIntensity, PassIntensity, t);

		if (_light)
		{
			if (!_isAmbientLight) _light.color = color;
			_light.intensity = intensity;
		}
		else if (_renderer)
		{
			_renderer.material.color = color;
		}
	}
}
