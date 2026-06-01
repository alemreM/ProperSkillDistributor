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

            HeroDeveloper developer = hero.HeroDeveloper;
            Hero mimicHero = null;

            List<KeyValuePair<CharacterAttribute, int>> attributeLine = new List<KeyValuePair<CharacterAttribute, int>>();
            List<KeyValuePair<SkillObject, int>> focusLine = new List<KeyValuePair<SkillObject, int>>();
            List<PerkObject> perkLine = new List<PerkObject>();

            if (preset.IsMimicPreset)
            {
                // mimic source is live and interactive. hero skill gain is read each time target is applied
                mimicHero = SkillPresetBehavior.Current?.GetMimicSourceHero(preset);

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
                    }
                }
            }

            int maxAttribute = Campaign.Current.Models.CharacterDevelopmentModel.MaxAttribute;

            while (developer.UnspentAttributePoints > 0)
            {
                CharacterAttribute picked = null;
                int pickedWeight = 0;

                for (int pass = 0; pass < 2 && picked == null; pass++)
                {
                    if (pass == 1)
                    {
                        // after all minimum targets are reached continue with the target with most points assigned
                    }

                    for (int i = 0; i < attributeLine.Count; i++)
                    {
                        CharacterAttribute attribute = attributeLine[i].Key;
                        int target = attributeLine[i].Value;
                        int current = hero.GetAttributeValue(attribute);

                        if (current >= maxAttribute)
                        {
                            continue;
                        }

                        if (pass == 0 && current >= target)
                        {
                            continue;
                        }

                        if (target > pickedWeight)
                        {
                            picked = attribute;
                            pickedWeight = target;
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

            while (developer.UnspentFocusPoints > 0)
            {
                SkillObject picked = null;
                int pickedWeight = 0;

                for (int pass = 0; pass < 2 && picked == null; pass++)
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

                        if (pass == 0 && current >= target)
                        {
                            continue;
                        }

                        if (target > pickedWeight)
                        {
                            picked = skill;
                            pickedWeight = target;
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

            foreach (PerkObject perk in perkLine)
            {
                if (hero.GetPerkValue(perk))
                {
                    continue;
                }

                if (perk.AlternativePerk != null && hero.GetPerkValue(perk.AlternativePerk))
                {
                    continue;
                }

                if (hero.GetSkillValue(perk.Skill) < perk.RequiredSkillValue)
                {
                    continue;
                }

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

                if (previousTier != null)
                {
                    bool previousTierPicked = hero.GetPerkValue(previousTier);

                    if (!previousTierPicked && previousTier.AlternativePerk != null)
                    {
                        previousTierPicked = hero.GetPerkValue(previousTier.AlternativePerk);
                    }

                    if (!previousTierPicked)
                    {
                        continue;
                    }
                }

                developer.AddPerk(perk);
            }
        }
    }
}