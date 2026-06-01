using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace ProperSkillDistributor
{
    [HarmonyPatch]
    public static class AutoPerkAllocationPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(CharacterDevelopmentCampaignBehavior), "DailyTickHero");
        }

        private static bool Prefix(Hero hero)
        {
            if (SkillPresetBehavior.IsPlayerDevelopableHero(hero) && hero != Hero.MainHero)
            {
                TaleWorlds.CampaignSystem.CampaignOptions.AutoAllocateClanMemberPerks = false;
                return false;
            }

            return true;
        }
    }
}
