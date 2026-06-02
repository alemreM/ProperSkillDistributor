using System;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace ProperSkillDistributor
{
    [HarmonyPatch]
    public static class AutoPerkDailyTickPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(CharacterDevelopmentCampaignBehavior), "DailyTickHero");
        }

        private static bool Prefix(Hero hero)
        {
            return AutoPerkGuard.ShouldVanillaAllocatePerks(hero);
        }
    }

    [HarmonyPatch]
    public static class AutoPerkDevelopmentGatePatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(CharacterDevelopmentCampaignBehavior),
                "ShouldDevelopCharacterStats",
                new[] { typeof(Hero) });
        }

        private static bool Prepare()
        {
            return AccessTools.Method(
                typeof(CharacterDevelopmentCampaignBehavior),
                "ShouldDevelopCharacterStats",
                new[] { typeof(Hero) }) != null;
        }

        private static bool Prefix(Hero hero, ref bool __result)
        {
            if (AutoPerkGuard.ShouldVanillaAllocatePerks(hero))
            {
                return true;
            }

            __result = false;
            return false;
        }
    }

    [HarmonyPatch]
    public static class AutoPerkCharacterCreationPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(CharacterDevelopmentCampaignBehavior),
                "OnCharacterCreationIsOver",
                Type.EmptyTypes);
        }

        private static bool Prepare()
        {
            return AccessTools.Method(
                typeof(CharacterDevelopmentCampaignBehavior),
                "OnCharacterCreationIsOver",
                Type.EmptyTypes) != null;
        }

        private static bool Prefix()
        {
            CampaignOptions.AutoAllocateClanMemberPerks = false;
            return false;
        }
    }

    internal static class AutoPerkGuard
    {
        public static bool ShouldVanillaAllocatePerks(Hero hero)
        {
            if (SkillPresetBehavior.IsPlayerDevelopableHero(hero) && hero != Hero.MainHero)
            {
                CampaignOptions.AutoAllocateClanMemberPerks = false;
                return false;
            }

            return true;
        }
    }
}
