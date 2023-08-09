using HarmonyLib;
using NewHorizons;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.ModHelper;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace QuasarProject;

public class QuasarProject : ModBehaviour
{
	public static QuasarProject Instance;

	public INewHorizons NewHorizons;

	public static AssetBundle ResourceBundle;

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
		ResourceBundle = ModHelper.Assets.LoadBundle(Path.Combine("planets", "volumetriclights"));

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

			Delay.FireOnNextUpdate(() =>
			{
				// for volumetric
				// copied from eots lol
				var gos = new[]
				{
					Locator.GetPlayerCamera().gameObject,
					Locator.GetProbe().GetForwardCamera().gameObject,
					Locator.GetProbe().GetReverseCamera().gameObject,
					Locator.GetProbe().GetRotatingCamera().gameObject,
					Locator.GetToolModeSwapper().GetProbeLauncher()._preLaunchCamera.gameObject
				};
				foreach (var go in gos) go.GetAddComponent<VolumetricLightRenderer>();

				// rumble support
				Locator.GetPlayerBody().gameObject.AddComponent<DiskGuardPassEffect>();
			});
		});
	}

	public override void Configure(IModConfig config)
	{
		var resolutionSetting = ModHelper.Config.GetSettingsValue<string>("Renderer Resolution");
		var resolution = resolutionSetting switch
		{
			"Full" => 1,
			"Half" => 2,
			"Quarter" => 4,
			"Eighth" => 8,
			_ => 1
		};
		PortalController.SetResolution(resolution);
	}
}
