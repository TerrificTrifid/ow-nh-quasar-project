using HarmonyLib;
using NewHorizons;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace QuasarProject
{
    public class QuasarProject : ModBehaviour
    {
        public static QuasarProject Instance;

        public INewHorizons NewHorizons;

        private void Awake()
        {
            Instance = this;

            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(QuasarProject)} is loaded!", MessageType.Success);

            // Get the New Horizons API and load configs
            NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizons.LoadConfigs(this);


            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (NewHorizons.GetCurrentStarSystem() != "Trifid.QuasarProject") return;
                ModHelper.Console.WriteLine("Loaded into QP!", MessageType.Success);
            };
        }
    }
}
