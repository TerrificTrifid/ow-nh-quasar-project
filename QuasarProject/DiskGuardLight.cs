using NewHorizons;
using UnityEngine;

namespace QuasarProject;

[UsedInUnityProject]
public class DiskGuardLight : MonoBehaviour
{
	private Color _lightColor;
	private float _lightIntesity;
	public Color DarkColor;
	public float DarkIntensity;

	private Light _light;
	private MeshRenderer _fogRenderer;

	private void Awake() { }
}
