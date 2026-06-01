using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace ProperSkillDistributor
{
    public class SkillPresetSaveTypes : SaveableTypeDefiner
    {
        public SkillPresetSaveTypes()
            : base(928440000)
        {
        }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(SkillPreset), 1);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<SkillPreset>));
            ConstructContainerDefinition(typeof(Dictionary<string, int>));
            ConstructContainerDefinition(typeof(List<string>));
        }
    }
}
