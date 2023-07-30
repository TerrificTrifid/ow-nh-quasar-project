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

        public PortalController pairedPortal; //required
        private Camera cam;
        private RenderTexture rt;
        private Renderer portalRenderer;

        private static Camera playerCam;
        private static Renderer[] playerRenderers; // to turn off while rendering portals :P

        public OWTriggerVolume VolumeWhereActive; //required
        public Collider[] IgnoreCollisionWith;

        [Header("Hacks")]
        public bool SetNearClipPlane;
        public Renderer[] OtherRenderersToDisable;

        public PortalController VisibleThroughPortal;
        private bool isVisibleThroughPortal;

        private const float nearClipOffset = 0.05f;
        private const float nearClipLimit = 0.2f;

        public void Awake()
        {
            portalRenderer = GetComponentInChildren<Renderer>();
            cam = GetComponentInChildren<Camera>();
            cam.enabled = false; // we render manually

            portalRenderer.material.SetInt("displayMask", 1);

            VolumeWhereActive.OnEntry += OnEntry;
            VolumeWhereActive.OnExit += OnExit;

            if (VisibleThroughPortal)
            {
                isVisibleThroughPortal = true; // let the main trigger control this by disabling, so it takes priority over vtp trigger
                VisibleThroughPortal.VolumeWhereActive.OnEntry += OnVtpEntry;
                VisibleThroughPortal.VolumeWhereActive.OnExit += OnVtpExit;
            }

            gameObject.SetActive(false);
        }

        public void Start()
        {
            if (!playerCam)
            {
                playerCam = Locator.GetPlayerCamera().mainCamera;
                playerRenderers = Locator.GetPlayerTransform().GetComponentsInChildren<Renderer>(true);
            }
        }

        public void OnDestroy()
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
            var body = hitobj.GetAttachedOWRigidbody();
            if (body.CompareTag("Player"))
            {
                // QuasarProject.Instance.ModHelper.Console.WriteLine($"player activate {this}");
                gameObject.SetActive(true);
                CreateRt();
                if (VisibleThroughPortal) isVisibleThroughPortal = false;
            }
        }

        private void OnExit(GameObject hitobj)
        {
            var body = hitobj.GetAttachedOWRigidbody();
            if (body.CompareTag("Player"))
            {
                // QuasarProject.Instance.ModHelper.Console.WriteLine($"player deactivate {this}");
                gameObject.SetActive(false);
                ReleaseRt();
                trackedBodies.Clear();
                foreach (var collider1 in body.GetComponentsInChildren<Collider>(true))
                    foreach (var collider2 in IgnoreCollisionWith)
                        Physics.IgnoreCollision(collider1, collider2, false);
                if (VisibleThroughPortal) isVisibleThroughPortal = true;
            }
        }

        private void OnVtpEntry(GameObject hitobj)
        {
            var body = hitobj.GetAttachedOWRigidbody();
            if (body.CompareTag("Player"))
            {
                // QuasarProject.Instance.ModHelper.Console.WriteLine($"player vtp activate {this}");
                gameObject.SetActive(true);
                CreateRt();
            }
        }

        private void OnVtpExit(GameObject hitobj)
        {
            var body = hitobj.GetAttachedOWRigidbody();
            if (body.CompareTag("Player"))
            {
                // QuasarProject.Instance.ModHelper.Console.WriteLine($"player vtp deactivate {this}");
                gameObject.SetActive(false);
                ReleaseRt();
                trackedBodies.Clear();
                foreach (var collider1 in body.GetComponentsInChildren<Collider>(true))
                    foreach (var collider2 in IgnoreCollisionWith)
                        Physics.IgnoreCollision(collider1, collider2, false);
            }
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

        public void OnTriggerEnter(Collider other)
        {
            var body = other.GetAttachedOWRigidbody();
            if (trackedBodies.SafeAdd(body))
            {
                QuasarProject.Instance.ModHelper.Console.WriteLine($"{body} enter {this}");
                foreach (var collider1 in body.GetComponentsInChildren<Collider>(true))
                    foreach (var collider2 in IgnoreCollisionWith)
                        Physics.IgnoreCollision(collider1, collider2, true);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            var body = other.GetAttachedOWRigidbody();
            if (trackedBodies.QuickRemove(body))
            {
                QuasarProject.Instance.ModHelper.Console.WriteLine($"{body} exit {this}");
                foreach (var collider1 in body.GetComponentsInChildren<Collider>(true))
                    foreach (var collider2 in IgnoreCollisionWith)
                        Physics.IgnoreCollision(collider1, collider2, false);
            }
        }

        public void Update()
        {
            if (isVisibleThroughPortal)
            {
                // BUG: might not work if vtp stuff is rotated differently? test at some point
                var relativePos1 = transform.InverseTransformPoint(playerCam.transform.position);
                var relativePos2 = VisibleThroughPortal.transform.InverseTransformPoint(VisibleThroughPortal.pairedPortal.transform.position);
                var relativeRot1 = transform.InverseTransformRotation(playerCam.transform.rotation);
                var relativeRot2 = VisibleThroughPortal.transform.InverseTransformRotation(VisibleThroughPortal.pairedPortal.transform.rotation);
                cam.transform.SetPositionAndRotation(pairedPortal.transform.TransformPoint(relativePos1 - relativePos2), pairedPortal.transform.TransformRotation(relativeRot1 * relativeRot2));
            }
            else
            {
                var relativePos = transform.InverseTransformPoint(playerCam.transform.position);
                var relativeRot = transform.InverseTransformRotation(playerCam.transform.rotation);
                cam.transform.SetPositionAndRotation(pairedPortal.transform.TransformPoint(relativePos), pairedPortal.transform.TransformRotation(relativeRot));
            }

            cam.fieldOfView = playerCam.fieldOfView;
            if (SetNearClipPlane) _SetNearClipPlane();
            // ProtectScreenFromClipping(playerCam.transform.position);
            pairedPortal.portalRenderer.forceRenderingOff = true;
            if (isVisibleThroughPortal) VisibleThroughPortal.portalRenderer.forceRenderingOff = true;
            foreach (var renderer in OtherRenderersToDisable) renderer.forceRenderingOff = true;
            foreach (var renderer in playerRenderers) renderer.forceRenderingOff = true;
            cam.Render();
            pairedPortal.portalRenderer.forceRenderingOff = false;
            if (isVisibleThroughPortal) VisibleThroughPortal.portalRenderer.forceRenderingOff = false;
            foreach (var renderer in OtherRenderersToDisable) renderer.forceRenderingOff = false;
            foreach (var renderer in playerRenderers) renderer.forceRenderingOff = false;

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
            var pos = body.CompareTag("Player") ? playerCam.transform.position : body.GetPosition();
            var relativePos = portalRenderer.transform.InverseTransformPoint(pos);

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
