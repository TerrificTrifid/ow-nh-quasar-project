using HarmonyLib;
using NewHorizons;
using NewHorizons.Handlers;
using UnityEngine;


namespace QuasarProject
{
    /// <summary>
    /// controls the hamster ball
    /// </summary>
    [UsedInUnityProject]
    [HarmonyPatch]
    public class HamsterBallItem : OWItem
    {
        private ScreenPrompt _activatePrompt;
        private ScreenPrompt _setCheckpointPrompt;
        private ScreenPrompt _gotoCheckpointPrompt;

        public override string GetDisplayName()
        {
            return TranslationHandler.GetTranslation("Prototype", TranslationHandler.TextType.UI);
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

            _activatePrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, TranslationHandler.GetTranslation("Activate", TranslationHandler.TextType.UI) + "   <CMD>");
            _setCheckpointPrompt = new ScreenPrompt(InputLibrary.toolOptionUp, TranslationHandler.GetTranslation("Designate", TranslationHandler.TextType.UI) + "   <CMD>");
            _gotoCheckpointPrompt = new ScreenPrompt(InputLibrary.toolOptionDown, TranslationHandler.GetTranslation("Recall", TranslationHandler.TextType.UI) + "   <CMD>");
        }

        public override void PickUpItem(Transform holdTranform)
        {
            base.PickUpItem(holdTranform);
            enabled = true;

            Locator.GetPromptManager().AddScreenPrompt(_activatePrompt, PromptPosition.UpperRight, true);
            Locator.GetPromptManager().AddScreenPrompt(_setCheckpointPrompt, PromptPosition.UpperRight, true);
            Locator.GetPromptManager().AddScreenPrompt(_gotoCheckpointPrompt, PromptPosition.UpperRight, true);
        }

        public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
        {
            base.DropItem(position, normal, parent, sector, customDropTarget);
            enabled = false;
            HamsterBallController.Instance.SetActive(false);

            Locator.GetPromptManager().RemoveScreenPrompt(_activatePrompt, PromptPosition.UpperRight);
            Locator.GetPromptManager().RemoveScreenPrompt(_setCheckpointPrompt, PromptPosition.UpperRight);
            Locator.GetPromptManager().RemoveScreenPrompt(_gotoCheckpointPrompt, PromptPosition.UpperRight);
        }

        public override void SocketItem(Transform socketTransform, Sector sector)
        {
            base.SocketItem(socketTransform, sector);
            enabled = false;
            HamsterBallController.Instance.SetActive(false);

            Locator.GetPromptManager().RemoveScreenPrompt(_activatePrompt, PromptPosition.UpperRight);
            Locator.GetPromptManager().RemoveScreenPrompt(_setCheckpointPrompt, PromptPosition.UpperRight);
            Locator.GetPromptManager().RemoveScreenPrompt(_gotoCheckpointPrompt, PromptPosition.UpperRight);
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


        [HarmonyPostfix, HarmonyPatch(typeof(ToolModeUI), nameof(ToolModeUI.Update))]
        private static void ToolModeUI_Update(ToolModeUI __instance)
        {
            if (OWInput.IsInputMode(InputMode.Character) && __instance._toolSwapper.GetToolMode() == ToolMode.Item)
            {
                if (__instance._toolSwapper.GetItemCarryTool().GetHeldItem() is HamsterBallItem)
                {
                    __instance._projectPrompt.SetVisibility(false);
                }
            }
        }
    }
}
