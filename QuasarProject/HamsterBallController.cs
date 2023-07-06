using NewHorizons;
using NewHorizons.Utility;
using OWML.Common;
using OWML.ModHelper;
using System;
using UnityEngine;

namespace QuasarProject
{
    /// <summary>
    /// attached to the player. handles the actual ball
    /// </summary>
    [UsedInUnityProject]
    public class HamsterBallController : MonoBehaviour
    {
        public static HamsterBallController Instance { get; private set; }

        public OWRigidbody Rigidbody;
        public PlayerAttachPoint AttachPoint;
        public GameObject checkpoint;
        public Vector3 checkpointNormal;

        private bool _active;

        private void Awake()
        {
            
            Instance = this;
            checkpoint = new GameObject("HamsterBallCheckpoint");
            checkpoint.transform.parent = this.transform;
            Rigidbody.Suspend();
            gameObject.SetActive(false);
            // AttachPoint.SetAttachOffset(Vector3.zero);
            // this.transform.position = Locator.GetPlayerTransform().position;
        }

        public void SetCheckpoint()
        {
            int layerMask = OWLayerMask.physicalMask;
            Vector3 position = Locator.GetActiveCamera().transform.position;
            Vector3 forward = Locator.GetActiveCamera().transform.forward;
            RaycastHit raycastHit;
            if (Physics.Raycast(position, forward, out raycastHit, 100f, layerMask)) { 
                checkpoint.transform.position = raycastHit.point;
                checkpoint.transform.parent = raycastHit.rigidbody.gameObject.transform;
                checkpointNormal = raycastHit.normal;
            } else
            {
                Locator.GetPlayerAudioController().OnExitDreamWorld(AudioType.Artifact_Extinguish);
            }


        }

        public void GoToCheckpoint()
        {
            if (checkpoint == null) return;
            OWRigidbody rigidbody = Locator.GetPlayerBody();
            rigidbody.transform.localPosition = checkpoint.transform.position;
            rigidbody.transform.localRotation = Quaternion.LookRotation(checkpointNormal);
            rigidbody.SetVelocity(Vector3.zero);
            rigidbody.SetAngularVelocity(Vector3.zero);
        }


        public bool IsActive() => _active;

        public void SetActive(bool active)
        {
            if (_active == active) return;
            _active = active;

            if (active)
            {
                Locator.GetPlayerAudioController().OnExitDreamWorld(AudioType.Artifact_Extinguish);
                gameObject.SetActive(true);
                Rigidbody.Unsuspend();
                // AttachPoint.AttachPlayer();
                // this.transform.position = Locator.GetPlayerTransform().position;
                // AttachPoint.SetAttachOffset(Vector3.zero);
            }
            else
            {
                Locator.GetPlayerAudioController().OnExitDreamWorld(AudioType.Artifact_Extinguish);

                // AttachPoint.DetachPlayer();
                // AttachPoint.SetAttachOffset(Vector3.zero);
                Rigidbody.Suspend();
                gameObject.SetActive(false);
            }
        }

        private void FixedUpdate()
        {
            // align attach point
            var currentDirection = -AttachPoint.transform.up;
            var targetDirection = Locator.GetPlayerForceDetector().GetAlignmentAcceleration();

            var rotation = Quaternion.FromToRotation(currentDirection, targetDirection);
            AttachPoint.transform.rotation = rotation * AttachPoint.transform.rotation;
            // AttachPoint.SetAttachOffset(Vector3.zero);
        }
    }
}
