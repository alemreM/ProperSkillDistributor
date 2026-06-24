using Bannerlord.UIExtenderEx.Attributes;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace ProperSkillDistributor
{
    public class PresetPickerRowVM : ViewModel
    {
        private enum PresetRowHover
        {
            None,
            Slot,
            Rename,
            Clear,
            Mimic,
            Templates,
            Export
        }

        private readonly PresetPickerVM _selector;

        private string _nameText;
        private string _statusText;
        private string _selectionText;
        private string _clearButtonText;
        private string _mimicButtonText;
        private string _templatesButtonText;
        private string _exportButtonText;
        private bool _isSelected;
        private bool _canRename;
        private bool _canClear;
        private bool _canMimic;
        private bool _canTemplates;
        private bool _canExport;
        private PresetRowHover _hoveredPart;

        public int SlotIndex { get; private set; }

        [DataSourceProperty]
        public string NameText
        {
            get { return _nameText; }
            set
            {
                if (value != _nameText)
                {
                    _nameText = value;
                    OnPropertyChangedWithValue(value, "NameText");
                }
            }
        }

        [DataSourceProperty]
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                if (value != _statusText)
                {
                    _statusText = value;
                    OnPropertyChangedWithValue(value, "StatusText");
                }
            }
        }

        [DataSourceProperty]
        public string SelectionText
        {
            get { return _selectionText; }
            set
            {
                if (value != _selectionText)
                {
                    _selectionText = value;
                    OnPropertyChangedWithValue(value, "SelectionText");
                }
            }
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChangedWithValue(value, "IsSelected");
                    SelectionText = value ? new TextObject("{=selected}selected").ToString() : string.Empty;
                }
            }
        }

        [DataSourceProperty]
        public bool CanRename
        {
            get { return _canRename; }
            set
            {
                if (value != _canRename)
                {
                    _canRename = value;
                    OnPropertyChangedWithValue(value, "CanRename");
                }
            }
        }

        [DataSourceProperty]
        public bool CanClear
        {
            get { return _canClear; }
            set
            {
                if (value != _canClear)
                {
                    _canClear = value;
                    OnPropertyChangedWithValue(value, "CanClear");
                }
            }
        }

        [DataSourceProperty]
        public bool CanMimic
        {
            get { return _canMimic; }
            set
            {
                if (value != _canMimic)
                {
                    _canMimic = value;
                    OnPropertyChangedWithValue(value, "CanMimic");
                }
            }
        }

        [DataSourceProperty]
        public bool CanTemplates
        {
            get { return _canTemplates; }
            set
            {
                if (value != _canTemplates)
                {
                    _canTemplates = value;
                    OnPropertyChangedWithValue(value, "CanTemplates");
                }
            }
        }

        [DataSourceProperty]
        public bool CanExport
        {
            get { return _canExport; }
            set
            {
                if (value != _canExport)
                {
                    _canExport = value;
                    OnPropertyChangedWithValue(value, "CanExport");
                }
            }
        }

        [DataSourceProperty]
        public bool IsSlotHovered
        {
            get { return _hoveredPart == PresetRowHover.Slot; }
        }

        [DataSourceProperty]
        public bool IsRenameHovered
        {
            get { return _hoveredPart == PresetRowHover.Rename; }
        }

        [DataSourceProperty]
        public bool IsClearHovered
        {
            get { return _hoveredPart == PresetRowHover.Clear; }
        }

        [DataSourceProperty]
        public bool IsMimicHovered
        {
            get { return _hoveredPart == PresetRowHover.Mimic; }
        }

        [DataSourceProperty]
        public bool IsTemplatesHovered
        {
            get { return _hoveredPart == PresetRowHover.Templates; }
        }

        [DataSourceProperty]
        public bool IsExportHovered
        {
            get { return _hoveredPart == PresetRowHover.Export; }
        }

        [DataSourceProperty]
        public string ClearText
        {
            get { return _clearButtonText; }
            set
            {
                if (value != _clearButtonText)
                {
                    _clearButtonText = value;
                    OnPropertyChangedWithValue(value, "ClearText");
                }
            }
        }

        [DataSourceProperty]
        public string MimicText
        {
            get { return _mimicButtonText; }
            set
            {
                if (value != _mimicButtonText)
                {
                    _mimicButtonText = value;
                    OnPropertyChangedWithValue(value, "MimicText");
                }
            }
        }

        [DataSourceProperty]
        public string TemplatesText
        {
            get { return _templatesButtonText; }
            set
            {
                if (value != _templatesButtonText)
                {
                    _templatesButtonText = value;
                    OnPropertyChangedWithValue(value, "TemplatesText");
                }
            }
        }

        [DataSourceProperty]
        public string ExportText
        {
            get { return _exportButtonText; }
            set
            {
                if (value != _exportButtonText)
                {
                    _exportButtonText = value;
                    OnPropertyChangedWithValue(value, "ExportText");
                }
            }
        }

        public PresetPickerRowVM(PresetPickerVM selector)
        {
            _selector = selector;
            SlotIndex = 0;
            NameText = new TextObject("{=none}None").ToString();
            StatusText = new TextObject("{=do_not_use_preset}Do not use a preset for this character.").ToString();
            SetActionButtonTexts();
            CanRename = false;
            CanClear = false;
            CanMimic = false;
            CanTemplates = false;
            CanExport = false;
            SelectionText = string.Empty;
            SetHoveredPart(PresetRowHover.None);
        }

        public PresetPickerRowVM(
            PresetPickerVM selector,
            SkillPreset preset,
            bool canRename)
        {
            _selector = selector;
            SlotIndex = preset.SlotIndex;
            CanRename = canRename;
            CanClear = canRename;
            CanMimic = canRename;
            CanTemplates = canRename;
            CanExport = canRename;
            SetActionButtonTexts();
            SelectionText = string.Empty;
            SetHoveredPart(PresetRowHover.None);

            RefreshFromPreset(preset);
        }


        private void SetActionButtonTexts()
        {
            ClearText = new TextObject("{=clear}Clear").ToString();
            MimicText = new TextObject("{=mimic}Mimic").ToString();
            TemplatesText = new TextObject("{=templates}Templates").ToString();
            ExportText = new TextObject("{=export}Export").ToString();
        }

        [DataSourceMethod]
        public void ExecuteSelect()
        {
            _selector.SelectRow(this);
        }

        [DataSourceMethod]
        public void ExecuteHoverBegin()
        {
            SetHoveredPart(PresetRowHover.Slot);
        }

        [DataSourceMethod]
        public void ExecuteHoverEnd()
        {
            SetHoveredPart(PresetRowHover.None);
        }

        [DataSourceMethod]
        public void ExecuteRename()
        {
            _selector.RenamePreset(this);
        }

        [DataSourceMethod]
        public void ExecuteClear()
        {
            _selector.ClearPreset(this);
        }

        [DataSourceMethod]
        public void ExecuteMimic()
        {
            _selector.MimicPreset(this);
        }

        [DataSourceMethod]
        public void ExecuteTemplates()
        {
            _selector.TemplatePreset(this);
        }

        [DataSourceMethod]
        public void ExecuteExport()
        {
            _selector.ExportPreset(this);
        }

        [DataSourceMethod]
        public void ExecuteShowRenameHint()
        {
            SetHoveredPart(PresetRowHover.Rename);
            MBInformationManager.ShowHint(new TextObject("{=rename}Rename").ToString());
        }

        [DataSourceMethod]
        public void ExecuteShowClearHint()
        {
            SetHoveredPart(PresetRowHover.Clear);
            MBInformationManager.ShowHint(new TextObject("{=clear_hint}Clear this preset slot").ToString());
        }

        [DataSourceMethod]
        public void ExecuteShowMimicHint()
        {
            SetHoveredPart(PresetRowHover.Mimic);
            MBInformationManager.ShowHint(new TextObject("{=mimic_hint}Use a hero as a dynamic skill set source").ToString());
        }

        [DataSourceMethod]
        public void ExecuteShowTemplatesHint()
        {
            SetHoveredPart(PresetRowHover.Templates);
            MBInformationManager.ShowHint(new TextObject("{=templates_hint}Copy a predefined template into this slot").ToString());
        }

        [DataSourceMethod]
        public void ExecuteShowExportHint()
        {
            SetHoveredPart(PresetRowHover.Export);
            MBInformationManager.ShowHint(new TextObject("{=export_hint}Export this preset to be used later").ToString());
        }

        [DataSourceMethod]
        public void ExecuteHideOptionHint()
        {
            SetHoveredPart(PresetRowHover.None);
            MBInformationManager.HideInformations();
        }

        private void SetHoveredPart(PresetRowHover hoveredPart)
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
            OnPropertyChangedWithValue(IsSlotHovered, "IsSlotHovered");
            OnPropertyChangedWithValue(IsRenameHovered, "IsRenameHovered");
            OnPropertyChangedWithValue(IsClearHovered, "IsClearHovered");
            OnPropertyChangedWithValue(IsMimicHovered, "IsMimicHovered");
            OnPropertyChangedWithValue(IsTemplatesHovered, "IsTemplatesHovered");
            OnPropertyChangedWithValue(IsExportHovered, "IsExportHovered");
        }

        public void RefreshFromPreset(SkillPreset preset)
        {
            NameText = preset.Name;

            if (preset.IsMimicPreset)
            {
                var mimicStatusText = new TextObject("{=mimics}Mimics {HERO_NAME}");
                mimicStatusText.SetTextVariable("HERO_NAME", preset.MimicHeroName);
                StatusText = mimicStatusText.ToString();
                return;
            }

            StatusText = preset.IsConfigured ? new TextObject("{=configured}Configured").ToString() : new TextObject("{=not_configured}Not configured").ToString();
        }
    }
}
