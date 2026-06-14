using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace ProperSkillDistributor
{
    internal static class TorCompatibility
    {
        public static bool IsLoaded
        {
            get
            {
                return HasCharacterAttribute("discipline")
                    && HasSkill("Faith")
                    && HasSkill("Gunpowder")
                    && HasSkill("Spellcraft");
            }
        }

        private static bool HasCharacterAttribute(string attributeId)
        {
            return MBObjectManager.Instance.GetObject<CharacterAttribute>(attributeId) != null;
        }

        private static bool HasSkill(string skillId)
        {
            return MBObjectManager.Instance.GetObject<SkillObject>(skillId) != null;
        }
    }
}
