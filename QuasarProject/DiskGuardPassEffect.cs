using NewHorizons;
using UnityEngine;

namespace QuasarProject;

[UsedInUnityProject]
public class DiskGuardPassEffect : MonoBehaviour
{
	private Color _defaultColor;
	private float _defaultIntensity;
	private float _defaultVolume;
	public Color PassColor;
	public float PassIntensity;
	public Light PassAmbientLight;
	public float PassVolume;

	private bool _isPlayer;
	private Light _light;
	private Renderer _renderer;
	private AudioSource _audioSource;
	private bool _isDefault;

	private static uint _rumbleId;

	private void Awake()
	{
		_isPlayer = gameObject.CompareTag("Player");
		_light = GetComponent<Light>();
		_renderer = GetComponent<Renderer>();
		_audioSource = GetComponent<AudioSource>();
		_isDefault = true;

		if (_isPlayer)
		{
			_rumbleId = RumbleManager.Steady(0, 0);
		}
		else if (_light)
		{
			_defaultColor = _light.color;
			_defaultIntensity = _light.intensity;
		}
		else if (_renderer)
		{
			_defaultColor = _renderer.material.color;
		}
		else if (_audioSource)
		{
			_defaultVolume = _audioSource.volume;
		}
	}

	private void OnDestroy()
	{
		if (_isPlayer)
		{
			RumbleManager.Release(_rumbleId);
		}
	}

	public void UpdateValues(float t)
	{
		if (t == 0)
		{
			if (_isDefault) return;
			_isDefault = true;
		}
		else
		{
			_isDefault = false;
		}

		if (_isPlayer)
		{
			var rumble = Mathf.Lerp(0, 1, t);
			RumbleManager.Modify(_rumbleId, rumble, rumble);
		}
		else if (_light)
		{
			_light.intensity = Mathf.Lerp(_defaultIntensity, PassIntensity, t);
			if (PassAmbientLight)
			{
				var t2 = Mathf.Lerp(t, 1 - t, 1 - t);
				PassAmbientLight.intensity = Mathf.Lerp(_defaultIntensity, PassIntensity, t2);
			}
			else
			{
				_light.color = Color.Lerp(_defaultColor, PassColor, t);
			}
		}
		else if (_renderer)
		{
			_renderer.material.color = Color.Lerp(_defaultColor, PassColor, t);
		}
		else if (_audioSource)
		{
			_audioSource.volume = Mathf.Lerp(_defaultVolume, PassVolume, t);
		}
	}
}
