using System.Runtime.CompilerServices;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace ProperSkillDistributor
{
    [ViewModelMixin("RefreshValues")]
    public class CharacterScreenPresetMixin : BaseViewModelMixin<CharacterDeveloperVM>
    {
        private static readonly ConditionalWeakTable<CharacterDeveloperVM, CharacterScreenPresetMixin> _registeredMixins =
            new ConditionalWeakTable<CharacterDeveloperVM, CharacterScreenPresetMixin>();

        private string _usePresetsText;
        private bool _isUsePresetDropdownOpen;
        private bool _isSkillPresetControlsVisible;

        public CharacterScreenPresetMixin(CharacterDeveloperVM viewModel)
            : base(viewModel)
        {
            _registeredMixins.Remove(viewModel);
            _registeredMixins.Add(viewModel, this);

            UsePresetRows = new MBBindingList<PresetAssignmentRowVM>();

            RefreshControlsVisibility();
            RefreshUsePresetsText();
            RefreshUsePresetRows();
        }

        [DataSourceProperty]
        public string UsePresetsText
        {
            get { return _usePresetsText; }
            set
            {
                if (value != _usePresetsText)
                {
                    _usePresetsText = value;
                    OnPropertyChangedWithValue(value, "UsePresetsText");
                }
            }
        }

        [DataSourceProperty]
        public bool IsUsePresetDropdownOpen
        {
            get { return _isUsePresetDropdownOpen; }
            set
            {
                if (value != _isUsePresetDropdownOpen)
                {
                    _isUsePresetDropdownOpen = value;
                    OnPropertyChangedWithValue(value, "IsUsePresetDropdownOpen");
                }
            }
        }

        [DataSourceProperty]
        public bool IsSkillPresetControlsVisible
        {
            get { return _isSkillPresetControlsVisible; }
            set
            {
                if (value != _isSkillPresetControlsVisible)
                {
                    _isSkillPresetControlsVisible = value;
                    OnPropertyChangedWithValue(value, "IsSkillPresetControlsVisible");
                }
            }
        }


        [DataSourceProperty]
        public string AddPresetsText
        {
            get { return new TextObject("{=add_presets}Add Presets").ToString(); }
        }

        [DataSourceProperty]
        public MBBindingList<PresetAssignmentRowVM> UsePresetRows { get; private set; }

        public override void OnRefresh()
        {
            RefreshControlsVisibility();
            RefreshUsePresetsText();

            if (IsUsePresetDropdownOpen)
            {
                RefreshUsePresetRows();
            }
        }

        public static void RefreshFor(CharacterDeveloperVM viewModel)
        {
            CharacterScreenPresetMixin mixin;

            if (viewModel != null && _registeredMixins.TryGetValue(viewModel, out mixin))
            {
                mixin.RefreshControlsVisibility();
                mixin.RefreshUsePresetsText();

                if (mixin.IsUsePresetDropdownOpen)
                {
                    mixin.RefreshUsePresetRows();
                }
            }
        }

        [DataSourceMethod]
        public void ExecuteOpenPresetEditorSelector()
        {
            CharacterScreenPresetActions.OpenPresetEditorSelector(ViewModel);
        }

        [DataSourceMethod]
        public void ExecuteOpenPresetAssignmentSelector()
        {
            // avoid opening another screen over CharacterDeveloper
            ExecuteToggleUsePresetDropdown();
        }

        [DataSourceMethod]
        public void ExecuteToggleUsePresetDropdown()
        {
            RefreshUsePresetsText();
            RefreshUsePresetRows();
            IsUsePresetDropdownOpen = !IsUsePresetDropdownOpen;
        }

        [DataSourceMethod]
        public void ExecuteCloseUsePresetDropdown()
        {
            IsUsePresetDropdownOpen = false;
        }

        public void SelectUsePreset(PresetAssignmentRowVM row)
        {
            Hero hero = ViewModel?.CurrentCharacter?.Hero;
            SkillPresetBehavior behavior = SkillPresetBehavior.Current;

            if (hero == null || behavior == null || row == null)
            {
                IsUsePresetDropdownOpen = false;
                return;
            }

            if (row.SlotIndex == 0)
            {
                behavior.ClearPresetAssignment(hero);
            }
            else
            {
                behavior.AssignPresetToHero(hero, row.SlotIndex);
            }

            IsUsePresetDropdownOpen = false;
            RefreshUsePresetsText();
            RefreshUsePresetRows();
        }
        private void RefreshControlsVisibility()
        {
            IsSkillPresetControlsVisible = !PresetEditorSession.IsActive;

            if (!IsSkillPresetControlsVisible)
            {
                IsUsePresetDropdownOpen = false;
            }
        }

        private void RefreshUsePresetsText()
        {
            Hero hero = ViewModel?.CurrentCharacter?.Hero;
            SkillPresetBehavior behavior = SkillPresetBehavior.Current;

            if (hero == null || behavior == null)
            {
                UsePresetsText = new TextObject("{=use_presets_none}Use Presets: None").ToString();
                return;
            }

            int assignedSlot = behavior.GetAssignedPresetSlot(hero);

            if (assignedSlot == 0)
            {
                UsePresetsText = new TextObject("{=use_presets_none}Use Presets: None").ToString();
                return;
            }

            SkillPreset preset = behavior.GetPreset(assignedSlot);
            if (preset != null)
            {
                var usePresetName = new TextObject("{=use_presets_name}Use Presets: {PRESET_NAME}");
                usePresetName.SetTextVariable("PRESET_NAME", preset.Name);
                UsePresetsText = usePresetName.ToString();
                return;
            }

            var usePresetSlot = new TextObject("{=use_presets_slot}Use Presets: Preset {SLOT_INDEX}");
            usePresetSlot.SetTextVariable("SLOT_INDEX", assignedSlot);
            UsePresetsText = usePresetSlot.ToString();
        }

        private void RefreshUsePresetRows()
        {
            Hero hero = ViewModel?.CurrentCharacter?.Hero;
            SkillPresetBehavior behavior = SkillPresetBehavior.Current;

            UsePresetRows.Clear();

            if (hero == null || behavior == null)
            {
                UsePresetRows.Add(new PresetAssignmentRowVM(this, 0, new TextObject("{=none}None").ToString(), new TextObject("{=no_active_hero}No active hero.").ToString(), true));
                return;
            }

            int assignedSlot = behavior.GetAssignedPresetSlot(hero);
            List<SkillPreset> presets = behavior.GetPresets();

            if (MBSaveLoad.CurrentVersion.IsOlderThan(ApplicationVersion.FromString("v1.4.0", 0)))
            {
                UsePresetRows.Add(new PresetAssignmentRowVM(
                    this,
                    0,
                    new TextObject("{=none}None").ToString(),
                    new TextObject("{=do_not_use_preset}Do not use a preset for this character.").ToString(),
                    assignedSlot == 0));

                foreach (SkillPreset preset in presets)
                {
                    UsePresetRows.Add(new PresetAssignmentRowVM(
                        this,
                        preset.SlotIndex,
                        preset.Name,
                        preset.IsConfigured ? new TextObject("{=configured}Configured").ToString() : new TextObject("{=not_configured}Not configured").ToString(),
                        assignedSlot == preset.SlotIndex));
                }
            }
            else
            {
                for (int i = presets.Count - 1; i >= 0; i--)
                {
                    SkillPreset preset = presets[i];

                    UsePresetRows.Add(new PresetAssignmentRowVM(
                        this,
                        preset.SlotIndex,
                        preset.Name,
                        preset.IsConfigured ? new TextObject("{=configured}Configured").ToString() : new TextObject("{=not_configured}Not configured").ToString(),
                        assignedSlot == preset.SlotIndex));
                }

                UsePresetRows.Add(new PresetAssignmentRowVM(
                    this,
                    0,
                    new TextObject("{=none}None").ToString(),
                    new TextObject("{=do_not_use_preset}Do not use a preset for this character.").ToString(),
                    assignedSlot == 0));
            }
        }
    }
}
