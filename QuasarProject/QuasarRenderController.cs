using NewHorizons;
using UnityEngine;

namespace QuasarProject
{
    [UsedInUnityProject]
    public class QuasarRenderController : MonoBehaviour
    {
        private Transform _center;
        private Material _material;

        private void Awake()
        {
            _center = this.GetAttachedOWRigidbody().transform;
            _material = GetComponent<Renderer>().material;
        }

        private void Update()
        {
            var toPlayer = Locator.GetPlayerTransform().position - _center.position;
            var toSelf = transform.position - _center.position;
            if (Vector3.Dot(toPlayer, toSelf) > 0)
            {
                _material.renderQueue = 3000 + 1;
            }
            else
            {
                _material.renderQueue = 3000 - 1;
            }
        }
    }
}
