using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper.PerkSelection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace ProperSkillDistributor
{
    // mimics CharacterDeveloper ui to preset screen by sandboxing an actual hero
    public static class PresetEditorSession
    {
        private static readonly FieldInfo _totalXpField = AccessTools.Field(typeof(HeroDeveloper), "_totalXp");
        private static readonly MethodInfo _setSkillXp = AccessTools.Method(typeof(HeroDeveloper), "SetSkillXp");
        private static readonly FieldInfo _heroPerks = AccessTools.Field(typeof(Hero), "_heroPerks");
        private static readonly FieldInfo _pendingPerks = AccessTools.Field(typeof(PerkSelectionVM), "_selectedPerks");

        private static SkillPresetBehavior _behavior;
        private static int _slotIndex;
        private static Action _closeScreen;
        private static List<HeroBeforePresetScreen> _heroesBeforePresetScreen;
        private static List<PerkObject> _perkChoicesForThisOpen;
        private static bool _restoreInProgress;

        public static bool IsActive { get; private set; }

        public static void Begin(SkillPresetBehavior behavior, int slotIndex, Action closeScreen)
        {
            RestoreIfActive();

            _behavior = behavior;
            _slotIndex = slotIndex;
            _closeScreen = closeScreen;
            _heroesBeforePresetScreen = new List<HeroBeforePresetScreen>();
            _perkChoicesForThisOpen = new List<PerkObject>();

            SkillPreset preset = behavior.GetPreset(slotIndex);

            // pending perk list is being kept in a private vm list. have to be patched after ctor
            if (preset != null && preset.IsMimicPreset)
            {
                Hero mimicHero = behavior.GetMimicSourceHero(preset);

                if (mimicHero != null)
                {
                    foreach (PerkObject perk in PerkObject.All)
                    {
                        if (perk.Skill != null && mimicHero.GetPerkValue(perk))
                        {
                            _perkChoicesForThisOpen.Add(perk);
                        }
                    }
                }
            }
            else if (preset != null)
            {
                foreach (string perkId in preset.SelectedPerkIds)
                {
                    PerkObject perk = MBObjectManager.Instance.GetObject<PerkObject>(perkId);

                    if (perk != null && perk.Skill != null)
                    {
                        _perkChoicesForThisOpen.Add(perk);
                    }
                }
            }

            _perkChoicesForThisOpen.Sort((left, right) =>
            {
                int skillComparison = string.Compare(left.Skill.StringId, right.Skill.StringId, StringComparison.Ordinal);

                if (skillComparison != 0)
                {
                    return skillComparison;
                }

                int levelComparison = left.RequiredSkillValue.CompareTo(right.RequiredSkillValue);

                if (levelComparison != 0)
                {
                    return levelComparison;
                }

                return string.Compare(left.StringId, right.StringId, StringComparison.Ordinal);
            });

            foreach (Hero hero in GetPresetEditorHeroes())
            {
                _heroesBeforePresetScreen.Add(new HeroBeforePresetScreen(hero));

                int fakeSkillLevelForPerkPicker = 300;
                int focusBudgetInPresetScreen = 100;

                Hero mimicHero = preset != null && preset.IsMimicPreset
                    ? _behavior.GetMimicSourceHero(preset)
                    : null;

                int maxAttribute = Campaign.Current.Models.CharacterDevelopmentModel.MaxAttribute;
                int spentAttributeTargets = 0;
                int spentFocusTargets = 0;

                foreach (CharacterAttribute attribute in Attributes.All)
                {
                    int wantedAttribute = mimicHero != null
                        ? mimicHero.GetAttributeValue(attribute)
                        : preset != null ? preset.GetAttributeTarget(attribute.StringId) : 0;

                    wantedAttribute = MBMath.ClampInt(wantedAttribute, 0, maxAttribute);
                    spentAttributeTargets += wantedAttribute;

                    int currentAttribute = hero.GetAttributeValue(attribute);

                    if (currentAttribute > wantedAttribute)
                    {
                        hero.HeroDeveloper.RemoveAttribute(attribute, currentAttribute - wantedAttribute);
                    }
                    else if (currentAttribute < wantedAttribute)
                    {
                        hero.HeroDeveloper.AddAttribute(attribute, wantedAttribute - currentAttribute, false);
                    }
                }

                foreach (SkillObject skill in Skills.All)
                {
                    hero.SetSkillValue(skill, fakeSkillLevelForPerkPicker);
                    _setSkillXp.Invoke(hero.HeroDeveloper, new object[]
                    {
                        (PropertyObject)skill,
                        Campaign.Current.Models.CharacterDevelopmentModel.GetXpRequiredForSkillLevel(fakeSkillLevelForPerkPicker)
                    });

                    int wantedFocus = mimicHero != null
                        ? mimicHero.HeroDeveloper.GetFocus(skill)
                        : preset != null ? preset.GetSkillFocusTarget(skill.StringId) : 0;

                    wantedFocus = MBMath.ClampInt(wantedFocus, 0, Campaign.Current.Models.CharacterDevelopmentModel.MaxFocusPerSkill);
                    spentFocusTargets += wantedFocus;

                    int currentFocus = hero.HeroDeveloper.GetFocus(skill);

                    if (currentFocus > wantedFocus)
                    {
                        hero.HeroDeveloper.RemoveFocus(skill, currentFocus - wantedFocus);
                    }
                    else if (currentFocus < wantedFocus)
                    {
                        hero.HeroDeveloper.AddFocus(skill, wantedFocus - currentFocus, false);
                    }
                }

                object perkOwner = _heroPerks.GetValue(hero);
                perkOwner.GetType().GetMethod("ClearAllProperty").Invoke(perkOwner, null);

                hero.HeroDeveloper.UnspentAttributePoints = Math.Max(0, Attributes.All.Count * maxAttribute - spentAttributeTargets);
                hero.HeroDeveloper.UnspentFocusPoints = Math.Max(0, focusBudgetInPresetScreen - spentFocusTargets);
                _totalXpField.SetValue(hero.HeroDeveloper, hero.HeroDeveloper.GetXpRequiredForLevel(hero.Level) + 1);
            }

            IsActive = true;
        }

        public static void Attach(CharacterDeveloperVM viewModel)
        {
            if (!IsActive || viewModel == null)
            {
                return;
            }

            foreach (CharacterDeveloperHeroItemVM heroItem in viewModel.HeroList)
            {
                List<PerkObject> pendingPerks = (List<PerkObject>)_pendingPerks.GetValue(heroItem.PerkSelection);

                pendingPerks.Clear();
                pendingPerks.AddRange(_perkChoicesForThisOpen);

                HashSet<SkillObject> refreshedSkills = new HashSet<SkillObject>();

                foreach (PerkObject perk in _perkChoicesForThisOpen)
                {
                    if (perk.Skill != null && refreshedSkills.Add(perk.Skill))
                    {
                        heroItem.RefreshPerksOfSkill(perk.Skill);
                    }
                }

                foreach (SkillVM skillVM in heroItem.Skills)
                {
                    skillVM.RefreshLists();
                }
            }

            viewModel.RefreshValues();
        }

        public static bool SaveAndClose(CharacterDeveloperVM viewModel)
        {
            if (!IsActive)
            {
                return true;
            }

            Action closeScreen = _closeScreen;
            SkillPresetBehavior behavior = _behavior;

            if (viewModel != null && viewModel.CurrentCharacter != null && _behavior != null)
            {
                CharacterDeveloperHeroItemVM heroItem = viewModel.CurrentCharacter;
                SkillPreset preset = _behavior.GetPreset(_slotIndex);

                string presetName = preset != null ? preset.Name : "Preset " + _slotIndex;

                Dictionary<string, int> attributeTargets = new Dictionary<string, int>();
                Dictionary<string, int> focusTargets = new Dictionary<string, int>();
                List<string> perkIds = new List<string>();

                foreach (CharacterAttributeItemVM attributeItem in heroItem.Attributes)
                {
                    if (attributeItem.AttributeValue > 0)
                    {
                        attributeTargets[attributeItem.AttributeType.StringId] = attributeItem.AttributeValue;
                    }
                }

                foreach (SkillVM skillItem in heroItem.Skills)
                {
                    if (skillItem.CurrentFocusLevel > 0)
                    {
                        focusTargets[skillItem.Skill.StringId] = skillItem.CurrentFocusLevel;
                    }
                }

                foreach (PerkObject perk in (List<PerkObject>)_pendingPerks.GetValue(heroItem.PerkSelection))
                {
                    if (!perkIds.Contains(perk.StringId))
                    {
                        perkIds.Add(perk.StringId);
                    }
                }

                _behavior.ConfigurePreset(_slotIndex, presetName, attributeTargets, focusTargets, perkIds);
            }

            RestoreIfActive();

            behavior?.ApplyAllAssignedPresets();

            closeScreen?.Invoke();
            return false;
        }

        public static bool CancelAndClose()
        {
            if (!IsActive)
            {
                return true;
            }

            Action closeScreen = _closeScreen;

            RestoreIfActive();

            closeScreen?.Invoke();
            return false;
        }

        public static void RestoreIfActive()
        {
            if (!IsActive || _restoreInProgress)
            {
                return;
            }

            _restoreInProgress = true;

            if (_heroesBeforePresetScreen != null)
            {
                foreach (HeroBeforePresetScreen heroBeforePresetScreen in _heroesBeforePresetScreen)
                {
                    heroBeforePresetScreen.Restore();
                }
            }

            _heroesBeforePresetScreen = null;
            _perkChoicesForThisOpen = null;
            _behavior = null;
            _slotIndex = 0;
            _closeScreen = null;
            IsActive = false;
            _restoreInProgress = false;
        }

        public static void ReapplyInitialPerks(CharacterDeveloperVM viewModel)
        {
            if (!IsActive || viewModel == null)
            {
                return;
            }

            foreach (CharacterDeveloperHeroItemVM heroItem in viewModel.HeroList)
            {
                List<PerkObject> pendingPerks = (List<PerkObject>)_pendingPerks.GetValue(heroItem.PerkSelection);

                pendingPerks.Clear();
                pendingPerks.AddRange(_perkChoicesForThisOpen);

                HashSet<SkillObject> refreshedSkills = new HashSet<SkillObject>();

                foreach (PerkObject perk in _perkChoicesForThisOpen)
                {
                    if (perk.Skill != null && refreshedSkills.Add(perk.Skill))
                    {
                        heroItem.RefreshPerksOfSkill(perk.Skill);
                    }
                }

                foreach (SkillVM skillVM in heroItem.Skills)
                {
                    skillVM.RefreshLists();
                }
            }
        }

        public static List<Hero> GetPresetEditorHeroes()
        {
            List<Hero> heroes = new List<Hero>();

            if (SkillPresetBehavior.IsPlayerDevelopableHero(Hero.MainHero))
            {
                heroes.Add(Hero.MainHero);
                return heroes;
            }

            if (Clan.PlayerClan == null)
            {
                return heroes;
            }

            foreach (Hero hero in Clan.PlayerClan.Heroes)
            {
                if (SkillPresetBehavior.IsPlayerDevelopableHero(hero))
                {
                    heroes.Add(hero);
                    return heroes;
                }
            }

            foreach (Hero hero in Clan.PlayerClan.Companions)
            {
                if (SkillPresetBehavior.IsPlayerDevelopableHero(hero))
                {
                    heroes.Add(hero);
                    return heroes;
                }
            }

            return heroes;
        }

        private class HeroBeforePresetScreen
        {
            private readonly Hero _hero;
            private readonly int _level;
            private readonly int _totalXp;
            private readonly int _unspentFocusPoints;
            private readonly int _unspentAttributePoints;
            private readonly Dictionary<CharacterAttribute, int> _attributes;
            private readonly Dictionary<SkillObject, int> _skills;
            private readonly Dictionary<SkillObject, float> _skillXps;
            private readonly Dictionary<SkillObject, int> _focuses;
            private readonly HashSet<PerkObject> _perks;

            public HeroBeforePresetScreen(Hero hero)
            {
                _hero = hero;
                _level = hero.Level;
                _totalXp = (int)_totalXpField.GetValue(hero.HeroDeveloper);
                _unspentFocusPoints = hero.HeroDeveloper.UnspentFocusPoints;
                _unspentAttributePoints = hero.HeroDeveloper.UnspentAttributePoints;

                _attributes = new Dictionary<CharacterAttribute, int>();
                _skills = new Dictionary<SkillObject, int>();
                _skillXps = new Dictionary<SkillObject, float>();
                _focuses = new Dictionary<SkillObject, int>();
                _perks = new HashSet<PerkObject>();

                foreach (CharacterAttribute attribute in Attributes.All)
                {
                    _attributes[attribute] = hero.GetAttributeValue(attribute);
                }

                foreach (SkillObject skill in Skills.All)
                {
                    _skills[skill] = hero.GetSkillValue(skill);
                    _skillXps[skill] = hero.HeroDeveloper.GetSkillXp(skill);
                    _focuses[skill] = hero.HeroDeveloper.GetFocus(skill);
                }

                foreach (PerkObject perk in PerkObject.All)
                {
                    if (hero.GetPerkValue(perk))
                    {
                        _perks.Add(perk);
                    }
                }
            }

            public void Restore()
            {
                // rollback point for editor

                _hero.Level = _level;
                _totalXpField.SetValue(_hero.HeroDeveloper, _totalXp);

                foreach (KeyValuePair<CharacterAttribute, int> attribute in _attributes)
                {
                    int currentAttribute = _hero.GetAttributeValue(attribute.Key);

                    if (currentAttribute > attribute.Value)
                    {
                        _hero.HeroDeveloper.RemoveAttribute(attribute.Key, currentAttribute - attribute.Value);
                    }
                    else if (currentAttribute < attribute.Value)
                    {
                        _hero.HeroDeveloper.AddAttribute(attribute.Key, attribute.Value - currentAttribute, false);
                    }
                }

                foreach (KeyValuePair<SkillObject, int> skill in _skills)
                {
                    _hero.SetSkillValue(skill.Key, skill.Value);
                }

                foreach (KeyValuePair<SkillObject, float> skillXp in _skillXps)
                {
                    _setSkillXp.Invoke(_hero.HeroDeveloper, new object[] { (PropertyObject)skillXp.Key, skillXp.Value });
                }

                foreach (KeyValuePair<SkillObject, int> focus in _focuses)
                {
                    int currentFocus = _hero.HeroDeveloper.GetFocus(focus.Key);

                    if (currentFocus > focus.Value)
                    {
                        _hero.HeroDeveloper.RemoveFocus(focus.Key, currentFocus - focus.Value);
                    }
                    else if (currentFocus < focus.Value)
                    {
                        _hero.HeroDeveloper.AddFocus(focus.Key, focus.Value - currentFocus, false);
                    }
                }

                object perkOwner = _heroPerks.GetValue(_hero);
                perkOwner.GetType().GetMethod("ClearAllProperty").Invoke(perkOwner, null);

                foreach (PerkObject perk in _perks)
                {
                    perkOwner.GetType().GetMethod("SetPropertyValue").Invoke(perkOwner, new object[] { perk, 1 });
                }

                _hero.HeroDeveloper.UnspentFocusPoints = _unspentFocusPoints;
                _hero.HeroDeveloper.UnspentAttributePoints = _unspentAttributePoints;
            }
        }
    }
}