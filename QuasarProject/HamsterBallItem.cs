using NewHorizons;
using NewHorizons.Handlers;
using UnityEngine;

namespace QuasarProject
{
    // using vision torch item for reference
    [UsedInUnityProject]
    public class HamsterBallItem : OWItem
    {
        public override string GetDisplayName()
        {
            return TranslationHandler.GetTranslation("Hamster Ball", TranslationHandler.TextType.UI);
        }

        public override void Awake()
        {
            // prevents scout equip
            _type = ItemType.VisionTorch;
            base.Awake();
        }

        private void Start()
        {
            enabled = false;
        }

        public override void PickUpItem(Transform holdTranform)
        {
            base.PickUpItem(holdTranform);
            enabled = true;
        }

        public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
        {
            base.DropItem(position, normal, parent, sector, customDropTarget);
            enabled = false;
        }

        public override void SocketItem(Transform socketTransform, Sector sector)
        {
            base.SocketItem(socketTransform, sector);
            enabled = false;
        }

        private void Update()
        {
            if (OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary, InputMode.Character))
            {
                Locator.GetPlayerAudioController().OnExitDreamWorld(AudioType.Artifact_Extinguish);
                // test
                Locator.GetPlayerBody().AddForce(Locator.GetPlayerBody().GetVelocity() * 1000f);
            }
        }
    }
}
