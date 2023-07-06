using NewHorizons;
using UnityEngine;

namespace QuasarProject
{
    /// <summary>
    /// attached to the player. handles the actual ball
    ///
    /// BUG: detector will fuck up with entry way trigger. might not need to do anything about this 
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
        }

        private void Start()
        {
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
                if (checkpoint == null)
                {
                    checkpoint = new GameObject("HamsterBallCheckpoint");
                }
                
                checkpointNormal = raycastHit.normal;
                checkpoint.transform.position = raycastHit.point + checkpointNormal * 1.5f/*ball radius*/;
                checkpoint.transform.parent = raycastHit.rigidbody.transform;
            } 
            else
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
            rigidbody.WarpToPositionRotation(checkpoint.transform.position, Quaternion.LookRotation(rigidbody.transform.forward, checkpointNormal));
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
            // should be same as ball's own detector alignment direction
            var targetDirection = Locator.GetPlayerForceDetector().GetAlignmentAcceleration();

            var rotation = Quaternion.FromToRotation(currentDirection, targetDirection);
            AttachPoint.transform.rotation = rotation * AttachPoint.transform.rotation;
        }
    }
}
