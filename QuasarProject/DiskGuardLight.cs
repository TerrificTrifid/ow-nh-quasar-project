using NewHorizons;
using UnityEngine;

namespace QuasarProject;

[UsedInUnityProject]
public class DiskGuardLight : MonoBehaviour
{
	private Color _lightColor;
	private float _lightIntensity;
	public Color DarkColor;
	public float DarkIntensity;

	private Light _light;
	private Renderer _fogRenderer;

	private void Awake()
	{
		_light = GetComponent<Light>();
		_fogRenderer = GetComponent<Renderer>();

		if (_light)
		{
			_lightColor = _light.color;
			_lightIntensity = _light.intensity;
		}
		else if (_fogRenderer)
		{
			_lightColor = _fogRenderer.material.color;
		}
	}

	public void UpdateValues(float t)
	{
		var color = Color.Lerp(_lightColor, DarkColor, t);
		var intensity = Mathf.Lerp(_lightIntensity, DarkIntensity, t);

		if (_light)
		{
			_light.color = color;
			_light.intensity = intensity;
		}
		else if (_fogRenderer)
		{
			_fogRenderer.material.color = color;
		}
	}
}
