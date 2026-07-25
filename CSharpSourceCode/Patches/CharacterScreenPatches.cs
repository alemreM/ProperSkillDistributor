using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.ScreenSystem;

namespace ProperSkillDistributor
{
    [HarmonyPatch]
    public static class CharacterScreenOpenPatch
    {
        private static ConstructorInfo TargetMethod()
        {
            return AccessTools.Constructor(
                typeof(CharacterDeveloperVM),
                new[] { typeof(Action) });
        }

        private static void Prefix()
        {
            // preset editor uses sandboxed hero values
            if (!PresetEditorSession.IsActive)
            {
                SkillPresetBehavior.Current?.ApplyAllAssignedPresets();
            }
        }

        private static void Postfix(CharacterDeveloperVM __instance)
        {
            if (PresetEditorSession.IsActive)
            {
                PresetEditorSession.Attach(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(CharacterDeveloperVM), "ExecuteDone")]
    public static class DoneLeavesCharacterScreen
    {
        private static bool Prefix(CharacterDeveloperVM __instance, ref bool __state)
        {
            __state = PresetEditorSession.IsActive;

            if (__state)
            {
                return PresetEditorSession.SaveAndClose(__instance);
            }

            return true;
        }

        private static void Postfix(bool __state)
        {
            if (__state || PresetEditorSession.IsActive)
            {
                return;
            }

            SkillPresetBehavior.Current?.ApplyAllAssignedPresets();
        }
    }

    [HarmonyPatch(typeof(CharacterDeveloperVM), "ExecuteCancel")]
    public static class CancelLeavesCharacterScreen
    {
        private static bool Prefix(ref bool __state)
        {
            __state = PresetEditorSession.IsActive;

            if (__state)
            {
                return PresetEditorSession.CancelAndClose();
            }

            return true;
        }

        private static void Postfix(bool __state)
        {
            if (__state || PresetEditorSession.IsActive)
            {
                return;
            }

            SkillPresetBehavior.Current?.ApplyAllAssignedPresets();
        }
    }

    [HarmonyPatch(typeof(CharacterDeveloperVM), "ExecuteReset")]
    public static class ResetInsidePresetEditor
    {
        private static void Postfix(CharacterDeveloperVM __instance)
        {
            PresetEditorSession.ReapplyInitialPerks(__instance);
        }
    }

    [HarmonyPatch(typeof(CharacterDeveloperVM), "OnFinalize")]
    public static class CharacterVmFinalCleanup
    {
        private static void Prefix()
        {
            PresetEditorSession.RestoreIfActive();
        }
    }

    [HarmonyPatch(typeof(CharacterDeveloperVM), "SetCurrentHero")]
    public static class PresetEditorStopsHeroJump
    {
        private static bool Prefix(CharacterDeveloperHeroItemVM currentHero, CharacterDeveloperHeroItemVM ____currentCharacter)
        {
            if (!PresetEditorSession.IsActive)
            {
                return true;
            }

            if (____currentCharacter == null || ____currentCharacter == currentHero)
            {
                return true;
            }

            return false;
        }

        private static void Postfix(CharacterDeveloperVM __instance)
        {
            CharacterScreenPresetMixin.RefreshFor(__instance);
        }
    }

    [HarmonyPatch(typeof(CharacterDeveloperVM), "GetApplicableHeroes")]
    public static class PresetEditorUsesFakeRoster
    {
        private static bool Prefix(ref List<Hero> __result)
        {
            if (!PresetEditorSession.IsActive)
            {
                return true;
            }

            // reused CharacterDeveloper on a singular hero
            __result = PresetEditorSession.GetPresetEditorHeroes();
            return false;
        }
    }

    [HarmonyPatch]
    public static class CharacterScreenFocusAfterPopup
    {
        private static bool _restoreFocusOnNextCharacterDeveloperTick;

        public static void RequestFocusRestore(string source)
        {
            _restoreFocusOnNextCharacterDeveloperTick = true;
        }

        private static MethodBase TargetMethod()
        {
            // ScreenBase become visible and usable without restoring focus. this will end up ignoring hotkeys are that only usable when focused (q, e, esc)
            Type screenType = AccessTools.TypeByName("SandBox.GauntletUI.GauntletCharacterDeveloperScreen");
            return AccessTools.Method(screenType, "OnFrameTick", new[] { typeof(float) });
        }

        private static void Postfix(GauntletLayer ____gauntletLayer, CharacterDeveloperVM ____dataSource)
        {
            if (!_restoreFocusOnNextCharacterDeveloperTick)
            {
                return;
            }

            if (____gauntletLayer == null || !____gauntletLayer.IsActive || ____gauntletLayer.IsFinalized)
            {
                return;
            }

            _restoreFocusOnNextCharacterDeveloperTick = false;

            CharacterScreenPresetMixin.RefreshFor(____dataSource);

            ____gauntletLayer.IsFocusLayer = true;

            if (ScreenManager.FocusedLayer != ____gauntletLayer)
            {
                ScreenManager.TrySetFocus(____gauntletLayer);
            }
        }
    }
}