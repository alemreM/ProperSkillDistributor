using Bannerlord.UIExtenderEx.Attributes;
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

            RefreshFromPreset(preset);
        }

        [DataSourceMethod]
        public void ExecuteSelect()
        {
            _selector.SelectRow(this);
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
