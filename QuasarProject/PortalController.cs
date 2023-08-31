using HarmonyLib;
using NewHorizons;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using UnityEngine;

namespace QuasarProject;

// https://github.com/SebLague/Portals/blob/master/Assets/Scripts/Core/Portal.cs
// https://danielilett.com/2019-12-14-tut4-2-portal-rendering/
[UsedInUnityProject]
[HarmonyPatch]
public class PortalController : MonoBehaviour
{
	private readonly List<OWRigidbody> trackedBodies = new();

	public PortalController pairedPortal; // required
	private Camera cam;
	private RenderTexture rt;
	private Renderer portalRenderer;

	private static Camera playerCam;
	private static Renderer[] playerRenderers; // to turn off while rendering portals :P

	public OWTriggerVolume VolumeWhereActive; // required
	public Collider[] IgnoreCollisionWith;

	[Header("Hacks")]
	public Renderer[] OtherRenderersToDisable;

	public PortalController VisibleThroughPortal;
	private bool isVisibleThroughPortal;

	private static Quaternion halfTurn = Quaternion.Euler(0.0f, 180.0f, 0.0f);

	private void Awake()
	{
		portalRenderer = GetComponentInChildren<Renderer>();
		cam = GetComponentInChildren<Camera>();
		cam.enabled = false; // we render manually
		cam.gameObject.AddComponent<VolumetricLightRenderer>(); // could add this in prefab, but eh
		cam.cameraType = CameraType.Preview; // so it doesnt do proxy shadows

		VolumeWhereActive.OnEntry += OnEntry;
		VolumeWhereActive.OnExit += OnExit;

		if (VisibleThroughPortal)
		{
			isVisibleThroughPortal = true; // let the main trigger control this by disabling, so it takes priority over vtp trigger
			VisibleThroughPortal.VolumeWhereActive.OnEntry += OnVtpEntry;
			VisibleThroughPortal.VolumeWhereActive.OnExit += OnVtpExit;
		}
	}

	private void Start()
	{
		if (!playerCam)
		{
			playerCam = Locator.GetPlayerCamera().mainCamera;
			playerRenderers = Locator.GetPlayerTransform().GetComponentsInChildren<Renderer>(true);
		}

		gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		ReleaseRt();

		VolumeWhereActive.OnEntry -= OnEntry;
		VolumeWhereActive.OnExit -= OnExit;

		if (VisibleThroughPortal)
		{
			VisibleThroughPortal.VolumeWhereActive.OnEntry -= OnVtpEntry;
			VisibleThroughPortal.VolumeWhereActive.OnExit -= OnVtpExit;
		}
	}

	private void OnEntry(GameObject hitobj)
	{
		if (gameObject.activeSelf) return;
		var body = hitobj.GetAttachedOWRigidbody();
		if (!body.CompareTag("Player")) return;

		NHLogger.LogVerbose($"activate \"{transform.GetPath()}\"");

		gameObject.SetActive(true);
		CreateRt();

		if (VisibleThroughPortal) isVisibleThroughPortal = false;
	}

	private void OnExit(GameObject hitobj)
	{
		if (!gameObject.activeSelf) return;
		var body = hitobj.GetAttachedOWRigidbody();
		if (!body.CompareTag("Player")) return;

		NHLogger.LogVerbose($"deactivate \"{transform.GetPath()}\"");

		gameObject.SetActive(false);
		ReleaseRt();
		trackedBodies.Clear();

		foreach (var collider1 in body.GetComponentsInChildren<Collider>(true))
			foreach (var collider2 in IgnoreCollisionWith)
				Physics.IgnoreCollision(collider1, collider2, false);
		if (body.TryGetComponent(out HighSpeedImpactSensor highSpeedImpactSensor))
			highSpeedImpactSensor.enabled = true;
		if (body.TryGetComponent(out ProbeAnchor probeAnchor))
			probeAnchor.enabled = true;

		if (VisibleThroughPortal) isVisibleThroughPortal = true;
	}

	private void OnVtpEntry(GameObject hitobj)
	{
		if (gameObject.activeSelf) return;
		var body = hitobj.GetAttachedOWRigidbody();
		if (!body.CompareTag("Player")) return;

		NHLogger.LogVerbose($"vtp activate \"{transform.GetPath()}\"");

		gameObject.SetActive(true);
		CreateRt();
	}

