using Bannerlord.UIExtenderEx.Attributes;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace ProperSkillDistributor
{
    public class PresetPickerVM : ViewModel
    {
        private readonly SkillPresetBehavior _presets;
        private readonly Action _leaveAddPresets;
        private readonly Action<int> _openEditorForSlot;

        private PresetPickerRowVM _selectedRow;
        private string _titleText;
        private string _descriptionText;

        [DataSourceProperty]
        public MBBindingList<PresetPickerRowVM> PresetRows { get; private set; }

        [DataSourceProperty]
        public string TitleText
        {
            get { return _titleText; }
            set
            {
                _titleText = value;
                OnPropertyChangedWithValue(value, "TitleText");
            }
        }

        [DataSourceProperty]
        public string DescriptionText
        {
            get { return _descriptionText; }
            set
            {
                _descriptionText = value;
                OnPropertyChangedWithValue(value, "DescriptionText");
            }
        }

        [DataSourceProperty]
        public bool IsEditMode
        {
            get { return true; }
        }

        public PresetPickerVM(
            SkillPresetBehavior presets,
            Action leaveAddPresets,
            Action<int> openEditorForSlot)
        {
            _presets = presets;
            _leaveAddPresets = leaveAddPresets;
            _openEditorForSlot = openEditorForSlot;

            PresetRows = new MBBindingList<PresetPickerRowVM>();

            FillPresetSlots();
        }

        [DataSourceMethod]
        public void ExecuteCancel()
        {
            _leaveAddPresets();
        }

        [DataSourceMethod]
        public void ExecuteContinue()
        {
            _leaveAddPresets();
        }

        [DataSourceMethod]
        public void ExecuteEdit()
        {
            if (_selectedRow == null || _selectedRow.SlotIndex == 0)
            {
                return;
            }

            _openEditorForSlot(_selectedRow.SlotIndex);
        }

        public void SelectRow(PresetPickerRowVM row)
        {
            if (_selectedRow != null)
            {
                _selectedRow.IsSelected = false;
            }

            _selectedRow = row;

            if (_selectedRow != null)
            {
                _selectedRow.IsSelected = true;
            }
        }

        public void RenamePreset(PresetPickerRowVM row)
        {
            if (row == null || row.SlotIndex == 0)
            {
                return;
            }

            InformationManager.ShowTextInquiry(
                new TextInquiryData(
                    "Rename Preset",
                    "Enter preset name.",
                    true,
                    true,
                    "Save",
                    "Cancel",
                    name => RenameSlot(row, name),
                    null,
                    false,
                    CheckPresetName,
                    string.Empty,
                    row.NameText),
                false,
                false);
        }

        public void ClearPreset(PresetPickerRowVM row)
        {
            if (row == null || row.SlotIndex == 0)
            {
                return;
            }

            InformationManager.ShowInquiry(
                new InquiryData(
                    "Clear Preset",
                    "Clear all skillset targets from " + row.NameText + "?",
                    true,
                    true,
                    "Clear",
                    "Cancel",
                    () => ClearSlot(row),
                    null),
                false,
                false);
        }

        public void MimicPreset(PresetPickerRowVM row)
        {
            if (row == null || row.SlotIndex == 0)
            {
                return;
            }

            List<InquiryElement> heroes = new List<InquiryElement>();

            foreach (Hero hero in _presets.GetPlayerFamilyAndCompanionsForMimic())
            {
                heroes.Add(new InquiryElement(
                    hero,
                    hero.Name.ToString(),
                    null,
                    true,
                    "Use this characters current skillset as a dynamic preset source."));
            }

            if (heroes.Count == 0)
            {
                InformationManager.DisplayMessage(new InformationMessage("No available clan member found to mimic preset."));
                return;
            }

            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    "Mimic Skillset",
                    "Select a clan member. This preset will dynamically copy that characters skillset.",
                    heroes,
                    true,
                    1,
                    1,
                    "Select",
                    "Cancel",
                    pickedHeroes => UseMimicSource(row, pickedHeroes),
                    null));
        }

        private void FillPresetSlots()
        {
            TitleText = "Add Presets";
            DescriptionText = "Pick a preset slot to edit. Slots can be renamed, cleared and mimiced dynamically copying another character.";

            foreach (SkillPreset preset in _presets.GetPresets())
            {
                PresetPickerRowVM row = new PresetPickerRowVM(this, preset, true);
                PresetRows.Add(row);

                if (_selectedRow == null)
                {
                    SelectRow(row);
                }
            }
        }

        private void RenameSlot(PresetPickerRowVM row, string name)
        {
            _presets.RenamePreset(row.SlotIndex, name);

            SkillPreset preset = _presets.GetPreset(row.SlotIndex);

            if (preset != null)
            {
                row.RefreshFromPreset(preset);
            }
        }

        private void ClearSlot(PresetPickerRowVM row)
        {
            _presets.ClearPreset(row.SlotIndex);

            SkillPreset preset = _presets.GetPreset(row.SlotIndex);

            if (preset != null)
            {
                row.RefreshFromPreset(preset);
            }
        }

        private void UseMimicSource(PresetPickerRowVM row, List<InquiryElement> pickedHeroes)
        {
            if (pickedHeroes == null || pickedHeroes.Count == 0)
            {
                return;
            }

            Hero mimicHero = pickedHeroes[0].Identifier as Hero;

            if (mimicHero == null)
            {
                return;
            }

            _presets.ConfigurePresetAsMimic(row.SlotIndex, mimicHero);

            SkillPreset preset = _presets.GetPreset(row.SlotIndex);

            if (preset != null)
            {
                row.RefreshFromPreset(preset);
            }

            SelectRow(row);
        }

        private static Tuple<bool, string> CheckPresetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new Tuple<bool, string>(false, "Preset name cannot be empty.");
            }

            return new Tuple<bool, string>(true, string.Empty);
        }
    }
}