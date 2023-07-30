using NewHorizons;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuasarProject
{
    // referencing https://github.com/SebLague/Portals/blob/master/Assets/Scripts/Core/Portal.cs
    [UsedInUnityProject]
    public class PortalController : MonoBehaviour
    {
        private readonly List<OWRigidbody> trackedBodies = new();

        public PortalController pairedPortal;
        private Camera cam;
        private RenderTexture rt;
        private Renderer portalRenderer;

        private Camera playerCam;

        public OWTriggerVolume VolumeWhereActive;

        [Header("Hacks")]
        public bool SetNearClipPlane;
        public Renderer[] OtherRenderersToDisable;

        public PortalController VisibleThroughPortal;


        private float nearClipOffset = 0.05f;
        private float nearClipLimit = 0.2f;

        public void Awake()
        {
            // low res to not kill ur game fuck u
            rt = new RenderTexture(Screen.width / 4, Screen.height / 4, 0);
            rt.Create();

            portalRenderer = GetComponentInChildren<Renderer>();
            cam = GetComponentInChildren<Camera>();
            cam.enabled = false; // we render manually

            cam.targetTexture = rt;
            portalRenderer.material.SetTexture("_MainTex", rt);
            portalRenderer.material.SetInt("displayMask", 1);

            VolumeWhereActive.OnEntry += OnEntry;
            VolumeWhereActive.OnExit += OnExit;
            gameObject.SetActive(false);
        }

        public void Start()
        {
            playerCam = Locator.GetPlayerCamera().mainCamera;
        }

        public void OnDestroy()
        {
            // Release the hardware resources used by the render texture 
            rt.Release();

            VolumeWhereActive.OnEntry -= OnEntry;
            VolumeWhereActive.OnExit -= OnExit;
        }

        private void OnEntry(GameObject hitobj)
        {
            if (hitobj.GetAttachedOWRigidbody().CompareTag("Player"))
            {
                QuasarProject.Instance.ModHelper.Console.WriteLine($"player activate {this}");
                gameObject.SetActive(true);
                trackedBodies.Clear();
            }
        }

        private void OnExit(GameObject hitobj)
        {
            if (hitobj.GetAttachedOWRigidbody().CompareTag("Player"))
            {
                QuasarProject.Instance.ModHelper.Console.WriteLine($"player deactivate {this}");
                gameObject.SetActive(false);
                trackedBodies.Clear();
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (trackedBodies.SafeAdd(other.GetAttachedOWRigidbody()))
                QuasarProject.Instance.ModHelper.Console.WriteLine($"{other} enter {this}");
        }

        public void OnTriggerExit(Collider other)
        {
            if (trackedBodies.QuickRemove(other.GetAttachedOWRigidbody()))
                QuasarProject.Instance.ModHelper.Console.WriteLine($"{other} exit {this}");
        }

        public void Update()
        {
            var relativePos = transform.InverseTransformPoint(playerCam.transform.position);
            var relativeRot = transform.InverseTransformRotation(playerCam.transform.rotation);
            cam.transform.SetPositionAndRotation(pairedPortal.transform.TransformPoint(relativePos), pairedPortal.transform.TransformRotation(relativeRot));
            cam.fieldOfView = playerCam.fieldOfView;
            if (SetNearClipPlane) _SetNearClipPlane();
            ProtectScreenFromClipping(playerCam.transform.position);
            pairedPortal.portalRenderer.forceRenderingOff = true;
            foreach (var renderer in OtherRenderersToDisable) renderer.forceRenderingOff = true;
            cam.Render();
            pairedPortal.portalRenderer.forceRenderingOff = false;
            foreach (var renderer in OtherRenderersToDisable) renderer.forceRenderingOff = false;

            if (trackedBodies.Count <= 0) return;

            foreach (var body in trackedBodies)
            {
                if (!IsPassedThrough(body)) continue;

                QuasarProject.Instance.ModHelper.Console.WriteLine($"{body} tp {this} -> {pairedPortal}");
                pairedPortal.ReceiveWarpedBody(body);
            }
        }

        private bool IsPassedThrough(OWRigidbody body)
        {
            // use portal renderer for proper direction
            var relativePos = portalRenderer.transform.InverseTransformPoint(body.GetPosition());

            // why does this have to be flipped backwards idk
            return Vector3.Dot(relativePos, Vector3.forward) < 0;
        }

        private void ReceiveWarpedBody(OWRigidbody body)
        {
            var relativePos = pairedPortal.transform.InverseTransformPoint(body.GetPosition());
            // relativePos += Vector3.forward * .1f; // push you thru the portal a bit more
            var relativeRot = pairedPortal.transform.InverseTransformRotation(body.GetRotation());

            var relativeVel = pairedPortal.transform.InverseTransformVector(body.GetVelocity());
            var relativeAngularVel = pairedPortal.transform.InverseTransformVector(body.GetAngularVelocity());

            body.WarpToPositionRotation(transform.TransformPoint(relativePos), transform.TransformRotation(relativeRot));

            body.SetVelocity(transform.TransformVector(relativeVel));
            body.SetAngularVelocity(transform.TransformVector(relativeAngularVel));
        }


        // Sets the thickness of the portal screen so as not to clip with camera near plane when player goes through
        float ProtectScreenFromClipping(Vector3 viewPoint)
        {
            float halfHeight = playerCam.nearClipPlane * Mathf.Tan(playerCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * playerCam.aspect;
            float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, playerCam.nearClipPlane).magnitude;
            float screenThickness = dstToNearClipPlaneCorner;

            Transform screenT = portalRenderer.transform;
            bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
            screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, screenThickness);
            screenT.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
            return screenThickness;
        }

        // Use custom projection matrix to align portal camera's near clip plane with the surface of the portal
        // Note that this affects precision of the depth buffer, which can cause issues with effects like screenspace AO
        void _SetNearClipPlane()
        {
            // Learning resource:
            // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
            Transform clipPlane = transform;
            int dot = Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - cam.transform.position));

            Vector3 camSpacePos = cam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
            Vector3 camSpaceNormal = cam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
            float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

            // Don't use oblique clip plane if very close to portal as it seems this can cause some visual artifacts
            if (Mathf.Abs(camSpaceDst) > nearClipLimit)
            {
                Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

                // Update projection based on new clip plane
                // Calculate matrix with player cam so that player camera settings (fov, etc) are used
                cam.projectionMatrix = playerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
            }
            else
            {
                cam.projectionMatrix = playerCam.projectionMatrix;
            }
        }


        private void OnDrawGizmos()
        {
            if (!portalRenderer)
                portalRenderer = GetComponentInChildren<Renderer>();
            var modifier = OWGizmos.IsDirectlySelected(gameObject) ? 1 : 2;

            // required things error checking
            Gizmos.matrix = Matrix4x4.TRS(portalRenderer.transform.position, portalRenderer.transform.rotation, transform.lossyScale);
            if (!VolumeWhereActive || !pairedPortal)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(Vector3.zero, new Vector3(4f, 4f, 0.101f));
                return;
            }

            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawLine(Vector3.forward * 0.06f, Vector3.forward * 4);
            Gizmos.DrawLine(Vector3.forward * 0.06f, Vector3.up * 2 + Vector3.forward * 0.06f);
            Gizmos.color = Color.grey;
            Gizmos.DrawCube(Vector3.forward * -0.025f, new Vector3(4f, 4f, 0.051f));
            Gizmos.color = Color.grey / modifier;
            Gizmos.DrawWireCube(Vector3.forward * -0.25f, new Vector3(4f, 4f, 0.5f));

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
    }
}
