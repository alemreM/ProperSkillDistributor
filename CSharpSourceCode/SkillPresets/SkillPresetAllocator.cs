using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace ProperSkillDistributor
{
    public static class SkillPresetAllocator
    {
        public static void ApplyPreset(Hero hero, SkillPreset preset)
        {
            if (!SkillPresetBehavior.IsPlayerDevelopableHero(hero) || preset == null || !preset.IsConfigured)
            {
                return;
            }

            preset.RebuildAfterLoad();

            SkillPresetBehavior behavior = SkillPresetBehavior.Current;
            bool spendLeftoverPoints = behavior == null || behavior.SpendLeftoverPoints;
            bool randomizeUnpickedPerks = behavior != null && behavior.RandomizeUnpickedPerks;
            HeroDeveloper developer = hero.HeroDeveloper;
            Hero mimicHero = null;

            List<KeyValuePair<CharacterAttribute, int>> attributeLine = new List<KeyValuePair<CharacterAttribute, int>>();
            List<KeyValuePair<SkillObject, int>> focusLine = new List<KeyValuePair<SkillObject, int>>();
            List<PerkObject> perkLine = new List<PerkObject>();
            HashSet<string> presetPerkChoices = new HashSet<string>();

            if (preset.IsMimicPreset)
            {
                // mimic source is live and interactive. hero skill gain is read each time target is applied
                mimicHero = behavior?.GetMimicSourceHero(preset);

                if (mimicHero == null)
                {
                    return;
                }
            }

            foreach (CharacterAttribute attribute in Attributes.All)
            {
                int target = mimicHero != null
                    ? mimicHero.GetAttributeValue(attribute)
                    : preset.GetAttributeTarget(attribute.StringId);

                if (target > 0)
                {
                    attributeLine.Add(new KeyValuePair<CharacterAttribute, int>(attribute, target));
                }
            }

            foreach (SkillObject skill in Skills.All)
            {
                int target = mimicHero != null
                    ? mimicHero.HeroDeveloper.GetFocus(skill)
                    : preset.GetSkillFocusTarget(skill.StringId);

                if (target > 0)
                {
                    focusLine.Add(new KeyValuePair<SkillObject, int>(skill, target));
                }
            }

            if (mimicHero != null)
            {
                foreach (PerkObject perk in PerkObject.All)
                {
                    if (perk.Skill != null && mimicHero.GetPerkValue(perk))
                    {
                        perkLine.Add(perk);
                        ReservePresetPerkPair(perk, presetPerkChoices);
                    }
                }
            }
            else
            {
                foreach (string perkId in preset.SelectedPerkIds)
                {
                    PerkObject perk = MBObjectManager.Instance.GetObject<PerkObject>(perkId);

                    if (perk != null && perk.Skill != null)
                    {
                        perkLine.Add(perk);
                        ReservePresetPerkPair(perk, presetPerkChoices);
                    }
                }
            }

            int maxAttribute = Campaign.Current.Models.CharacterDevelopmentModel.MaxAttribute;
            int attributeFloorLimit = behavior != null ? behavior.AttributeFloorLimit : maxAttribute;
            int attributePassCount = spendLeftoverPoints ? 3 : 2;

            while (developer.UnspentAttributePoints > 0)
            {
                CharacterAttribute picked = null;
                int pickedFloorTarget = 0;
                int pickedTarget = 0;
                int pickedCurrent = 0;

                for (int pass = 0; pass < attributePassCount && picked == null; pass++)
                {
                    for (int i = 0; i < attributeLine.Count; i++)
                    {
                        CharacterAttribute attribute = attributeLine[i].Key;
                        int target = attributeLine[i].Value;
                        int current = hero.GetAttributeValue(attribute);

                        if (current >= maxAttribute)
                        {
                            continue;
                        }

                        if (pass == 0)
                        {
                            int floorTarget = GetFloorTarget(target, attributeFloorLimit);

                            if (floorTarget <= 0 || current >= floorTarget)
                            {
                                continue;
                            }

                            if (floorTarget > pickedFloorTarget || floorTarget == pickedFloorTarget && target > pickedTarget)
                            {
                                picked = attribute;
                                pickedFloorTarget = floorTarget;
                                pickedTarget = target;
                            }
                        }
                        else if (pass == 1)
                        {
                            if (current >= target)
                            {
                                continue;
                            }

                            if (target > pickedTarget)
                            {
                                picked = attribute;
                                pickedTarget = target;
                            }
                        }
                        else if (CompareLeftoverTarget(current, target, pickedCurrent, pickedTarget) > 0)
                        {
                            picked = attribute;
                            pickedTarget = target;
                            pickedCurrent = current;
                        }
                    }
                }

                if (picked == null)
                {
                    break;
                }

                developer.AddAttribute(picked, 1);
            }

            int maxFocus = Campaign.Current.Models.CharacterDevelopmentModel.MaxFocusPerSkill;
            int focusFloorLimit = behavior != null ? behavior.FocusFloorLimit : maxFocus;
            int focusPassCount = spendLeftoverPoints ? 3 : 2;

            while (developer.UnspentFocusPoints > 0)
            {
                SkillObject picked = null;
                int pickedFloorTarget = 0;
                int pickedTarget = 0;
                int pickedCurrent = 0;
                int pickedAttributePriority = 0;

                for (int pass = 0; pass < focusPassCount && picked == null; pass++)
                {
                    for (int i = 0; i < focusLine.Count; i++)
                    {
                        SkillObject skill = focusLine[i].Key;
                        int target = focusLine[i].Value;
                        int current = developer.GetFocus(skill);

                        if (current >= maxFocus || !developer.CanAddFocusToSkill(skill))
                        {
                            continue;
                        }

                        int attributePriority = GetSkillAttributePriority(skill, attributeLine);

                        if (pass == 0)
                        {
                            int floorTarget = GetFloorTarget(target, focusFloorLimit);

                            if (floorTarget <= 0 || current >= floorTarget)
                            {
                                continue;
                            }

                            if (floorTarget > pickedFloorTarget
                                || floorTarget == pickedFloorTarget && target > pickedTarget
                                || floorTarget == pickedFloorTarget && target == pickedTarget && attributePriority > pickedAttributePriority)
                            {
                                picked = skill;
                                pickedFloorTarget = floorTarget;
                                pickedTarget = target;
                                pickedAttributePriority = attributePriority;
                            }
                        }
                        else if (pass == 1)
                        {
                            if (current >= target)
                            {
                                continue;
                            }

                            if (target > pickedTarget || target == pickedTarget && attributePriority > pickedAttributePriority)
                            {
                                picked = skill;
                                pickedTarget = target;
                                pickedAttributePriority = attributePriority;
                            }
                        }
                        else
                        {
                            int leftoverOrder = CompareLeftoverTarget(current, target, pickedCurrent, pickedTarget);

                            if (leftoverOrder > 0 || leftoverOrder == 0 && attributePriority > pickedAttributePriority)
                            {
                                picked = skill;
                                pickedTarget = target;
                                pickedCurrent = current;
                                pickedAttributePriority = attributePriority;
                            }
                        }
                    }
                }

                if (picked == null)
                {
                    break;
                }

                developer.AddFocus(picked, 1);
            }

            perkLine.Sort(delegate (PerkObject left, PerkObject right)
            {
                int skillOrder = string.Compare(left.Skill.StringId, right.Skill.StringId, StringComparison.Ordinal);

                if (skillOrder != 0)
                {
                    return skillOrder;
                }

                int tierOrder = left.RequiredSkillValue.CompareTo(right.RequiredSkillValue);

                if (tierOrder != 0)
                {
                    return tierOrder;
                }

                return string.Compare(left.StringId, right.StringId, StringComparison.Ordinal);
            });

            SpendPresetPerkLine(hero, developer, perkLine);

            if (randomizeUnpickedPerks)
            {
                SpendOpenPerkTiersRandomly(hero, developer, presetPerkChoices);
                SpendPresetPerkLine(hero, developer, perkLine);
            }
        }

        private static void SpendPresetPerkLine(Hero hero, HeroDeveloper developer, List<PerkObject> perkLine)
        {
            foreach (PerkObject perk in perkLine)
            {
                if (CanPickPerkTierNow(hero, perk))
                {
                    developer.AddPerk(perk);
                }
            }
        }

        private static void ReservePresetPerkPair(PerkObject perk, HashSet<string> presetPerkChoices)
        {
            presetPerkChoices.Add(perk.StringId);

            if (perk.AlternativePerk != null)
            {
                presetPerkChoices.Add(perk.AlternativePerk.StringId);
            }
        }

        private static void SpendOpenPerkTiersRandomly(Hero hero, HeroDeveloper developer, HashSet<string> presetPerkChoices)
        {
            List<PerkObject> openPerkTiers = new List<PerkObject>();

            foreach (PerkObject perk in PerkObject.All)
            {
                if (perk.Skill == null || PresetAlreadyOwnsThisTier(perk, presetPerkChoices))
                {
                    continue;
                }

                if (perk.AlternativePerk != null && string.Compare(perk.StringId, perk.AlternativePerk.StringId, StringComparison.Ordinal) > 0)
                {
                    continue;
                }

                openPerkTiers.Add(perk);
            }

            openPerkTiers.Sort(ComparePerkPlanOrder);

            for (int i = 0; i < openPerkTiers.Count; i++)
            {
                PerkObject perk = openPerkTiers[i];

                if (!CanPickPerkTierNow(hero, perk))
                {
                    continue;
                }

                List<PerkObject> randomChoices = new List<PerkObject> { perk };

                if (perk.AlternativePerk != null && CanPickPerkTierNow(hero, perk.AlternativePerk))
                {
                    randomChoices.Add(perk.AlternativePerk);
                }

                developer.AddPerk(randomChoices[MBRandom.RandomInt(randomChoices.Count)]);
            }
        }

        private static bool PresetAlreadyOwnsThisTier(PerkObject perk, HashSet<string> presetPerkChoices)
        {
            if (presetPerkChoices.Contains(perk.StringId))
            {
                return true;
            }

            return perk.AlternativePerk != null && presetPerkChoices.Contains(perk.AlternativePerk.StringId);
        }

        private static bool CanPickPerkTierNow(Hero hero, PerkObject perk)
        {
            if (hero.GetPerkValue(perk))
            {
                return false;
            }

            if (perk.AlternativePerk != null && hero.GetPerkValue(perk.AlternativePerk))
            {
                return false;
            }

            if (hero.GetSkillValue(perk.Skill) < perk.RequiredSkillValue)
            {
                return false;
            }

            return PreviousPerkTierPicked(hero, perk);
        }

        private static bool PreviousPerkTierPicked(Hero hero, PerkObject perk)
        {
            PerkObject previousTier = null;

            foreach (PerkObject candidate in PerkObject.All)
            {
                if (candidate.Skill != perk.Skill || candidate.RequiredSkillValue >= perk.RequiredSkillValue)
                {
                    continue;
                }

                if (previousTier == null || candidate.RequiredSkillValue > previousTier.RequiredSkillValue)
                {
                    previousTier = candidate;
                }
            }

            if (previousTier == null)
            {
                return true;
            }

            if (hero.GetPerkValue(previousTier))
            {
                return true;
            }

            return previousTier.AlternativePerk != null && hero.GetPerkValue(previousTier.AlternativePerk);
        }

        private static int ComparePerkPlanOrder(PerkObject left, PerkObject right)
        {
            int skillOrder = string.Compare(left.Skill.StringId, right.Skill.StringId, StringComparison.Ordinal);

            if (skillOrder != 0)
            {
                return skillOrder;
            }

            int tierOrder = left.RequiredSkillValue.CompareTo(right.RequiredSkillValue);

            if (tierOrder != 0)
            {
                return tierOrder;
            }

            return string.Compare(left.StringId, right.StringId, StringComparison.Ordinal);
        }

        private static int CompareLeftoverTarget(int current, int target, int pickedCurrent, int pickedTarget)
        {
            if (pickedTarget <= 0)
            {
                return 1;
            }

            int extra = System.Math.Max(0, current - target);
            int pickedExtra = System.Math.Max(0, pickedCurrent - pickedTarget);

            long nextWeightedExtra = (long)(extra + 1) * pickedTarget;
            long pickedNextWeightedExtra = (long)(pickedExtra + 1) * target;

            if (nextWeightedExtra < pickedNextWeightedExtra)
            {
                return 1;
            }

            if (nextWeightedExtra > pickedNextWeightedExtra)
            {
                return -1;
            }

            if (target < pickedTarget)
            {
                return 1;
            }

            if (target > pickedTarget)
            {
                return -1;
            }

            return 0;
        }

        private static int GetFloorTarget(int target, int limit)
        {
            return System.Math.Min(target, limit);
        }

        private static int GetSkillAttributePriority(SkillObject skill, List<KeyValuePair<CharacterAttribute, int>> attributeLine)
        {
            int priority = 0;

            foreach (CharacterAttribute skillAttribute in skill.Attributes)
            {
                for (int i = 0; i < attributeLine.Count; i++)
                {
                    if (attributeLine[i].Key == skillAttribute && attributeLine[i].Value > priority)
                    {
                        priority = attributeLine[i].Value;
                    }
                }
            }

            return priority;
        }
    }
}