using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;
using TaleWorlds.Localization;

namespace ProperSkillDistributor
{
    public static class CharacterScreenPresetActions
    {
        public static void OpenPresetEditorSelector(CharacterDeveloperVM characterDeveloperVM)
        {
            SkillPresetBehavior behavior = SkillPresetBehavior.Current;

            if (behavior == null)
            {
                ShowMessage(new TextObject("{=skill_presets_title}Skill Presets").ToString(), new TextObject("{=no_active_campaign_behavior}No active campaign behavior was found.").ToString());
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
                    new TextObject("{=ok}OK").ToString(),
                    string.Empty,
                    null,
                    null),
                false,
                false);
        }
    }
}