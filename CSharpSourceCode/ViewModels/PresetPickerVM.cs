using Bannerlord.UIExtenderEx.Attributes;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
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
        private string _focusFloorText;
        private string _attributeFloorText;

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

        [DataSourceProperty]
        public string FocusFloorText
        {
            get { return _focusFloorText; }
            set
            {
                if (value != _focusFloorText)
                {
                    _focusFloorText = value;
                    OnPropertyChangedWithValue(value, "FocusFloorText");
                }
            }
        }

        [DataSourceProperty]
        public string AttributeFloorText
        {
            get { return _attributeFloorText; }
            set
            {
                if (value != _attributeFloorText)
                {
                    _attributeFloorText = value;
                    OnPropertyChangedWithValue(value, "AttributeFloorText");
                }
            }
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

            RefreshFloorText();
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

        [DataSourceMethod]
        public void ExecuteDecreaseFocusFloor()
        {
            _presets.DecreaseFocusFloorLimit();
            RefreshFloorText();
        }

        [DataSourceMethod]
        public void ExecuteIncreaseFocusFloor()
        {
            _presets.IncreaseFocusFloorLimit();
            RefreshFloorText();
        }

        [DataSourceMethod]
        public void ExecuteDecreaseAttributeFloor()
        {
            _presets.DecreaseAttributeFloorLimit();
            RefreshFloorText();
        }

        [DataSourceMethod]
        public void ExecuteIncreaseAttributeFloor()
        {
            _presets.IncreaseAttributeFloorLimit();
            RefreshFloorText();
        }

        [DataSourceMethod]
        public void ExecuteShowFocusFloorHint()
        {
            MBInformationManager.ShowHint(GetFocusFloorHint());
        }

        [DataSourceMethod]
        public void ExecuteShowAttributeFloorHint()
        {
            MBInformationManager.ShowHint(GetAttributeFloorHint());
        }

        [DataSourceMethod]
        public void ExecuteHideFloorHint()
        {
            MBInformationManager.HideInformations();
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

        public void TemplatePreset(PresetPickerRowVM row)
        {
            if (row == null || row.SlotIndex == 0)
            {
                return;
            }

            List<TemplatePreset> templates = PredefinedTemplateCatalog.GetTemplates();

            if (templates.Count == 0)
            {
                string message = string.IsNullOrEmpty(PredefinedTemplateCatalog.LoadError)
                    ? "No skill preset templates were found."
                    : PredefinedTemplateCatalog.LoadError;

                InformationManager.DisplayMessage(new InformationMessage(message));
                return;
            }

            List<InquiryElement> elements = new List<InquiryElement>();

            foreach (TemplatePreset template in templates)
            {
                string hint = string.IsNullOrEmpty(template.Description)
                    ? "Copy this template into " + row.NameText + "."
                    : template.Description;

                elements.Add(new InquiryElement(
                    template,
                    template.Name,
                    null,
                    true,
                    hint));
            }

            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    "Templates",
                    "Select a built-in template to copy into " + row.NameText + ".",
                    elements,
                    true,
                    1,
                    1,
                    "Copy",
                    "Cancel",
                    pickedTemplates => UseTemplate(row, pickedTemplates),
                    null));
        }

        private string GetFocusFloorHint()
        {
			return "Focus floor. Assigned skills are filled to this value before the preset starts targeting other skills with higher values (if their target value is over the floor value). Use - / + to change it.";
		}

        private string GetAttributeFloorHint()
        {
            return "Attribute floor. Assigned attributes are filled to this value before the preset starts targeting other skills with higher values (if their target value is over the floor value). Use - / + to change it.";
        }

        private void RefreshFloorText()
        {
            FocusFloorText = "Focus " + _presets.FocusFloorLimit;
            AttributeFloorText = "Attr " + _presets.AttributeFloorLimit;
        }

        private void FillPresetSlots()
        {
            TitleText = "Add Presets";
            DescriptionText = "Pick a preset slot to edit. Slots can be renamed, cleared, mimiced, or filled from templates.";

            List<SkillPreset> presets = _presets.GetPresets();
            PresetPickerRowVM firstPresetRow = null;

            if (MBSaveLoad.CurrentVersion.IsOlderThan(ApplicationVersion.FromString("v1.4.0", 0)))
            {
                foreach (SkillPreset preset in presets)
                {
                    PresetPickerRowVM row = new PresetPickerRowVM(this, preset, true);
                    PresetRows.Add(row);

                    if (preset.SlotIndex == 1)
                    {
                        firstPresetRow = row;
                    }
                }
            }
            else
            {
                for (int i = presets.Count - 1; i >= 0; i--)
                {
                    SkillPreset preset = presets[i];
                    PresetPickerRowVM row = new PresetPickerRowVM(this, preset, true);
                    PresetRows.Add(row);

                    if (preset.SlotIndex == 1)
                    {
                        firstPresetRow = row;
                    }
                }
            }

            if (firstPresetRow != null)
            {
                SelectRow(firstPresetRow);
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

        private void UseTemplate(PresetPickerRowVM row, List<InquiryElement> pickedTemplates)
        {
            if (pickedTemplates == null || pickedTemplates.Count == 0)
            {
                return;
            }

            TemplatePreset template = pickedTemplates[0].Identifier as TemplatePreset;

            if (template == null)
            {
                return;
            }

            _presets.ConfigurePreset(
                row.SlotIndex,
                template.Name,
                GetAvailableAttributeTargets(template),
                GetAvailableSkillFocusTargets(template),
                GetAvailablePerkIds(template));

            SkillPreset preset = _presets.GetPreset(row.SlotIndex);

            if (preset != null)
            {
                row.RefreshFromPreset(preset);
            }

            SelectRow(row);
        }
        private static Dictionary<string, int> GetAvailableAttributeTargets(TemplatePreset template)
        {
            Dictionary<string, int> availableTargets = new Dictionary<string, int>();

            foreach (CharacterAttribute attribute in Attributes.All)
            {
                int target;

                if (template.AttributeTargets.TryGetValue(attribute.StringId, out target) && target > 0)
                {
                    availableTargets[attribute.StringId] = target;
                }
            }

            return availableTargets;
        }

        private static Dictionary<string, int> GetAvailableSkillFocusTargets(TemplatePreset template)
        {
            Dictionary<string, int> availableTargets = new Dictionary<string, int>();

            foreach (SkillObject skill in Skills.All)
            {
                int target;

                if (template.SkillFocusTargets.TryGetValue(skill.StringId, out target) && target > 0)
                {
                    availableTargets[skill.StringId] = target;
                }
            }

            return availableTargets;
        }

        private static List<string> GetAvailablePerkIds(TemplatePreset template)
        {
            List<string> availablePerkIds = new List<string>();
            HashSet<string> addedPerkIds = new HashSet<string>();

            foreach (PerkObject perk in PerkObject.All)
            {
                if (perk.Skill == null || string.IsNullOrEmpty(perk.StringId))
                {
                    continue;
                }

                if (template.SelectedPerkIds.Contains(perk.StringId) && addedPerkIds.Add(perk.StringId))
                {
                    availablePerkIds.Add(perk.StringId);
                }
            }

            return availablePerkIds;
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