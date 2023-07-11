using NewHorizons;
using NewHorizons.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace QuasarProject
{
    // referencing https://github.com/SebLague/Portals/blob/master/Assets/Scripts/Core/Portal.cs
    [UsedInUnityProject]
    public class PortalController : MonoBehaviour
    {
        private readonly List<OWRigidbody> enteringBodies = new();

        public PortalController pairedPortal;
        private Camera cam;
        private RenderTexture rt;
        private MeshRenderer portalRenderer;

        private Transform playerTransform;

        public Transform debugPlayerReplacement;

        public void Awake()
        {
            rt = new RenderTexture(256, 256, 0);
            rt.Create();

            portalRenderer = GetComponentInChildren<MeshRenderer>();
            cam = GetComponentInChildren<Camera>();

            cam.targetTexture = rt;
        }

        public void Start()
        {
            portalRenderer.material.SetTexture("_MainTex", pairedPortal.rt);
            playerTransform = debugPlayerReplacement ?? Locator.GetPlayerTransform();
        }

        public void OnDestroy()
        {
            // Release the hardware resources used by the render texture 
            rt.Release();
        }

        public void OnTriggerEnter(Collider other)
        {
            enteringBodies.SafeAdd(other.GetAttachedOWRigidbody());
        }

        public void OnTriggerExit(Collider other)
        {
            enteringBodies.QuickRemove(other.GetAttachedOWRigidbody());
        }

        public void Update()
        {
            var relativePos = transform.InverseTransformPoint(playerTransform.position);
            var relativeRot = transform.InverseTransformRotation(playerTransform.rotation);
            pairedPortal.cam.transform.localPosition = -relativePos;
            pairedPortal.cam.transform.localRotation = relativeRot * Quaternion.Euler(0, 180, 0);

            // if any enteringGOs are on the opposite side, teleport them to pairedPortal, and add them to pairedPortal.enteredGOs
            // if any enteredGOs are on the opposite side, teleport them to pairedPortal, and add them to pairedPortal.enteringGOs

            if (enteringBodies.Count <= 0) return;

            for (var i = enteringBodies.Count - 1; i >= 0; i--)
            {
                var body = enteringBodies[i];
                if (!IsPassedThrough(body)) continue;
                pairedPortal.ReceiveWarpedBody(body);

                pairedPortal.enteringBodies.SafeAdd(body);
                enteringBodies.QuickRemove(body);
            }
        }

        // returns true if the center of inQuestion is behind the portal
        // I think this is the correct implementation, we'll find out
        private bool IsPassedThrough(OWRigidbody body)
        {
            var relativePos = transform.InverseTransformPoint(body.GetPosition());

            return Vector3.Dot(relativePos, new Vector3(1, 0, 0)) < 0;
        }

        private void ReceiveWarpedBody(OWRigidbody body)
        {
            var relativePos = pairedPortal.transform.InverseTransformPoint(body.GetPosition());
            var relativeRot = pairedPortal.transform.InverseTransformRotation(body.GetRotation());

            var relativeVel = pairedPortal.transform.InverseTransformVector(body.GetVelocity());
            var relativeAngularVel = pairedPortal.transform.InverseTransformVector(body.GetAngularVelocity());

            body.WarpToPositionRotation(transform.TransformPoint(relativePos), transform.TransformRotation(relativeRot));

            body.SetVelocity(transform.TransformVector(relativeVel));
            body.SetAngularVelocity(transform.TransformVector(relativeAngularVel));
        }
    }
}
