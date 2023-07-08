using NewHorizons;
using NewHorizons.Utility.Files;
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

        [Space]
        public OWAudioSource AudioSource;
        public AudioClip Activate, Deactivate;

        [Space]
        public GameObject CheckpointPrefab;

        private GameObject _checkpoint;

        private bool _active;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // let other scripts Start run
            Delay.FireOnNextUpdate(() =>
            {
                // add visual
                var surface = Locator.GetPlayerTransform().Find("Surface");
                if (surface != null)
                {
                    surface.SetParent(transform);
                    surface.localPosition = Vector3.zero;
                    surface.localScale = new Vector3(1.4f, 1.4f, 1.4f);

                    var material = Locator.GetSunController()._supernova._supernovaMaterial;
                    material.SetColor("_Color", new Color(0.25f, 0.25f, 0.25f));
                    material.SetTexture("_ColorRamp", ImageUtilities.GetTexture(QuasarProject.Instance, "planets/BallRamp.png"));
                    material.SetVector("_WaveScaleMain", new Vector4(0.4f, 0.05f, 2f, 2f));
                    material.SetVector("_WaveScaleMacro", new Vector4(2f, 0.2f, 2f, 1f));
                    surface.GetComponent<TessellatedSphereRenderer>()._materials = new[] { material };
                }

                gameObject.SetActive(false);
            });
        }

        private void OnDestroy()
        {
            Destroy(_checkpoint);
        }

        public void SetCheckpoint()
        {
            if (Physics.Raycast(Locator.GetPlayerBody().GetPosition(), -Locator.GetPlayerBody().GetLocalUpDirection(), out var raycastHit, 2f, OWLayerMask.groundMask))
            {
                if (_checkpoint == null)
                {
                    _checkpoint = Instantiate(CheckpointPrefab);
                }

                _checkpoint.transform.position = Locator.GetPlayerBody().GetPosition();
                _checkpoint.transform.rotation = Locator.GetPlayerBody().GetRotation();
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

            var rigidbody = _active ? Rigidbody : Locator.GetPlayerBody();
            rigidbody.WarpToPositionRotation(_checkpoint.transform.position, _checkpoint.transform.rotation);
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

                Rigidbody.WarpToPositionRotation(Locator.GetPlayerBody().GetPosition(), Locator.GetPlayerBody().GetRotation());
                Rigidbody.SetVelocity(Locator.GetPlayerBody().GetVelocity());
                Rigidbody.SetAngularVelocity(Vector3.zero);
                // makes it not flip
                AttachPoint.transform.rotation = Locator.GetPlayerBody().GetRotation();

                gameObject.SetActive(true);

                AttachPoint.AttachPlayer();
                // snap to center
                Locator.GetPlayerTransform().localPosition = Vector3.zero;
                Locator.GetPlayerTransform().localRotation = Quaternion.identity;
            }
            else
            {
                Locator.GetPlayerAudioController().OnExitDreamWorld(AudioType.Artifact_Extinguish);

                AttachPoint.DetachPlayer();

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


            // movement wah
            var wasd = OWInput.GetAxisValue(InputLibrary.moveXZ, InputMode.Character);
            var localMovement = new Vector3(wasd.x, 0, wasd.y);
            var movement = Locator.GetPlayerTransform().TransformDirection(localMovement);
            Rigidbody.AddVelocityChange(movement * .3f);
        }
    }
}
