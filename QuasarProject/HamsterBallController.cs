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

        private bool _active;

        private void Awake()
        {
            Instance = this;
            Rigidbody.Suspend();
            gameObject.SetActive(false);
            AttachPoint.SetAttachOffset(Vector3.zero);
            this.transform.position = Locator.GetPlayerTransform().position;
        }

        public bool IsActive() => _active;

        public void SetActive(bool active)
        {
            if (_active == active) return;
            _active = active;

            if (active)
            {
                Locator.GetPlayerAudioController().OnExitDreamWorld(AudioType.Artifact_Extinguish);
                this.transform.position = Locator.GetPlayerTransform().position;
                gameObject.SetActive(true);
                Rigidbody.Unsuspend();
                AttachPoint.AttachPlayer();
                AttachPoint.SetAttachOffset(Vector3.zero);
                
            }
            else
            {
                Locator.GetPlayerAudioController().OnExitDreamWorld(AudioType.Artifact_Extinguish);

                AttachPoint.DetachPlayer();
                Rigidbody.Suspend();
                gameObject.SetActive(false);
                AttachPoint.SetAttachOffset(Vector3.zero);
            }
        }

        private void FixedUpdate()
        {
            // align attach point
            var currentDirection = -AttachPoint.transform.up;
            var targetDirection = Locator.GetPlayerForceDetector().GetAlignmentAcceleration();

            var rotation = Quaternion.FromToRotation(currentDirection, targetDirection);
            AttachPoint.transform.rotation = rotation * AttachPoint.transform.rotation;
            AttachPoint.SetAttachOffset(Vector3.zero);

            
        }
    }
}
