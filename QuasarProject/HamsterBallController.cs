using NewHorizons;
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
        public bool checkpointSet = false;

        private bool _active;

        private void Awake()
        {
            Instance = this;
            
            Rigidbody.Suspend();
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            Destroy(checkpoint);
        }

        public void SetCheckpoint()
        {
            int layerMask = OWLayerMask.physicalMask;
            Vector3 position = Locator.GetActiveCamera().transform.position;
            Vector3 forward = Locator.GetActiveCamera().transform.forward;
            RaycastHit raycastHit;

            
            if (Physics.Raycast(position, forward, out raycastHit, 100f, layerMask)) {
                Quaternion q = Quaternion.LookRotation(Vector3.ProjectOnPlane((position - raycastHit.point).normalized, raycastHit.normal), raycastHit.normal);

                checkpoint.transform.position = raycastHit.point;
                checkpoint.transform.rotation = raycastHit.rigidbody.transform.InverseTransformRotation(q);
                checkpoint.transform.parent = raycastHit.rigidbody.gameObject.transform;
                checkpointNormal = raycastHit.normal;
                checkpointSet = true;
            } else
            {
                Locator.GetPlayerAudioController().PlayNegativeUISound();
            }
        }

        public void GoToCheckpoint()
        {
            if (checkpoint == null)
            {
                Locator.GetPlayerAudioController().PlayNegativeUISound();
                return;
            }

            OWRigidbody rigidbody = _active ? Rigidbody : Locator.GetPlayerBody();
            rigidbody.WarpToPositionRotation(checkpoint.transform.position, Quaternion.LookRotation(checkpointNormal));
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
                
                Rigidbody.SetVelocity(Vector3.zero);
                Rigidbody.SetAngularVelocity(Vector3.zero);
                
                Rigidbody.SetPosition(Locator.GetPlayerBody().GetPosition());
                AttachPoint.AttachPlayer();
            }
            else
            {
                Locator.GetPlayerAudioController().OnExitDreamWorld(AudioType.Artifact_Extinguish);

                AttachPoint.DetachPlayer();
                
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
        }
    }
}
