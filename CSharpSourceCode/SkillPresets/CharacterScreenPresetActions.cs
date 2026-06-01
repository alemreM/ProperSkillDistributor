using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace ProperSkillDistributor
{
    public static class CharacterScreenPresetActions
    {
        public static void OpenPresetEditorSelector(CharacterDeveloperVM characterDeveloperVM)
        {
            SkillPresetBehavior behavior = SkillPresetBehavior.Current;

            if (behavior == null)
            {
                ShowMessage("Skill Presets", "No active campaign behavior was found.");
                return;
            }

            ScreenManager.PushScreen(new PresetPickerScreen(behavior));
        }

        public static void OpenPresetAssignmentSelector(CharacterDeveloperVM characterDeveloperVM)
        {
        }

        private static void ShowMessage(string title, string message)
        {
            InformationManager.ShowInquiry(
                new InquiryData(
                    title,
                    message,
                    true,
                    false,
                    "OK",
                    string.Empty,
                    null,
                    null),
                false,
                false);
        }
    }
}