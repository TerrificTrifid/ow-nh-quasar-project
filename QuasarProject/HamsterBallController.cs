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
        
        private GameObject _checkpoint;
        private Vector3 _checkpointNormal;

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
                    material.SetTexture("_ColorRamp", ImageUtilities.GetTexture(Main.Instance, "planets/BallRamp.png"));
                    material.SetVector("_WaveScaleMain", new Vector4(0.4f, 0.05f, 2f, 2f));
                    material.SetVector("_WaveScaleMacro", new Vector4(2f, 0.2f, 2f, 1f));
                    surface.GetComponent<TessellatedSphereRenderer>()._materials = new Material[] { material };
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
