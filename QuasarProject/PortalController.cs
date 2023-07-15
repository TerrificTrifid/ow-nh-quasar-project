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
        private MeshRenderer portalRenderer;

        private Camera playerCam;

        public OWTriggerVolume VolumeWhereActive;


        private float nearClipOffset = 0.05f;
        private float nearClipLimit = 0.2f;

        public void Awake()
        {
            // low res to not kill ur game fuck u
            rt = new RenderTexture(256, 256, 0);
            rt.Create();

            portalRenderer = GetComponentInChildren<MeshRenderer>();
            cam = GetComponentInChildren<Camera>();

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
            gameObject.SetActive(true);
        }

        private void OnExit(GameObject hitobj)
        {
            gameObject.SetActive(false);
        }

        public void OnTriggerEnter(Collider other)
        {
            trackedBodies.SafeAdd(other.GetAttachedOWRigidbody());
            QuasarProject.Instance.ModHelper.Console.WriteLine($"{other} enter {this}");
        }

        public void OnTriggerExit(Collider other)
        {
            trackedBodies.QuickRemove(other.GetAttachedOWRigidbody());
            QuasarProject.Instance.ModHelper.Console.WriteLine($"{other} exit {this}");
        }

        public void Update()
        {
            var relativePos = transform.InverseTransformPoint(playerCam.transform.position);
            var relativeRot = transform.InverseTransformRotation(playerCam.transform.rotation);
            cam.transform.SetPositionAndRotation(pairedPortal.transform.TransformPoint(relativePos), pairedPortal.transform.TransformRotation(relativeRot));
            cam.fieldOfView = playerCam.fieldOfView;
            ProtectScreenFromClipping(playerCam.transform.position);
            SetNearClipPlane();

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
        void SetNearClipPlane()
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
    }
}
