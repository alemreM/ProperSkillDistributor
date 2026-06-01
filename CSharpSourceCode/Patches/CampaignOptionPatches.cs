using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;

namespace ProperSkillDistributor
{
    [HarmonyPatch(typeof(CampaignOptionData), "GetIsDisabledWithReason")]
    public static class CampaignOptionPatches
    {
        private static void Postfix(CampaignOptionData __instance, ref CampaignOptionDisableStatus __result)
        {
            if (__instance.GetIdentifier() != "AutoAllocateClanMemberPerks")
            {
                return;
            }

            CampaignOptions.AutoAllocateClanMemberPerks = false;

            __result = new CampaignOptionDisableStatus(
                true,
                "Disabled by Skill Presets.",
                0f);
        }
    }
}