	private void OnVtpExit(GameObject hitobj)
	{
		if (!gameObject.activeSelf) return;
		var body = hitobj.GetAttachedOWRigidbody();
		if (!body.CompareTag("Player")) return;

		NHLogger.LogVerbose($"vtp deactivate \"{transform.GetPath()}\"");

		gameObject.SetActive(false);
		ReleaseRt();
		trackedBodies.Clear();

		foreach (var collider1 in body.GetComponentsInChildren<Collider>(true))
			foreach (var collider2 in IgnoreCollisionWith)
				Physics.IgnoreCollision(collider1, collider2, false);
		if (body.TryGetComponent(out HighSpeedImpactSensor highSpeedImpactSensor))
			highSpeedImpactSensor.enabled = true;
		if (body.TryGetComponent(out ProbeAnchor probeAnchor))
			probeAnchor.enabled = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!gameObject.activeSelf) return;
		var body = other.GetAttachedOWRigidbody();
		if (!trackedBodies.SafeAdd(body)) return;

		NHLogger.LogVerbose($"\"{body.name}\" enter \"{transform.GetPath()}\"");

		foreach (var collider1 in body.GetComponentsInChildren<Collider>(true))
			foreach (var collider2 in IgnoreCollisionWith)
				Physics.IgnoreCollision(collider1, collider2, true);
		if (body.TryGetComponent(out HighSpeedImpactSensor highSpeedImpactSensor))
			highSpeedImpactSensor.enabled = false;
		if (body.TryGetComponent(out ProbeAnchor probeAnchor))
			probeAnchor.enabled = false;
	}

	private void OnTriggerExit(Collider other)
	{
		if (!gameObject.activeSelf) return;
		var body = other.GetAttachedOWRigidbody();
		if (!trackedBodies.QuickRemove(body)) return;

		NHLogger.LogVerbose($"\"{body.name}\" exit \"{transform.GetPath()}\"");

		foreach (var collider1 in body.GetComponentsInChildren<Collider>(true))
			foreach (var collider2 in IgnoreCollisionWith)
				Physics.IgnoreCollision(collider1, collider2, false);
		if (body.TryGetComponent(out HighSpeedImpactSensor highSpeedImpactSensor))
			highSpeedImpactSensor.enabled = true;
		if (body.TryGetComponent(out ProbeAnchor probeAnchor))
			probeAnchor.enabled = true;
	}

	#region resolution stuff

	private static int _resolution = 1;

	public static void SetResolution(int resolution)
	{
		_resolution = resolution;
		// inactive ones will set resolution on enable so its okay
		foreach (var portalController in FindObjectsOfType<PortalController>())
		{
			portalController.ReleaseRt();
			portalController.CreateRt();
		}
	}

	private void CreateRt()
	{
		if (rt != null) return;

		rt = new RenderTexture(Screen.width / _resolution, Screen.height / _resolution, 0);
		rt.Create();

		cam.targetTexture = rt;
		portalRenderer.material.SetTexture("_MainTex", rt);
	}

	private void ReleaseRt()
	{
		if (rt == null) return;

		cam.targetTexture = null;
		portalRenderer.material.SetTexture("_MainTex", null);

		rt.Release();
		rt = null;
	}

	#endregion

	private void Update()
	{
		// move to player cam
		cam.transform.SetPositionAndRotation(playerCam.transform.position, playerCam.transform.rotation);
		if (isVisibleThroughPortal)
		{
			// move from vtp portal to vtp paired portal
			var relativePos = VisibleThroughPortal.transform.InverseTransformPoint(cam.transform.position);
			var relativeRot = VisibleThroughPortal.transform.InverseTransformRotation(cam.transform.rotation);
			cam.transform.SetPositionAndRotation(
				VisibleThroughPortal.pairedPortal.transform.TransformPoint(halfTurn * relativePos),
				VisibleThroughPortal.pairedPortal.transform.TransformRotation(halfTurn * relativeRot)
			);
		}
		{
			// move from this portal to paired portal
			var relativePos = transform.InverseTransformPoint(cam.transform.position);
			var relativeRot = transform.InverseTransformRotation(cam.transform.rotation);
			cam.transform.SetPositionAndRotation(
				pairedPortal.transform.TransformPoint(halfTurn * relativePos),
				pairedPortal.transform.TransformRotation(halfTurn * relativeRot)
			);
		}

		cam.fieldOfView = playerCam.fieldOfView;

		portalRenderer.forceRenderingOff = true;
		pairedPortal.portalRenderer.forceRenderingOff = true;
		if (isVisibleThroughPortal)
		{
			VisibleThroughPortal.portalRenderer.forceRenderingOff = true;
			VisibleThroughPortal.pairedPortal.portalRenderer.forceRenderingOff = true;
		}
		foreach (var renderer in OtherRenderersToDisable) renderer.forceRenderingOff = true;
		foreach (var renderer in playerRenderers) renderer.forceRenderingOff = true;

		cam.Render();

		portalRenderer.forceRenderingOff = false;
		pairedPortal.portalRenderer.forceRenderingOff = false;
		if (isVisibleThroughPortal)
		{
			VisibleThroughPortal.portalRenderer.forceRenderingOff = false;
			VisibleThroughPortal.pairedPortal.portalRenderer.forceRenderingOff = false;
		}
		foreach (var renderer in OtherRenderersToDisable) renderer.forceRenderingOff = false;
		foreach (var renderer in playerRenderers) renderer.forceRenderingOff = false;


		// go backwards since we remove
		for (var i = trackedBodies.Count - 1; i >= 0; i--)
		{
			var body = trackedBodies[i];
			if (!IsPassedThrough(body)) continue;

			NHLogger.LogVerbose($"\"{body.name}\" tp \"{transform.GetPath()}\" -> \"{pairedPortal.transform.GetPath()}\"");
			// triggers are in FixedUpdate so we have to do this manually
			var someCollider = body.GetComponentInChildren<Collider>(true);
			OnTriggerExit(someCollider);
			VolumeWhereActive.OnTriggerExit(someCollider);
			pairedPortal.ReceiveWarpedBody(body);
			pairedPortal.VolumeWhereActive.OnTriggerEnter(someCollider);
			pairedPortal.OnTriggerEnter(someCollider);
		}
	}

	private bool IsPassedThrough(OWRigidbody body)
	{
		var pos = body.CompareTag("Player") ? playerCam.transform.position : body.GetPosition();
		return Vector3.Dot(pos - transform.position, transform.forward) < 0;
	}

	private void ReceiveWarpedBody(OWRigidbody body)
	{
		var relativePos = pairedPortal.transform.InverseTransformPoint(body.GetPosition());
		var relativeRot = pairedPortal.transform.InverseTransformRotation(body.GetRotation());

		var relativeVel = pairedPortal.transform.InverseTransformDirection(body.GetVelocity());
		var relativeAngularVel = pairedPortal.transform.InverseTransformDirection(body.GetAngularVelocity());

		body.SetPosition(transform.TransformPoint(halfTurn * relativePos));
		body.SetRotation(transform.TransformRotation(halfTurn * relativeRot));

		body.SetVelocity(transform.TransformVector(halfTurn * relativeVel));
		body.SetAngularVelocity(transform.TransformVector(halfTurn * relativeAngularVel));

		if (!Physics.autoSyncTransforms) Physics.SyncTransforms();
	}


	private void OnDrawGizmos()
	{
		var modifier = OWGizmos.IsDirectlySelected(gameObject) ? 1 : 2;

		// required things error checking
		Gizmos.matrix = transform.localToWorldMatrix;
		if (!VolumeWhereActive || !pairedPortal)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawCube(Vector3.zero, new Vector3(4f, 4f, 1f));
			return;
		}

		Gizmos.color = new Color(1f, 0.5f, 0f);
		Gizmos.DrawLine(Vector3.zero, Vector3.forward * 4);
		Gizmos.DrawLine(Vector3.zero, Vector3.up * 2);
		Gizmos.color = Color.grey;
		Gizmos.DrawCube(Vector3.forward * -0.75f, new Vector3(4f, 4f, 0.5f));

		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.color = Color.yellow / modifier;
		Gizmos.DrawLine(transform.position, pairedPortal.transform.position);
		if (VisibleThroughPortal)
		{
			Gizmos.color = Color.cyan / modifier;
			Gizmos.DrawLine(transform.position, VisibleThroughPortal.transform.position);
		}

		Gizmos.matrix = VolumeWhereActive.transform.localToWorldMatrix;
		var box = VolumeWhereActive.GetComponent<BoxCollider>();
		if (box && OWGizmos.IsDirectlySelected(gameObject))
		{
			Gizmos.color = new Color(0.5f, 1f, 0.5f);
			Gizmos.DrawWireCube(box.center, box.size);
		}
	}


	// increase align rate for portals
	[HarmonyPostfix, HarmonyPatch(typeof(AlignPlayerWithForce), nameof(AlignPlayerWithForce.StartAlignment))]
	private static void AlignPlayerWithForce_StartAlignment(AlignPlayerWithForce __instance)
	{
		if (QuasarProject.Instance.NewHorizons.GetCurrentStarSystem() == "Trifid.QuasarProject")
		{
			__instance._degreesPerSecond *= 1.5f;
		}
	}
}
