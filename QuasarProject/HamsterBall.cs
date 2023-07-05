using NewHorizons;
using NewHorizons.Handlers;
using static StencilPreviewImageEffect;
using UnityEngine;

namespace QuasarProject
{
    [UsedInUnityProject]
    public class HamsterBall : OWItem
    {
        public override string GetDisplayName()
        {
            return TranslationHandler.GetTranslation("Hamster Ball", TranslationHandler.TextType.UI);
        }
        public HamsterBall() {
            
            this.onPickedUp += new OWEvent<OWItem>.OWCallback(this.OnPickup);

            //blocks scout launcher
            this._type = ItemType.VisionTorch;

        }

        private void OnPickup(OWItem instance)
        {
            base.enabled = true;
            Locator.GetToolModeSwapper().EquipToolMode(ToolMode.Item);
        }
        public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
        {
            base.DropItem(position, normal, parent, sector, customDropTarget);
            base.enabled = false;
            Locator.GetToolModeSwapper().EquipToolMode(ToolMode.None);
        }
        private void Update()

        {
            bool canbeused = Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Item);
            this.wasUsing = this.Using;
            this.Using = (OWInput.IsPressed(InputLibrary.toolActionPrimary, InputMode.Character, 0f));

            if (this.Using && !this.wasUsing && canbeused)
            {
                Locator.GetPlayerAudioController().OnExitDreamWorld(AudioType.Artifact_Extinguish);
                Locator.GetPlayerBody().GetComponent<Rigidbody>().AddForce(Locator.GetPlayerBody().GetComponent<Rigidbody>().velocity * 1000f);
            }
        }


        bool Using;
        bool wasUsing;

    }
}