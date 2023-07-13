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
        private readonly List<OWRigidbody> trackedBodies = new();

        public PortalController pairedPortal;
        private Camera cam;
        private RenderTexture rt;
        private MeshRenderer portalRenderer;

        private Transform playerCamTransform;

        public Transform debugPlayerReplacement;

        public void Awake()
        {
            rt = new RenderTexture(256, 256, 0);
            rt.Create();

            portalRenderer = GetComponentsInChildren<MeshRenderer>()[1]; // change back after removing cubes
            cam = GetComponentInChildren<Camera>();

            cam.targetTexture = rt;
        }

        public void Start()
        {
            portalRenderer.material.SetTexture("_MainTex", pairedPortal.rt);
            playerCamTransform = debugPlayerReplacement ?? Locator.GetPlayerCamera().transform;
        }

        public void OnDestroy()
        {
            // Release the hardware resources used by the render texture 
            rt.Release();
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
            var relativePos = transform.InverseTransformPoint(playerCamTransform.position);
            var relativeRot = transform.InverseTransformRotation(playerCamTransform.rotation);
            pairedPortal.cam.transform.localPosition = relativePos;
            pairedPortal.cam.transform.localRotation = relativeRot;

            if (trackedBodies.Count <= 0) return;

            for (var i = trackedBodies.Count - 1; i >= 0; i--)
            {
                var body = trackedBodies[i];
                if (!IsPassedThrough(body)) continue;
                QuasarProject.Instance.ModHelper.Console.WriteLine($"{body} tp {this} -> {pairedPortal}");
                pairedPortal.ReceiveWarpedBody(body);

                // pairedPortal.trackedBodies.SafeAdd(body);
                // trackedBodies.QuickRemoveAt(i);
            }
        }

        private bool IsPassedThrough(OWRigidbody body)
        {
            var relativePos = transform.InverseTransformPoint(body.GetPosition());

            return Vector3.Dot(relativePos, Vector3.forward) < 0;
        }

        private void ReceiveWarpedBody(OWRigidbody body)
        {
            var relativePos = pairedPortal.transform.InverseTransformPoint(body.GetPosition());
            relativePos += Vector3.forward * .1f; // push you thru the portal a bit more
            var relativeRot = pairedPortal.transform.InverseTransformRotation(body.GetRotation());

            var relativeVel = pairedPortal.transform.InverseTransformVector(body.GetVelocity());
            var relativeAngularVel = pairedPortal.transform.InverseTransformVector(body.GetAngularVelocity());

            body.WarpToPositionRotation(transform.TransformPoint(relativePos), transform.TransformRotation(relativeRot));

            body.SetVelocity(transform.TransformVector(relativeVel));
            body.SetAngularVelocity(transform.TransformVector(relativeAngularVel));
        }
    }
}
