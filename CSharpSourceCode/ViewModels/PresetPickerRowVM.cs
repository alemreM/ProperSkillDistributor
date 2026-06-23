using Bannerlord.UIExtenderEx.Attributes;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace ProperSkillDistributor
{
    public class PresetPickerRowVM : ViewModel
    {
        private readonly PresetPickerVM _selector;

        private string _nameText;
        private string _statusText;
        private string _selectionText;
        private bool _isSelected;
        private bool _canRename;
        private bool _canClear;
        private bool _canMimic;
        private bool _canTemplates;
        private bool _canExport;
        private bool _isSlotHovered;
        private bool _isRenameHovered;
        private bool _isClearHovered;
        private bool _isMimicHovered;
        private bool _isTemplatesHovered;
        private bool _isExportHovered;

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
                    SelectionText = value ? "selected" : string.Empty;
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
            get { return _isSlotHovered; }
            set
            {
                if (value != _isSlotHovered)
                {
                    _isSlotHovered = value;
                    OnPropertyChangedWithValue(value, "IsSlotHovered");
                }
            }
        }

        [DataSourceProperty]
        public bool IsRenameHovered
        {
            get { return _isRenameHovered; }
            set
            {
                if (value != _isRenameHovered)
                {
                    _isRenameHovered = value;
                    OnPropertyChangedWithValue(value, "IsRenameHovered");
                }
            }
        }

        [DataSourceProperty]
        public bool IsClearHovered
        {
            get { return _isClearHovered; }
            set
            {
                if (value != _isClearHovered)
                {
                    _isClearHovered = value;
                    OnPropertyChangedWithValue(value, "IsClearHovered");
                }
            }
        }

        [DataSourceProperty]
        public bool IsMimicHovered
        {
            get { return _isMimicHovered; }
            set
            {
                if (value != _isMimicHovered)
                {
                    _isMimicHovered = value;
                    OnPropertyChangedWithValue(value, "IsMimicHovered");
                }
            }
        }

        [DataSourceProperty]
        public bool IsTemplatesHovered
        {
            get { return _isTemplatesHovered; }
            set
            {
                if (value != _isTemplatesHovered)
                {
                    _isTemplatesHovered = value;
                    OnPropertyChangedWithValue(value, "IsTemplatesHovered");
                }
            }
        }

        [DataSourceProperty]
        public bool IsExportHovered
        {
            get { return _isExportHovered; }
            set
            {
                if (value != _isExportHovered)
                {
                    _isExportHovered = value;
                    OnPropertyChangedWithValue(value, "IsExportHovered");
                }
            }
        }

        public PresetPickerRowVM(PresetPickerVM selector)
        {
            _selector = selector;
            SlotIndex = 0;
            NameText = "None";
            StatusText = "Do not use a preset for this character.";
            CanRename = false;
            CanClear = false;
            CanMimic = false;
            CanTemplates = false;
            CanExport = false;
            SelectionText = string.Empty;
            ClearHoverState();
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
            SelectionText = string.Empty;
            ClearHoverState();

            RefreshFromPreset(preset);
        }

        [DataSourceMethod]
        public void ExecuteSelect()
        {
            _selector.SelectRow(this);
        }

        [DataSourceMethod]
        public void ExecuteHoverBegin()
        {
            IsSlotHovered = true;
        }

        [DataSourceMethod]
        public void ExecuteHoverEnd()
        {
            IsSlotHovered = false;
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
            IsRenameHovered = true;
            MBInformationManager.ShowHint("Rename");
        }

        [DataSourceMethod]
        public void ExecuteShowClearHint()
        {
            IsClearHovered = true;
            MBInformationManager.ShowHint("Clear this preset slot");
        }

        [DataSourceMethod]
        public void ExecuteShowMimicHint()
        {
            IsMimicHovered = true;
            MBInformationManager.ShowHint("Use a hero as a dynamic skill set source");
        }

        [DataSourceMethod]
        public void ExecuteShowTemplatesHint()
        {
            IsTemplatesHovered = true;
            MBInformationManager.ShowHint("Copy a predefined template into this slot");
        }

        [DataSourceMethod]
        public void ExecuteShowExportHint()
        {
            IsExportHovered = true;
            MBInformationManager.ShowHint("Export this preset to be used later");
        }

        [DataSourceMethod]
        public void ExecuteHideOptionHint()
        {
            ClearActionHoverState();
            MBInformationManager.HideInformations();
        }

        private void ClearHoverState()
        {
            IsSlotHovered = false;
            ClearActionHoverState();
        }

        private void ClearActionHoverState()
        {
            IsRenameHovered = false;
            IsClearHovered = false;
            IsMimicHovered = false;
            IsTemplatesHovered = false;
            IsExportHovered = false;
        }

        public void RefreshFromPreset(SkillPreset preset)
        {
            NameText = preset.Name;

            if (preset.IsMimicPreset)
            {
                StatusText = "Mimics " + preset.MimicHeroName;
                return;
            }

            StatusText = preset.IsConfigured ? "Configured" : "Not configured";
        }
    }
}
