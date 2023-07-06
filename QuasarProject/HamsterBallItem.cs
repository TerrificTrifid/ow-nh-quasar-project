using NewHorizons;
using NewHorizons.Handlers;
using UnityEngine;


namespace QuasarProject
{
    /// <summary>
    /// controls the hamster ball
    /// 
    /// using vision torch item for reference
    ///
    /// TODO: button prompts. where is this done in base game? its not on the item.
    /// </summary>
    [UsedInUnityProject]
    public class HamsterBallItem : OWItem
    {
        public override string GetDisplayName()
        {
            return TranslationHandler.GetTranslation("Hamster Ball Remote", TranslationHandler.TextType.UI);
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
            HamsterBallController.Instance.SetActive(false);
        }

        public override void SocketItem(Transform socketTransform, Sector sector)
        {
            base.SocketItem(socketTransform, sector);
            enabled = false;
            HamsterBallController.Instance.SetActive(false);
        }

        public override bool CheckIsDroppable()
        {
            return !HamsterBallController.Instance.IsActive();
        }

        private void Update()
        {
            if (OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary, InputMode.Character))
            {
                HamsterBallController.Instance.SetActive(!HamsterBallController.Instance.IsActive());
            }
            if (OWInput.IsNewlyPressed(InputLibrary.toolOptionUp, InputMode.Character))
            {
                HamsterBallController.Instance.SetCheckpoint();
            }
            if (OWInput.IsNewlyPressed(InputLibrary.toolOptionDown, InputMode.Character))
            {
                HamsterBallController.Instance.GoToCheckpoint();
            }
        }
    }
}
