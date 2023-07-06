using NewHorizons;
using NewHorizons.Utility.OWML;
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
        
        private GameObject _checkpoint;
        private Vector3 _checkpointNormal;

        private bool _active;

        private void Awake()
        {
            Instance = this;
            
            // wait for stuff to init LOLLL
            Delay.FireInNUpdates(() =>
            {
                Rigidbody.Suspend();
                gameObject.SetActive(false);
                
                Rigidbody.SetVelocity(Vector3.zero);
                Rigidbody.SetAngularVelocity(Vector3.zero);
            }, 2);
        }

        private void OnDestroy()
        {
            Destroy(_checkpoint);
        }

        public void SetCheckpoint()
        {
            int layerMask = OWLayerMask.physicalMask;
            Vector3 position = Locator.GetActiveCamera().transform.position;
            Vector3 forward = Locator.GetActiveCamera().transform.forward;
            RaycastHit raycastHit;
            if (Physics.Raycast(position, forward, out raycastHit, 100f, layerMask)) {
                if (_checkpoint == null)
                {
                    _checkpoint = new GameObject("HamsterBallCheckpoint");
                }
                
                _checkpointNormal = raycastHit.normal;
                _checkpoint.transform.position = raycastHit.point + _checkpointNormal * 1.5f/*ball radius*/;
                _checkpoint.transform.parent = raycastHit.rigidbody.transform;
            } 
            else
            {
                Locator.GetPlayerAudioController().PlayNegativeUISound();
            }
        }

        public void GoToCheckpoint()
        {
            if (_checkpoint == null)
            {
                Locator.GetPlayerAudioController().PlayNegativeUISound();
                return;
            }

            OWRigidbody rigidbody = _active ? Rigidbody : Locator.GetPlayerBody();
            rigidbody.WarpToPositionRotation(_checkpoint.transform.position, Quaternion.LookRotation(rigidbody.transform.forward, _checkpointNormal));
           
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
                
                AttachPoint.AttachPlayer();
            }
            else
            {
                Locator.GetPlayerAudioController().OnExitDreamWorld(AudioType.Artifact_Extinguish);

                AttachPoint.DetachPlayer();
                
                Rigidbody.Suspend();
                gameObject.SetActive(false);
            }
            
            Rigidbody.SetPosition(Locator.GetPlayerBody().GetPosition());
            Rigidbody.SetVelocity(Vector3.zero);
            Rigidbody.SetAngularVelocity(Vector3.zero);
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
