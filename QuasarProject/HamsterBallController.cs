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
        public AudioClip Loop;
        private OWAudioSource _loopAudioSource;
        public AudioClip Activate;
        public AudioClip Deactivate;
        private OWAudioSource _oneShotAudioSource;

        [Space]
        public GameObject CheckpointPrefab;
        private GameObject _checkpoint;

        private bool _active;

        private void Awake()
        {
            Instance = this;

            Rigidbody.SetMaxAngularVelocity(10);
        }

        private void Start()
        {
            _loopAudioSource = Instantiate(
                Locator.GetPlayerAudioController()._oneShotSource,
                Locator.GetPlayerAudioController()._oneShotSource.transform.parent
            );
            _loopAudioSource.clip = Loop;
            _loopAudioSource.loop = true;
            _loopAudioSource.SetMaxVolume(0.1f);
            _oneShotAudioSource = Instantiate(
                Locator.GetPlayerAudioController()._oneShotSource,
                Locator.GetPlayerAudioController()._oneShotSource.transform.parent
            );

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

            // let other scripts Start run
            Delay.FireOnNextUpdate(() =>
            {
                gameObject.SetActive(false);
            });
        }

        private void OnDestroy()
        {
            Destroy(_checkpoint);

            Destroy(_loopAudioSource.gameObject);
            Destroy(_oneShotAudioSource.gameObject);
        }

        public void SetCheckpoint()
        {
            // check for wall in front first
            if (Physics.Raycast(Locator.GetPlayerBody().GetPosition(), Locator.GetPlayerTransform().forward,
                out var raycastHit, 2, OWLayerMask.groundMask))
            {
                // then go down at the wall
                if (Physics.Raycast(raycastHit.point, -Locator.GetPlayerBody().GetLocalUpDirection(),
                    out raycastHit, 2, OWLayerMask.groundMask))
                {
                    if (_checkpoint == null)
                    {
                        _checkpoint = Instantiate(CheckpointPrefab);
                    }

                    _checkpoint.transform.position = raycastHit.point + Locator.GetPlayerBody().GetLocalUpDirection() * 2; // does this still clip into the wall a bit?
                    _checkpoint.transform.rotation = Locator.GetPlayerBody().GetRotation();
                    _checkpoint.transform.parent = raycastHit.rigidbody.transform;
                }
                else
                {
                    Locator.GetPlayerAudioController().PlayNegativeUISound();
                }
            }
            else
            {
                // otherwise just go straight forward and down
                if (Physics.Raycast(Locator.GetPlayerBody().GetPosition() + Locator.GetPlayerTransform().forward * 2,
                    -Locator.GetPlayerBody().GetLocalUpDirection(), out raycastHit, 2, OWLayerMask.groundMask))
                {
                    if (_checkpoint == null)
                    {
                        _checkpoint = Instantiate(CheckpointPrefab);
                    }

                    _checkpoint.transform.position = raycastHit.point + Locator.GetPlayerBody().GetLocalUpDirection() * 2;
                    _checkpoint.transform.rotation = Locator.GetPlayerBody().GetRotation();
                    _checkpoint.transform.parent = raycastHit.rigidbody.transform;
                }
                else
                {
                    Locator.GetPlayerAudioController().PlayNegativeUISound();
                }
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
                _loopAudioSource.FadeIn(.5f);
                _oneShotAudioSource.PlayOneShot(Activate);

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
                _loopAudioSource.FadeOut(.5f);
                _oneShotAudioSource.PlayOneShot(Deactivate);

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


            _loopAudioSource.pitch = 1 + Rigidbody.GetVelocity().magnitude / 100;
        }
    }
}
