using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace ProperSkillDistributor
{
    public class SkillPreset
    {
        [SaveableField(1)]
        private int _slotIndex;

        [SaveableField(2)]
        private string _label;

        [SaveableField(3)]
        private bool _hasBuild;

        [SaveableField(4)]
        private Dictionary<string, int> _attributePlan;

        [SaveableField(5)]
        private Dictionary<string, int> _focusPlan;

        [SaveableField(6)]
        private List<string> _perkPlan;

        [SaveableField(7)]
        private string _mimicHeroStringId;

        [SaveableField(8)]
        private string _mimicHeroName;

        public int SlotIndex => _slotIndex;

        public string Name => string.IsNullOrEmpty(_label) ? "Preset " + _slotIndex : _label;

        public bool IsConfigured => _hasBuild;

        public Dictionary<string, int> AttributeTargets => _attributePlan;

        public Dictionary<string, int> SkillFocusTargets => _focusPlan;

        public List<string> SelectedPerkIds => _perkPlan;

        public string MimicHeroStringId => _mimicHeroStringId;

        public string MimicHeroName => _mimicHeroName;

        public bool IsMimicPreset => !string.IsNullOrEmpty(_mimicHeroStringId);

        public SkillPreset()
        {
            _attributePlan = new Dictionary<string, int>();
            _focusPlan = new Dictionary<string, int>();
            _perkPlan = new List<string>();
        }

        public SkillPreset(int slotIndex)
            : this()
        {
            _slotIndex = slotIndex;
            _label = "Preset " + slotIndex;
        }

        public void RebuildAfterLoad()
        {
            if (string.IsNullOrEmpty(_label))
            {
                _label = "Preset " + _slotIndex;
            }

            _attributePlan = _attributePlan ?? new Dictionary<string, int>();
            _focusPlan = _focusPlan ?? new Dictionary<string, int>();
            _perkPlan = _perkPlan ?? new List<string>();
        }

        public void Rename(string name)
        {
            _label = string.IsNullOrEmpty(name) ? "Preset " + _slotIndex : name;
        }

        public void Configure(
            string name,
            Dictionary<string, int> attributeTargets,
            Dictionary<string, int> skillFocusTargets,
            List<string> selectedPerkIds)
        {
            RebuildAfterLoad();
            Rename(name);

            _attributePlan.Clear();
            _focusPlan.Clear();
            _perkPlan.Clear();

            if (attributeTargets != null)
            {
                foreach (KeyValuePair<string, int> target in attributeTargets)
                {
                    _attributePlan[target.Key] = target.Value;
                }
            }

            if (skillFocusTargets != null)
            {
                foreach (KeyValuePair<string, int> target in skillFocusTargets)
                {
                    _focusPlan[target.Key] = target.Value;
                }
            }

            if (selectedPerkIds != null)
            {
                foreach (string perkId in selectedPerkIds)
                {
                    _perkPlan.Add(perkId);
                }
            }

            // manual editing turns a mimic preset into a regular preset
            _mimicHeroStringId = null;
            _mimicHeroName = null;
            _hasBuild = true;
        }

        public void ConfigureAsMimic(string mimicHeroStringId, string mimicHeroName)
        {
            RebuildAfterLoad();

            _hasBuild = true;
            _mimicHeroStringId = mimicHeroStringId;
            _mimicHeroName = mimicHeroName;
            _label = mimicHeroName + "'s skillset";

            _attributePlan.Clear();
            _focusPlan.Clear();
            _perkPlan.Clear();
        }

        public void Clear()
        {
            RebuildAfterLoad();

            _hasBuild = false;
            _mimicHeroStringId = null;
            _mimicHeroName = null;

            _attributePlan.Clear();
            _focusPlan.Clear();
            _perkPlan.Clear();
        }

        public int GetAttributeTarget(string attributeId)
        {
            RebuildAfterLoad();

            int wantedValue;
            return _attributePlan.TryGetValue(attributeId, out wantedValue) ? wantedValue : 0;
        }

        public int GetSkillFocusTarget(string skillId)
        {
            RebuildAfterLoad();

            int wantedFocus;
            return _focusPlan.TryGetValue(skillId, out wantedFocus) ? wantedFocus : 0;
        }
    }
}