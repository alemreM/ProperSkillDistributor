using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Localization;

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
                new TextObject("{=auto_perks_disabled}Disabled by Skill Distributor.").ToString(),
                0f);
        }
    }
}
