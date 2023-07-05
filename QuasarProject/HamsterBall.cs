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

        private void OnPickup(OWItem arg1)
        {
            DeathManager deathManager = FindObjectOfType<DeathManager>();
            deathManager.KillPlayer(DeathType.Default);

        }

        //on pickup add a force to the player







    }
}