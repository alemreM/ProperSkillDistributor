using Bannerlord.UIExtenderEx.Attributes;
using TaleWorlds.Library;

namespace ProperSkillDistributor
{
    public class PresetAssignmentRowVM : ViewModel
    {
        private readonly CharacterScreenPresetMixin _characterScreenControls;

        private string _nameText;
        private string _statusText;
        private bool _isSelected;
        private bool _isHovered;

        public int SlotIndex { get; }

        [DataSourceProperty]
        public string NameText
        {
            get { return _nameText; }
            set
            {
                _nameText = value;
                OnPropertyChangedWithValue(value, "NameText");
            }
        }

        [DataSourceProperty]
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                OnPropertyChangedWithValue(value, "StatusText");
            }
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChangedWithValue(value, "IsSelected");
            }
        }

        [DataSourceProperty]
        public bool IsHovered
        {
            get { return _isHovered; }
            set
            {
                _isHovered = value;
                OnPropertyChangedWithValue(value, "IsHovered");
            }
        }

        public PresetAssignmentRowVM(
            CharacterScreenPresetMixin characterScreenControls,
            int slotIndex,
            string nameText,
            string statusText,
            bool isSelected)
        {
            _characterScreenControls = characterScreenControls;
            SlotIndex = slotIndex;
            _nameText = nameText;
            _statusText = statusText;
            _isSelected = isSelected;
            _isHovered = false;
        }

        [DataSourceMethod]
        public void ExecuteSelect()
        {
            _characterScreenControls.SelectUsePreset(this);
        }

        [DataSourceMethod]
        public void ExecuteHoverBegin()
        {
            IsHovered = true;
        }

        [DataSourceMethod]
        public void ExecuteHoverEnd()
        {
            IsHovered = false;
        }
    }
}