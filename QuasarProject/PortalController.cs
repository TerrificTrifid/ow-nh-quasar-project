using NewHorizons;
using System.Collections.Generic;
using UnityEngine;

namespace QuasarProject
{
    [UsedInUnityProject]
    public class PortalController : MonoBehaviour
    {
        private List<GameObject> enteringGOs = new List<GameObject>();

        public PortalController pairedPortal;
        private Camera cam;
        protected RenderTexture rt;
        private MeshRenderer portalRenderer;

        private Transform playerTransform;

        public Transform debugPlayerReplacement;

        public void Awake()
        {
            rt = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
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
            enteringGOs.Add(other.gameObject);
        }

        public void OnTriggerExit(Collider other)
        {
            enteringGOs.Remove(other.gameObject);
        }

        private static Quaternion QuaternionFromEuler(Vector3 euler)
        {
            return Quaternion.Euler(euler.x, euler.y, euler.z);
        }

        public void Update()
        {
            var relativePos = pairedPortal.transform.InverseTransformPoint(playerTransform.position);
            var relativeRot = pairedPortal.transform.InverseTransformRotation(playerTransform.rotation);
            cam.transform.localPosition = -relativePos;
            cam.transform.localRotation = this.transform.InverseTransformRotation(pairedPortal.transform.rotation) * relativeRot; //QuaternionFromEuler(relativeRot.eulerAngles + new Vector3(0, 180, 0));

            // if any enteringGOs are on the opposite side, teleport them to pairedPortal, and add them to pairedPortal.enteredGOs
            // if any enteredGOs are on the opposite side, teleport them to pairedPortal, and add them to pairedPortal.enteringGOs

            if (enteringGOs.Count <= 0) return;

            var testList = enteringGOs.ToArray();
            foreach (var go in testList)
            {
                if (!IsPassedThrough(go.transform)) continue;
                pairedPortal.TeleportToMeFromPairedPortal(go.transform);
                pairedPortal.enteringGOs.Add(go);
                this.enteringGOs.Remove(go);
            }
        }

        // returns true if the center of inQuestion is behind the portal
        // I think this is the correct implementation, we'll find out
        private bool IsPassedThrough(Transform inQuestion)
        {
            var relativeLocation = this.transform.InverseTransformPoint(inQuestion.position);

            return Vector3.Dot(relativeLocation, new Vector3(1, 0, 0)) < 0;
        }

        private void TeleportToMeFromPairedPortal(Transform t)
        {
            var relativePos = pairedPortal.transform.InverseTransformPoint(t.position);
            var relativeRot = pairedPortal.transform.InverseTransformRotation(t.rotation);

            //t.transform.rotation = this.transform.rotation * relativeRot;
            //t.transform.position = this.transform.position + relativePos;

            // account for owrigidbody speed & angular momentum
            var rb = t.GetComponentInChildren<OWRigidbody>();
            var angVel = rb.GetAngularVelocity();
            var relativeVel = pairedPortal.transform.InverseTransformVector(rb.GetVelocity());
            var relativeAngularVel = pairedPortal.transform.InverseTransformRotation(Quaternion.Euler(angVel.x, angVel.y, angVel.z));

            rb.WarpToPositionRotation(this.transform.position + relativePos, this.transform.rotation * relativeRot);

            rb.SetVelocity(this.transform.TransformVector(relativeVel));
            rb.SetAngularVelocity((this.transform.rotation * relativeAngularVel).eulerAngles);
        }
    }
}
