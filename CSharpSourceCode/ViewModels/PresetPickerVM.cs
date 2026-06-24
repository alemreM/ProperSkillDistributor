using Bannerlord.UIExtenderEx.Attributes;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace ProperSkillDistributor
{
    public class PresetPickerVM : ViewModel
    {
        private enum PickerHover
        {
            None,
            Cancel,
            Edit,
            Continue,
            LeftoverToggle,
            RandomPerksToggle,
            FocusFloorMinus,
            FocusFloorPlus,
            AttributeFloorMinus,
            AttributeFloorPlus
        }

        private readonly SkillPresetBehavior _presets;
        private readonly Action _leaveAddPresets;
        private readonly Action<int> _openEditorForSlot;
        private readonly List<PresetSlotSnapshot> _presetsBeforeAddScreen;

        private PresetPickerRowVM _selectedRow;
        private string _titleText;
        private string _descriptionText;
        private string _focusFloorText;
        private string _attributeFloorText;
        private string _footerCancelText;
        private string _footerEditText;
        private string _footerContinueText;
        private string _leftoverToggleText;
        private string _randomPerksToggleText;
        private PickerHover _hoveredPart;
        private bool _spendLeftoverPoints;
        private bool _randomizeUnpickedPerks;

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
        public string CancelText
        {
            get { return _footerCancelText; }
            set
            {
                if (value != _footerCancelText)
                {
                    _footerCancelText = value;
                    OnPropertyChangedWithValue(value, "CancelText");
                }
            }
        }

        [DataSourceProperty]
        public string EditText
        {
            get { return _footerEditText; }
            set
            {
                if (value != _footerEditText)
                {
                    _footerEditText = value;
                    OnPropertyChangedWithValue(value, "EditText");
                }
            }
        }

        [DataSourceProperty]
        public string ContinueText
        {
            get { return _footerContinueText; }
            set
            {
                if (value != _footerContinueText)
                {
                    _footerContinueText = value;
                    OnPropertyChangedWithValue(value, "ContinueText");
                }
            }
        }

        [DataSourceProperty]
        public string SpendLeftoverPointsText
        {
            get { return _leftoverToggleText; }
            set
            {
                if (value != _leftoverToggleText)
                {
                    _leftoverToggleText = value;
                    OnPropertyChangedWithValue(value, "SpendLeftoverPointsText");
                }
            }
        }

        [DataSourceProperty]
        public string RandomUnpickedPerksText
        {
            get { return _randomPerksToggleText; }
            set
            {
                if (value != _randomPerksToggleText)
                {
                    _randomPerksToggleText = value;
                    OnPropertyChangedWithValue(value, "RandomUnpickedPerksText");
                }
            }
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

        [DataSourceProperty]
        public bool IsCancelHovered
        {
            get { return _hoveredPart == PickerHover.Cancel; }
        }

        [DataSourceProperty]
        public bool IsEditHovered
        {
            get { return _hoveredPart == PickerHover.Edit; }
        }

        [DataSourceProperty]
        public bool IsContinueHovered
        {
            get { return _hoveredPart == PickerHover.Continue; }
        }

        [DataSourceProperty]
        public bool IsSpendLeftoverHovered
        {
            get { return _hoveredPart == PickerHover.LeftoverToggle; }
        }

        [DataSourceProperty]
        public bool IsRandomUnpickedPerksHovered
        {
            get { return _hoveredPart == PickerHover.RandomPerksToggle; }
        }

        [DataSourceProperty]
        public bool IsDecreaseFocusFloorHovered
        {
            get { return _hoveredPart == PickerHover.FocusFloorMinus; }
        }

        [DataSourceProperty]
        public bool IsIncreaseFocusFloorHovered
        {
            get { return _hoveredPart == PickerHover.FocusFloorPlus; }
        }

        [DataSourceProperty]
        public bool IsDecreaseAttributeFloorHovered
        {
            get { return _hoveredPart == PickerHover.AttributeFloorMinus; }
        }

        [DataSourceProperty]
        public bool IsIncreaseAttributeFloorHovered
        {
            get { return _hoveredPart == PickerHover.AttributeFloorPlus; }
        }

        [DataSourceProperty]
        public bool SpendLeftoverPoints
        {
            get { return _spendLeftoverPoints; }
            set
            {
                if (value != _spendLeftoverPoints)
                {
                    _spendLeftoverPoints = value;
                    OnPropertyChangedWithValue(value, "SpendLeftoverPoints");
                }
            }
        }

        [DataSourceProperty]
        public bool RandomizeUnpickedPerks
        {
            get { return _randomizeUnpickedPerks; }
            set
            {
                if (value != _randomizeUnpickedPerks)
                {
                    _randomizeUnpickedPerks = value;
                    OnPropertyChangedWithValue(value, "RandomizeUnpickedPerks");
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
            _presetsBeforeAddScreen = TakePresetSnapshot();

            CancelText = new TextObject("{=cancel}Cancel").ToString();
            EditText = new TextObject("{=edit}Edit").ToString();
            ContinueText = new TextObject("{=continue}Continue").ToString();
            SpendLeftoverPointsText = new TextObject("{=spend_leftover_points}Spend leftover points :").ToString();
            RandomUnpickedPerksText = new TextObject("{=random_unpicked_perks}Randomize unpicked perks :").ToString();

            RefreshFloorText();
            RefreshRandomizeUnpickedPerks();
            RefreshSpendLeftoverPoints();
            FillPresetSlots();
        }

        [DataSourceMethod]
        public void ExecuteCancel()
        {
            RestorePresetSnapshot();
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
        public void ExecuteHoverCancelButton()
        {
            SetHoveredPart(PickerHover.Cancel);
        }

        [DataSourceMethod]
        public void ExecuteHoverEditButton()
        {
            SetHoveredPart(PickerHover.Edit);
        }

        [DataSourceMethod]
        public void ExecuteHoverContinueButton()
        {
            SetHoveredPart(PickerHover.Continue);
        }

        [DataSourceMethod]
        public void ExecuteLeavePickerButton()
        {
            SetHoveredPart(PickerHover.None);
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

        [DataSourceMethod]
        public void ExecuteToggleSpendLeftoverPoints()
        {
            _presets.ToggleSpendLeftoverPoints();
            RefreshSpendLeftoverPoints();
        }

        [DataSourceMethod]
        public void ExecuteToggleRandomizeUnpickedPerks()
        {
            _presets.ToggleRandomizeUnpickedPerks();
            RefreshRandomizeUnpickedPerks();
        }

        [DataSourceMethod]
        public void ExecuteShowRandomUnpickedPerksHint()
        {
            SetHoveredPart(PickerHover.RandomPerksToggle);
            MBInformationManager.ShowHint(GetRandomUnpickedPerksHint());
        }

        [DataSourceMethod]
        public void ExecuteHideRandomUnpickedPerksHint()
        {
            SetHoveredPart(PickerHover.None);
            MBInformationManager.HideInformations();
        }

        [DataSourceMethod]
        public void ExecuteShowSpendLeftoverHint()
        {
            SetHoveredPart(PickerHover.LeftoverToggle);
            MBInformationManager.ShowHint(GetSpendLeftoverPointsHint());
        }

        [DataSourceMethod]
        public void ExecuteHideSpendLeftoverHint()
        {
            SetHoveredPart(PickerHover.None);
            MBInformationManager.HideInformations();
        }

        [DataSourceMethod]
        public void ExecuteHoverFocusFloorMinus()
        {
            SetHoveredPart(PickerHover.FocusFloorMinus);
        }

        [DataSourceMethod]
        public void ExecuteHoverFocusFloorPlus()
        {
            SetHoveredPart(PickerHover.FocusFloorPlus);
        }

        [DataSourceMethod]
        public void ExecuteHoverAttributeFloorMinus()
        {
            SetHoveredPart(PickerHover.AttributeFloorMinus);
        }

        [DataSourceMethod]
        public void ExecuteHoverAttributeFloorPlus()
        {
            SetHoveredPart(PickerHover.AttributeFloorPlus);
        }

        private void SetHoveredPart(PickerHover hoveredPart)
        {
            if (_hoveredPart == hoveredPart)
            {
                return;
            }

            _hoveredPart = hoveredPart;
            RefreshHoverBindings();
        }

        private void RefreshHoverBindings()
        {
            OnPropertyChangedWithValue(IsCancelHovered, "IsCancelHovered");
            OnPropertyChangedWithValue(IsEditHovered, "IsEditHovered");
            OnPropertyChangedWithValue(IsContinueHovered, "IsContinueHovered");
            OnPropertyChangedWithValue(IsSpendLeftoverHovered, "IsSpendLeftoverHovered");
            OnPropertyChangedWithValue(IsRandomUnpickedPerksHovered, "IsRandomUnpickedPerksHovered");
            OnPropertyChangedWithValue(IsDecreaseFocusFloorHovered, "IsDecreaseFocusFloorHovered");
            OnPropertyChangedWithValue(IsIncreaseFocusFloorHovered, "IsIncreaseFocusFloorHovered");
            OnPropertyChangedWithValue(IsDecreaseAttributeFloorHovered, "IsDecreaseAttributeFloorHovered");
            OnPropertyChangedWithValue(IsIncreaseAttributeFloorHovered, "IsIncreaseAttributeFloorHovered");
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
                    new TextObject("{=rename_preset}Rename Preset").ToString(),
                    new TextObject("{=enter_preset_name}Enter preset name.").ToString(),
                    true,
                    true,
                    new TextObject("{=save}Save").ToString(),
                    new TextObject("{=cancel}Cancel").ToString(),
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

            var clearPresetQuestion = new TextObject("{=clear_preset_question}Clear all skillset targets from {PRESET_NAME}?");
            clearPresetQuestion.SetTextVariable("PRESET_NAME", row.NameText);

            InformationManager.ShowInquiry(
                new InquiryData(
                    new TextObject("{=clear_preset}Clear Preset").ToString(),
                    clearPresetQuestion.ToString(),
                    true,
                    true,
                    new TextObject("{=clear}Clear").ToString(),
                    new TextObject("{=cancel}Cancel").ToString(),
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
                    new TextObject("{=mimic_source_hint}Use this characters current skillset as a dynamic preset source.").ToString()));
            }

            if (heroes.Count == 0)
            {
                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=no_mimic_hero_found}No available clan member found to mimic preset.").ToString()));
                return;
            }

            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    new TextObject("{=mimic_skillset}Mimic Skillset").ToString(),
                    new TextObject("{=mimic_skillset_description}Select a clan member. This preset will dynamically copy that characters skillset.").ToString(),
                    heroes,
                    true,
                    1,
                    1,
                    new TextObject("{=select}Select").ToString(),
                    new TextObject("{=cancel}Cancel").ToString(),
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
                    ? new TextObject("{=no_templates_found}No skill preset templates were found.").ToString()
                    : PredefinedTemplateCatalog.LoadError;

                InformationManager.DisplayMessage(new InformationMessage(message));
                return;
            }

            List<InquiryElement> elements = new List<InquiryElement>();

            foreach (TemplatePreset template in templates)
            {
                string hint;

                if (string.IsNullOrEmpty(template.Description))
                {
                    var copyTemplateHint = new TextObject("{=copy_template_into}Copy this template into {PRESET_NAME}.");
                    copyTemplateHint.SetTextVariable("PRESET_NAME", row.NameText);
                    hint = copyTemplateHint.ToString();
                }
                else
                {
                    hint = template.Description;
                }

                elements.Add(new InquiryElement(
                    template,
                    template.Name,
                    null,
                    true,
                    hint));
            }

            var templatesDescription = new TextObject("{=templates_description}Select a built-in template to copy into {PRESET_NAME}.");
            templatesDescription.SetTextVariable("PRESET_NAME", row.NameText);

            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    new TextObject("{=templates}Templates").ToString(),
                    templatesDescription.ToString(),
                    elements,
                    true,
                    1,
                    1,
                    new TextObject("{=copy}Copy").ToString(),
                    new TextObject("{=cancel}Cancel").ToString(),
                    pickedTemplates => UseTemplate(row, pickedTemplates),
                    null));
        }


        public void ExportPreset(PresetPickerRowVM row)
        {
            if (row == null || row.SlotIndex == 0)
            {
                return;
            }

            SkillPreset preset = _presets.GetPreset(row.SlotIndex);

            if (preset == null || !preset.IsConfigured)
            {
                ShowExportResult(false, new TextObject("{=only_configured_export}Only configured presets can be exported.").ToString());
                return;
            }

            InformationManager.ShowTextInquiry(
                new TextInquiryData(
                    new TextObject("{=export_preset}Export Preset").ToString(),
                    new TextObject("{=name_exported_template}Name this exported template").ToString(),
                    true,
                    true,
                    new TextObject("{=export}Export").ToString(),
                    new TextObject("{=cancel}Cancel").ToString(),
                    exportName => ExportSlotWithName(row, exportName),
                    null,
                    false,
                    CheckPresetName,
                    string.Empty,
                    preset.Name),
                false,
                false);
        }
        private void ExportSlotWithName(PresetPickerRowVM row, string exportName)
        {
            SkillPreset preset = _presets.GetPreset(row.SlotIndex);
            string message;
            bool success = TemplateExporter.TryExportPreset(preset, exportName, out message);

            ShowExportResult(success, message);
        }

        private static void ShowExportResult(bool success, string message)
        {
            InformationManager.ShowInquiry(
                new InquiryData(
                    success ? new TextObject("{=export_successful}Export successful").ToString() : new TextObject("{=export_failed}Export failed").ToString(),
                    message,
                    true,
                    false,
                    new TextObject("{=ok}OK").ToString(),
                    string.Empty,
                    null,
                    null),
                false,
                false);
        }

        private string GetFocusFloorHint()
        {
			return new TextObject("{=focus_floor_hint}Focus floor. Assigned skills are filled to this value before the preset starts targeting other skills with higher values (if their target value is over the floor value). Use - / + to change it.").ToString();
		}

        private string GetAttributeFloorHint()
        {
            return new TextObject("{=attribute_floor_hint}Attribute floor. Assigned attributes are filled to this value before the preset starts targeting other skills with higher values (if their target value is over the floor value). Use - / + to change it.").ToString();
        }
        private string GetSpendLeftoverPointsHint()
        {
            return new TextObject("{=spend_leftover_hint}On: after every preset target is reached, extra points will keep being allocated into the highest target assigned skill or attribute that is not maxed yet.\nOff: stop when preset target is reached for every skill and attribute.").ToString();
        }

        private string GetRandomUnpickedPerksHint()
        {
            return new TextObject("{=random_unpicked_perks_hint}On: if a hero unlocks a perk tier not defined by their preset, a random available perk from that tier will be chosen randomly.\nOff: only perks defined in the preset are assigned.").ToString();
        }

        private void RefreshFloorText()
        {
            var focusFloorText = new TextObject("{=focus_floor_text}Focus {VALUE}");
            focusFloorText.SetTextVariable("VALUE", _presets.FocusFloorLimit);
            FocusFloorText = focusFloorText.ToString();

            var attributeFloorText = new TextObject("{=attribute_floor_text}Attr {VALUE}");
            attributeFloorText.SetTextVariable("VALUE", _presets.AttributeFloorLimit);
            AttributeFloorText = attributeFloorText.ToString();
        }
        private void RefreshRandomizeUnpickedPerks()
        {
            RandomizeUnpickedPerks = _presets.RandomizeUnpickedPerks;
        }

        private void RefreshSpendLeftoverPoints()
        {
            SpendLeftoverPoints = _presets.SpendLeftoverPoints;
        }

        private void FillPresetSlots()
        {
            TitleText = new TextObject("{=add_presets}Add Presets").ToString();
            DescriptionText = new TextObject("{=add_presets_description}Pick a preset slot to edit. Slots can be renamed, cleared, mimiced, or filled from templates.").ToString();

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

        private List<PresetSlotSnapshot> TakePresetSnapshot()
        {
            List<PresetSlotSnapshot> snapshot = new List<PresetSlotSnapshot>();

            foreach (SkillPreset preset in _presets.GetPresets())
            {
                preset.RebuildAfterLoad();
                snapshot.Add(new PresetSlotSnapshot(preset));
            }

            return snapshot;
        }

        private void RestorePresetSnapshot()
        {
            foreach (PresetSlotSnapshot slot in _presetsBeforeAddScreen)
            {
                SkillPreset preset = _presets.GetPreset(slot.SlotIndex);

                if (preset == null)
                {
                    continue;
                }

                if (!slot.IsConfigured)
                {
                    _presets.ClearPreset(slot.SlotIndex);
                    _presets.RenamePreset(slot.SlotIndex, slot.Name);
                }
                else if (slot.IsMimicPreset)
                {
                    preset.ConfigureAsMimic(slot.MimicHeroStringId, slot.MimicHeroName);
                }
                else
                {
                    _presets.ConfigurePreset(
                        slot.SlotIndex,
                        slot.Name,
                        new Dictionary<string, int>(slot.AttributeTargets),
                        new Dictionary<string, int>(slot.SkillFocusTargets),
                        new List<string>(slot.SelectedPerkIds));
                }
            }
        }

        private sealed class PresetSlotSnapshot
        {
            public readonly int SlotIndex;
            public readonly string Name;
            public readonly bool IsConfigured;
            public readonly bool IsMimicPreset;
            public readonly string MimicHeroStringId;
            public readonly string MimicHeroName;
            public readonly Dictionary<string, int> AttributeTargets;
            public readonly Dictionary<string, int> SkillFocusTargets;
            public readonly List<string> SelectedPerkIds;

            public PresetSlotSnapshot(SkillPreset preset)
            {
                SlotIndex = preset.SlotIndex;
                Name = preset.HasDefaultName ? string.Empty : preset.Name;
                IsConfigured = preset.IsConfigured;
                IsMimicPreset = preset.IsMimicPreset;
                MimicHeroStringId = preset.MimicHeroStringId;
                MimicHeroName = preset.MimicHeroName;
                AttributeTargets = new Dictionary<string, int>(preset.AttributeTargets);
                SkillFocusTargets = new Dictionary<string, int>(preset.SkillFocusTargets);
                SelectedPerkIds = new List<string>(preset.SelectedPerkIds);
            }
        }

        private static Tuple<bool, string> CheckPresetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new Tuple<bool, string>(false, new TextObject("{=preset_name_empty}Preset name cannot be empty.").ToString());
            }

            return new Tuple<bool, string>(true, string.Empty);
        }
    }
}