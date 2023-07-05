using NewHorizons;
using NewHorizons.Handlers;

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

        }

        private void OnPickup(OWItem instance)
        {
            DeathManager deathManager = FindObjectOfType<DeathManager>();
            deathManager.KillPlayer(DeathType.Default);

        }

        







    }
}