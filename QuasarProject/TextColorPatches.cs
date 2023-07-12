using HarmonyLib;
using UnityEngine;

namespace QuasarProject;

[HarmonyPatch]
public static class TextColorPatches
{
	private static class OriginalColors
	{
		public static readonly Color s_originSpeakerColorUnread = new(0.5294114f, 0.5762672f, 1.5f, 1f);
		public static readonly Color s_originSpeakerColorTranslated = new(0.6f, 0.6f, 0.6f, 1f);
		public static readonly Color s_externalSpeakerColorUnread = new(1.5f, 0.9606341f, 0.5735294f, 1f);
		public static readonly Color s_externalSpeakerColorTranslated = new(0.6f, 0.4f, 0.22f, 1f);

		public static readonly Color _baseEmissionColor = new(0.5294f, 0.5763f, 1.5f, 1f);
		public static readonly Color s_colorTranslated = new(0.6f, 0.6f, 0.6f, 1f);

		public static readonly Color _baseTextColor = new(0.8824f, 0.9604f, 2.5f, 1f);
		public static readonly Color s_textColorTranslated = new(1.5f, 1.5f, 1.5f, 1f);
		public static readonly Color _baseTextShadowColor = new(0.3529f, 0.3843f, 1f, 0.25f);
		public static readonly Color s_textShadowColorTranslated = new(1f, 1f, 1f, 0.3f);
		public static readonly Color _baseProjectorColor = new(1.4118f, 1.5367f, 4f, 1f);
		public static readonly Color s_projectorColorTranslated = new(3f, 3f, 3f, 1f);
	}

	private static class EditedColors
	{
		public static readonly Color s_originSpeakerColorUnread = new(0.5294114f, 1.5f, 0.5762672f, 1f);
		public static readonly Color s_originSpeakerColorTranslated = new(0.6f, 0.6f, 0.6f, 1f);
		public static readonly Color s_externalSpeakerColorUnread = new(1.5f, 0.5735294f, 0.9606341f, 1f);
		public static readonly Color s_externalSpeakerColorTranslated = new(0.6f, 0.22f, 0.4f, 1f);

		public static readonly Color _baseEmissionColor = new(0.5294f, 1.5f, 0.5763f, 1f);
		public static readonly Color s_colorTranslated = new(0.6f, 0.6f, 0.6f, 1f);

		public static readonly Color _baseTextColor = new(0.8824f, 2.5f, 0.9604f, 1f);
		public static readonly Color s_textColorTranslated = new(1.5f, 1.5f, 1.5f, 1f);
		public static readonly Color _baseTextShadowColor = new(0.3529f, 1f, 0.3843f, 0.25f);
		public static readonly Color s_textShadowColorTranslated = new(1f, 1f, 1f, 0.3f);
		public static readonly Color _baseProjectorColor = new(1.4118f, 4f, 1.5367f, 1f);
		public static readonly Color s_projectorColorTranslated = new(3f, 3f, 3f, 1f);
	}

	[HarmonyPostfix, HarmonyPatch(typeof(NomaiTextLine), nameof(NomaiTextLine.Awake))]
	private static void NomaiTextLine_Awake(NomaiTextLine __instance)
	{
		if (true || QuasarProject.Instance.NewHorizons.GetCurrentStarSystem() == "Trifid.QuasarProject")
		{
			NomaiTextLine.s_originSpeakerColorUnread = EditedColors.s_originSpeakerColorUnread;
			NomaiTextLine.s_originSpeakerColorTranslated = EditedColors.s_originSpeakerColorTranslated;
			NomaiTextLine.s_externalSpeakerColorUnread = EditedColors.s_externalSpeakerColorUnread;
			NomaiTextLine.s_externalSpeakerColorTranslated = EditedColors.s_externalSpeakerColorTranslated;
		}
		else
		{
			NomaiTextLine.s_originSpeakerColorUnread = OriginalColors.s_originSpeakerColorUnread;
			NomaiTextLine.s_originSpeakerColorTranslated = OriginalColors.s_originSpeakerColorTranslated;
			NomaiTextLine.s_externalSpeakerColorUnread = OriginalColors.s_externalSpeakerColorUnread;
			NomaiTextLine.s_externalSpeakerColorTranslated = OriginalColors.s_externalSpeakerColorTranslated;
		}
	}

	[HarmonyPostfix, HarmonyPatch(typeof(NomaiComputerRing), nameof(NomaiComputerRing.Initialize))]
	private static void NomaiComputerRing_Initialize(NomaiComputerRing __instance)
	{
		if (true || QuasarProject.Instance.NewHorizons.GetCurrentStarSystem() == "Trifid.QuasarProject")
		{
			__instance._baseEmissionColor = EditedColors._baseEmissionColor;
			NomaiComputerRing.s_colorTranslated = EditedColors.s_colorTranslated;
		}
		else
		{
			__instance._baseEmissionColor = OriginalColors._baseEmissionColor;
			NomaiComputerRing.s_colorTranslated = OriginalColors.s_colorTranslated;
		}
	}

	[HarmonyPostfix, HarmonyPatch(typeof(NomaiVesselComputerRing), nameof(NomaiVesselComputerRing.Awake))]
	private static void NomaiVesselComputerRing_Awake(NomaiVesselComputerRing __instance)
	{
		if (true || QuasarProject.Instance.NewHorizons.GetCurrentStarSystem() == "Trifid.QuasarProject")
		{
			__instance._baseTextColor = EditedColors._baseTextColor;
			NomaiVesselComputerRing.s_textColorTranslated = EditedColors.s_textColorTranslated;
			__instance._baseTextShadowColor = EditedColors._baseTextShadowColor;
			NomaiVesselComputerRing.s_textShadowColorTranslated = EditedColors.s_textShadowColorTranslated;
			__instance._baseProjectorColor = EditedColors._baseProjectorColor;
			NomaiVesselComputerRing.s_projectorColorTranslated = EditedColors.s_projectorColorTranslated;
		}
		else
		{
			__instance._baseTextColor = OriginalColors._baseTextColor;
			NomaiVesselComputerRing.s_textColorTranslated = OriginalColors.s_textColorTranslated;
			__instance._baseTextShadowColor = OriginalColors._baseTextShadowColor;
			NomaiVesselComputerRing.s_textShadowColorTranslated = OriginalColors.s_textShadowColorTranslated;
			__instance._baseProjectorColor = OriginalColors._baseProjectorColor;
			NomaiVesselComputerRing.s_projectorColorTranslated = OriginalColors.s_projectorColorTranslated;
		}
	}
}
