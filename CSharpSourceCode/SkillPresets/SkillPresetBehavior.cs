using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace ProperSkillDistributor
{
    public class SkillPresetBehavior : CampaignBehaviorBase
    {
        [SaveableField(1)]
        private List<SkillPreset> _presets;

        [SaveableField(2)]
        private Dictionary<string, int> _heroPresetAssignments;

        [SaveableField(3)]
        private int _focusFloorLimit; // floor is the minimum target value for every skill with points assigned (over floor value) before going for the maximum target points for any skill. probably not the best name for it

        [SaveableField(4)]
        private int _attributeFloorLimit;

        [SaveableField(5)]
        private bool _floorLimitsInitialized;

        [SaveableField(6)]
        private bool _spendLeftoverPoints;

        [SaveableField(7)]
        private bool _spendLeftoverPointsInitialized;

        [SaveableField(8)]
        private bool _randomizeUnpickedPerks;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.HeroLevelledUp.AddNonSerializedListener(this, OnHeroLevelledUp);
            CampaignEvents.HeroGainedSkill.AddNonSerializedListener(this, OnHeroGainedSkill);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_skillPresets", ref _presets);
            dataStore.SyncData("_skillPresetAssignments", ref _heroPresetAssignments);
            dataStore.SyncData("_focusFloorLimit", ref _focusFloorLimit);
            dataStore.SyncData("_attributeFloorLimit", ref _attributeFloorLimit);
            dataStore.SyncData("_floorLimitsInitialized", ref _floorLimitsInitialized);
            dataStore.SyncData("_spendLeftoverPoints", ref _spendLeftoverPoints);
            dataStore.SyncData("_spendLeftoverPointsInitialized", ref _spendLeftoverPointsInitialized);
            dataStore.SyncData("_randomizeUnpickedPerks", ref _randomizeUnpickedPerks);

            RepairPresetSlotsAfterLoad();
        }

        public List<SkillPreset> GetPresets()
        {
            RepairPresetSlotsAfterLoad();
            return _presets;
        }

        public SkillPreset GetPreset(int slotIndex)
        {
            RepairPresetSlotsAfterLoad();
            return FindPresetInCurrentSlots(slotIndex);
        }

        public int FocusFloorLimit
        {
            get { return _focusFloorLimit; }
        }

        public int AttributeFloorLimit
        {
            get { return _attributeFloorLimit; }
        }

        public bool SpendLeftoverPoints
        {
            get { return _spendLeftoverPoints; }
        }

        public bool RandomizeUnpickedPerks
        {
            get { return _randomizeUnpickedPerks; }
        }

        public void ToggleSpendLeftoverPoints()
        {
            _spendLeftoverPoints = !_spendLeftoverPoints;
        }

        public void ToggleRandomizeUnpickedPerks()
        {
            _randomizeUnpickedPerks = !_randomizeUnpickedPerks;
        }

        public void IncreaseFocusFloorLimit()
        {
            if (_focusFloorLimit < 5) _focusFloorLimit++;
        }

        public void DecreaseFocusFloorLimit()
        {
            if (_focusFloorLimit > 0) _focusFloorLimit--;
        }

        public void IncreaseAttributeFloorLimit()
        {
            if (_attributeFloorLimit < 10) _attributeFloorLimit++;
        }

        public void DecreaseAttributeFloorLimit()
        {
            if (_attributeFloorLimit > 0) _attributeFloorLimit--;
        }

        public int GetAssignedPresetSlot(Hero hero)
        {
            RepairPresetSlotsAfterLoad();

            if (!IsPlayerDevelopableHero(hero))
            {
                return 0;
            }

            int slotIndex;
            return _heroPresetAssignments.TryGetValue(hero.StringId, out slotIndex) ? slotIndex : 0;
        }

        public void AssignPresetToHero(Hero hero, int slotIndex)
        {
            RepairPresetSlotsAfterLoad();

            if (!IsPlayerDevelopableHero(hero))
            {
                return;
            }

            if (slotIndex < 1 || slotIndex > 9)
            {
                ClearPresetAssignment(hero);
                return;
            }

            _heroPresetAssignments[hero.StringId] = slotIndex;
        }

        public void ClearPresetAssignment(Hero hero)
        {
            RepairPresetSlotsAfterLoad();

            if (hero == null || string.IsNullOrEmpty(hero.StringId))
            {
                return;
            }

            _heroPresetAssignments.Remove(hero.StringId);
        }

        public void RenamePreset(int slotIndex, string name)
        {
            RepairPresetSlotsAfterLoad();

            SkillPreset preset = FindPresetInCurrentSlots(slotIndex);

            if (preset != null)
            {
                preset.Rename(name);
            }
        }

        public void ClearPreset(int slotIndex)
        {
            RepairPresetSlotsAfterLoad();

            SkillPreset preset = FindPresetInCurrentSlots(slotIndex);

            if (preset != null)
            {
                preset.Clear();
            }
        }

        public void ConfigurePreset(
            int slotIndex,
            string name,
            Dictionary<string, int> attributeTargets,
            Dictionary<string, int> skillFocusTargets,
            List<string> selectedPerkIds)
        {
            RepairPresetSlotsAfterLoad();

            SkillPreset preset = FindPresetInCurrentSlots(slotIndex);

            if (preset != null)
            {
                preset.Configure(name, attributeTargets, skillFocusTargets, selectedPerkIds);
            }
        }

        public void ConfigurePresetAsMimic(int slotIndex, Hero mimicHero)
        {
            RepairPresetSlotsAfterLoad();

            SkillPreset preset = FindPresetInCurrentSlots(slotIndex);

            if (preset == null || mimicHero == null)
            {
                return;
            }

            preset.ConfigureAsMimic(mimicHero.StringId, mimicHero.Name.ToString());
        }

        public void ApplyAssignedPresetToHero(Hero hero)
        {
            RepairPresetSlotsAfterLoad();

            if (!IsPlayerDevelopableHero(hero))
            {
                return;
            }

            int slotIndex;
            if (!_heroPresetAssignments.TryGetValue(hero.StringId, out slotIndex))
            {
                return;
            }

            SkillPreset preset = FindPresetInCurrentSlots(slotIndex);

            if (preset == null || !preset.IsConfigured)
            {
                return;
            }

            SkillPresetAllocator.ApplyPreset(hero, preset);
        }

        public void ApplyAllAssignedPresets()
        {
            RepairPresetSlotsAfterLoad();

            if (Clan.PlayerClan == null)
            {
                return;
            }

            foreach (Hero hero in LiveClanSkillRoster())
            {
                if (!_heroPresetAssignments.ContainsKey(hero.StringId))
                {
                    continue;
                }

                ApplyAssignedPresetToHero(hero);
            }
        }

        public Hero GetMimicSourceHero(SkillPreset preset)
        {
            RepairPresetSlotsAfterLoad();

            if (preset == null || !preset.IsMimicPreset)
            {
                return null;
            }

            foreach (Hero hero in GetPlayerFamilyAndCompanionsForMimic())
            {
                if (hero.StringId == preset.MimicHeroStringId)
                {
                    return hero;
                }
            }

            return null;
        }

        public IEnumerable<Hero> GetPlayerFamilyAndCompanionsForMimic()
        {
            RepairPresetSlotsAfterLoad();

            if (Clan.PlayerClan == null)
            {
                yield break;
            }

            HashSet<string> seenHeroes = new HashSet<string>();

            if (CanShowUpInMimicList(Hero.MainHero) && seenHeroes.Add(Hero.MainHero.StringId))
            {
                yield return Hero.MainHero;
            }

            foreach (Hero hero in Clan.PlayerClan.Heroes)
            {
                if (!CanShowUpInMimicList(hero) || !seenHeroes.Add(hero.StringId))
                {
                    continue;
                }

                yield return hero;
            }

            foreach (Hero hero in Clan.PlayerClan.Companions)
            {
                if (!CanShowUpInMimicList(hero) || !seenHeroes.Add(hero.StringId))
                {
                    continue;
                }

                yield return hero;
            }
        }

        public static bool IsPlayerDevelopableHero(Hero hero)
        {
            if (hero == null || hero.HeroState == Hero.CharacterStates.Disabled || !hero.IsAlive || hero.IsChild || string.IsNullOrEmpty(hero.StringId))
            {
                return false;
            }

            if (Clan.PlayerClan == null)
            {
                return false;
            }

            return Clan.PlayerClan.Heroes.Contains(hero) || Clan.PlayerClan.Companions.Contains(hero);
        }

        public static SkillPresetBehavior Current
        {
            get
            {
                return Campaign.Current == null
                    ? null
                    : Campaign.Current.GetCampaignBehavior<SkillPresetBehavior>();
            }
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            RepairPresetSlotsAfterLoad();
            TaleWorlds.CampaignSystem.CampaignOptions.AutoAllocateClanMemberPerks = false;
        }

        private void OnHeroLevelledUp(Hero hero, bool shouldNotify)
        {
            ApplyAssignedPresetToHero(hero);
        }

        private void OnHeroGainedSkill(Hero hero, SkillObject skill, int change, bool shouldNotify)
        {
            ApplyAssignedPresetToHero(hero);
        }

        private static IEnumerable<Hero> LiveClanSkillRoster()
        {
            HashSet<string> seenHeroes = new HashSet<string>();

            foreach (Hero hero in Clan.PlayerClan.Heroes)
            {
                if (!IsPlayerDevelopableHero(hero) || !seenHeroes.Add(hero.StringId))
                {
                    continue;
                }

                yield return hero;
            }

            foreach (Hero hero in Clan.PlayerClan.Companions)
            {
                if (!IsPlayerDevelopableHero(hero) || !seenHeroes.Add(hero.StringId))
                {
                    continue;
                }

                yield return hero;
            }
        }

        private static bool CanShowUpInMimicList(Hero hero)
        {
            return hero != null
                && hero.HeroState != Hero.CharacterStates.Disabled
                && hero.IsAlive
                && !hero.IsChild
                && !string.IsNullOrEmpty(hero.StringId);
        }

        private SkillPreset FindPresetInCurrentSlots(int slotIndex)
        {
            for (int i = 0; i < _presets.Count; i++)
            {
                if (_presets[i].SlotIndex == slotIndex)
                {
                    return _presets[i];
                }
            }

            return null;
        }

        private void RepairPresetSlotsAfterLoad()
        {
            _presets = _presets ?? new List<SkillPreset>();
            _heroPresetAssignments = _heroPresetAssignments ?? new Dictionary<string, int>();

            if (!_floorLimitsInitialized)
            {
                _focusFloorLimit = 5;
                _attributeFloorLimit = 10;
                _floorLimitsInitialized = true;
            }

            if (!_spendLeftoverPointsInitialized)
            {
                _spendLeftoverPoints = true;
                _spendLeftoverPointsInitialized = true;
            }

            _focusFloorLimit = System.Math.Max(0, System.Math.Min(5, _focusFloorLimit));
            _attributeFloorLimit = System.Math.Max(0, System.Math.Min(10, _attributeFloorLimit));

            _presets.RemoveAll(preset => preset == null);

            for (int slotIndex = 1; slotIndex <= 9; slotIndex++)
            {
                if (FindPresetInCurrentSlots(slotIndex) != null)
                {
                    continue;
                }
                _presets.Add(new SkillPreset(slotIndex));
            }

            _presets.Sort(delegate (SkillPreset left, SkillPreset right)
            {
                return left.SlotIndex.CompareTo(right.SlotIndex);
            });

            foreach (SkillPreset preset in _presets)
            {
                preset.RebuildAfterLoad();
            }
        }
    }
}