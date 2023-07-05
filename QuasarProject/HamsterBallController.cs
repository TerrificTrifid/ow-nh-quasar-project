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

        private void Awake()
        {
            Instance = this;
        }

        private bool _active;
        public bool IsActive() => _active;

        public void SetActive(bool active)
        {
            if (_active == active) return;
            _active = active;

            if (active)
            {
                Locator.GetPlayerAudioController().OnExitDreamWorld(AudioType.Artifact_Extinguish);
                // test
                Locator.GetPlayerBody().AddForce(Locator.GetPlayerBody().GetVelocity() * 1000f);
            }
            else
            {
                Locator.GetPlayerAudioController().OnExitDreamWorld(AudioType.Artifact_Extinguish);
                // test
                Locator.GetPlayerBody().AddForce(Locator.GetPlayerBody().GetVelocity() * -1000f);
            }
        }
    }
}
