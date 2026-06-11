using System.Collections.Generic;

namespace ProperSkillDistributor
{
    internal sealed class TemplatePreset
    {
        public string Id { get; private set; }
        public string Name { get; private set; }

        public string Description { get; private set; }

        public Dictionary<string, int> AttributeTargets { get; private set; }

        public Dictionary<string, int> SkillFocusTargets { get; private set; }

        public List<string> SelectedPerkIds { get; private set; }

        public TemplatePreset(
            string id,
            string name,
            string description,
            Dictionary<string, int> attributeTargets,
            Dictionary<string, int> skillFocusTargets,
            List<string> selectedPerkIds)
        {
            Id = id;
            Name = name;
            Description = description;

            AttributeTargets = attributeTargets ?? new Dictionary<string, int>();
            SkillFocusTargets = skillFocusTargets ?? new Dictionary<string, int>();
            SelectedPerkIds = selectedPerkIds ?? new List<string>();
        }
    }
}