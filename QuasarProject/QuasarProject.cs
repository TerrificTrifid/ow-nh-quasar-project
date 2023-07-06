using NewHorizons;
using OWML.Common;
using OWML.ModHelper;

namespace QuasarProject
{
    public class QuasarProject : ModBehaviour
    {
        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(QuasarProject)} is loaded!", MessageType.Success);

            // Get the New Horizons API and load configs
            var newHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            newHorizons.LoadConfigs(this);


            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (newHorizons.GetCurrentStarSystem() != "Trifid.QuasarProject") return;
                ModHelper.Console.WriteLine("Loaded into QP!", MessageType.Success);
            };
        }
    }
}
