using HarmonyLib;
using NewHorizons;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;
using UnityEngine;

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
            //ModHelper.Console.WriteLine($"My mod {nameof(QuasarProject)} is loaded!", MessageType.Success);

            // Get the New Horizons API and load configs
            NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizons.LoadConfigs(this);


            NewHorizons.GetStarSystemLoadedEvent().AddListener(system =>
            {
                if (system != "Trifid.QuasarProject") return;
                ModHelper.Console.WriteLine("Loaded into QP!", MessageType.Success);

                // copied from ring/moving skybox
                var rotateTransform = GameObject.Find("/Skybox").AddComponent<RotateTransform>();
                rotateTransform._localAxis = Vector3.up;
                rotateTransform._degreesPerSecond = -1.5f;
            });
        }
    }
}
