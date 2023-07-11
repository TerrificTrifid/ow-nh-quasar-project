using HarmonyLib;
using UnityEngine;

namespace QuasarProject;

[HarmonyPatch]
public static class TextColorPatches
{
	private static readonly Color _baseEmissionColor = new(0.5294f, 0.5763f, 1.5f, 1f);

	private static readonly Color _baseProjectorColor = new(1.4118f, 1.5367f, 4f, 1f);
	private static readonly Color _baseTextColor = new(0.8824f, 0.9604f, 2.5f, 1f);
	private static readonly Color _baseTextShadowColor = new(0.3529f, 0.3843f, 1f, 0.25f);


	[HarmonyPrefix, HarmonyPatch(typeof(NomaiTextLine), nameof(NomaiTextLine.DetermineTextLineColor))]
	private static bool NomaiTextLine_DetermineTextLineColor(NomaiTextLine __instance, NomaiTextLine.VisualState state, out Color __result)
	{
		Color color = Color.white;
		bool flag = false;
		if (__instance._active)
		{
			switch (state)
			{
				case NomaiTextLine.VisualState.HIDDEN:
					color = Color.white;
					flag = false;
					break;
				case NomaiTextLine.VisualState.UNREAD:
					flag = true;
					if (__instance._speakerLocation == NomaiText.Location.UNSPECIFIED || __instance._textLineLocation == NomaiText.Location.UNSPECIFIED)
					{
						color = NomaiTextLine.s_originSpeakerColorUnread;
					}
					else if (__instance._speakerLocation == __instance._textLineLocation)
					{
						color = NomaiTextLine.s_originSpeakerColorUnread;
					}
					else
					{
						color = NomaiTextLine.s_externalSpeakerColorUnread;
					}
					break;
				case NomaiTextLine.VisualState.TRANSLATED:
					if (__instance._speakerLocation == NomaiText.Location.UNSPECIFIED || __instance._textLineLocation == NomaiText.Location.UNSPECIFIED)
					{
						color = NomaiTextLine.s_originSpeakerColorTranslated;
					}
					else if (__instance._speakerLocation == __instance._textLineLocation)
					{
						color = NomaiTextLine.s_originSpeakerColorTranslated;
					}
					else
					{
						color = NomaiTextLine.s_externalSpeakerColorTranslated;
					}
					flag = true;
					break;
			}
		}
		if (flag)
		{
			color.a = 1f;
		}
		else
		{
			color.a = 0f;
		}
		__result = color;

		return false;
	}


	[HarmonyPrefix, HarmonyPatch(typeof(NomaiComputerRing), nameof(NomaiComputerRing.Update))]
	private static bool NomaiComputerRing_Update(NomaiComputerRing __instance)
	{
		if ((!__instance._activated || !__instance._translated) && __instance._emissionColorT < 1f)
		{
			__instance._emissionColorT = Mathf.MoveTowards(__instance._emissionColorT, 1f, Time.unscaledDeltaTime / __instance._colorFadeTime);
			NomaiComputerRing.s_matPropBlock.SetColor(NomaiComputerRing.s_propID_Detail1EmissionColor, Color.Lerp(NomaiComputerRing.s_colorTranslated, __instance._baseEmissionColor, __instance._emissionColorT));
			__instance._renderer.SetPropertyBlock(NomaiComputerRing.s_matPropBlock);
			return false;
		}
		if (__instance._activated && __instance._translated && __instance._emissionColorT > 0f)
		{
			__instance._emissionColorT = Mathf.MoveTowards(__instance._emissionColorT, 0f, Time.unscaledDeltaTime / __instance._colorFadeTime);
			NomaiComputerRing.s_matPropBlock.SetColor(NomaiComputerRing.s_propID_Detail1EmissionColor, Color.Lerp(NomaiComputerRing.s_colorTranslated, __instance._baseEmissionColor, __instance._emissionColorT));
			__instance._renderer.SetPropertyBlock(NomaiComputerRing.s_matPropBlock);
		}

		return false;
	}


	[HarmonyPrefix, HarmonyPatch(typeof(NomaiVesselComputerRing), nameof(NomaiVesselComputerRing.UpdateColor))]
	private static bool NomaiVesselComputerRing_UpdateColor(NomaiVesselComputerRing __instance)
	{
		__instance._ringRenderer.SetColor(Color.Lerp(NomaiVesselComputerRing.s_textColorTranslated, __instance._baseTextColor, __instance._colorT));
		__instance._ringRenderer.SetMaterialProperty(__instance._propID_ShadowColor, Color.Lerp(NomaiVesselComputerRing.s_textShadowColorTranslated, __instance._baseTextShadowColor, __instance._colorT));
		__instance._projectorRenderer.SetColor(Color.Lerp(NomaiVesselComputerRing.s_projectorColorTranslated, __instance._baseProjectorColor, __instance._colorT));

		return false;
	}
}
