using System;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace ProperSkillDistributor
{
    public class PresetEditorScreen : ScreenBase
    {
        private readonly SkillPresetBehavior _behavior;
        private readonly int _slotIndex;
        private readonly Action _afterClose;

        private CharacterDeveloperVM _dataSource;
        private GauntletLayer _gauntletLayer;
        private readonly CharacterDeveloperSpriteScope _spriteScope = new CharacterDeveloperSpriteScope();

        public PresetEditorScreen(
            SkillPresetBehavior behavior,
            int slotIndex,
            Action afterClose = null)
        {
            _behavior = behavior;
            _slotIndex = slotIndex;
            _afterClose = afterClose;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            InformationManager.HideAllMessages();

            _spriteScope.Load();

            // if editor isnt active before new CharacterDeveloperVM it will be built from the actual hero roster
            PresetEditorSession.Begin(_behavior, _slotIndex, CloseScreen);

            _dataSource = new CharacterDeveloperVM(CloseScreen);
            _dataSource.SetGetKeyTextFromKeyIDFunc(Game.Current.GameTextManager.GetHotKeyGameTextFromKeyID);
            _dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
            _dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
            _dataSource.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
            _dataSource.SetPreviousCharacterInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToPreviousTab"));
            _dataSource.SetNextCharacterInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToNextTab"));

            _gauntletLayer = new GauntletLayer("PresetEditor", 2, true);
            _gauntletLayer.InputRestrictions.SetInputRestrictions();
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
            _gauntletLayer.LoadMovie("CharacterDeveloper", _dataSource);

            CharacterScreenPresetMixin.RefreshFor(_dataSource);

            AddLayer(_gauntletLayer);

            _gauntletLayer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_gauntletLayer);
            _gauntletLayer.GamepadNavigationContext.GainNavigationAfterFrames(2, null);

            UISoundsHelper.PlayUISound("event:/ui/panels/panel_character_open");
        }

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);

            if (_gauntletLayer.Input.IsHotKeyReleased("Exit"))
            {
                UISoundsHelper.PlayUISound("event:/ui/default");

                if (_dataSource.CurrentCharacter.IsInspectingAnAttribute)
                {
                    _dataSource.CurrentCharacter.ExecuteStopInspectingCurrentAttribute();
                }
                else if (_dataSource.CurrentCharacter.PerkSelection.IsActive)
                {
                    _dataSource.CurrentCharacter.PerkSelection.ExecuteDeactivate();
                }
                else
                {
                    _dataSource.ExecuteCancel();
                }
            }
            else if (_gauntletLayer.Input.IsHotKeyReleased("Confirm"))
            {
                UISoundsHelper.PlayUISound("event:/ui/default");
                _dataSource.ExecuteDone();
            }
            else if (_gauntletLayer.Input.IsHotKeyReleased("Reset"))
            {
                UISoundsHelper.PlayUISound("event:/ui/default");
                _dataSource.ExecuteReset();
            }
        }

        protected override void OnFinalize()
        {
            base.OnFinalize();

            PresetEditorSession.RestoreIfActive();

            RemoveLayer(_gauntletLayer);
            _gauntletLayer.IsFocusLayer = false;
            ScreenManager.TryLoseFocus(_gauntletLayer);
            _gauntletLayer = null;

            _dataSource.OnFinalize();
            _dataSource = null;

            _spriteScope.Unload();
        }

        private void ExecuteSwitchToPreviousTab()
        {
            MBBindingList<SelectorItemVM> itemList = _dataSource.CharacterList.ItemList;

            if (itemList != null && itemList.Count > 1)
            {
                UISoundsHelper.PlayUISound("event:/ui/checkbox");
            }

            _dataSource.CharacterList.ExecuteSelectPreviousItem();
        }

        private void ExecuteSwitchToNextTab()
        {
            MBBindingList<SelectorItemVM> itemList = _dataSource.CharacterList.ItemList;

            if (itemList != null && itemList.Count > 1)
            {
                UISoundsHelper.PlayUISound("event:/ui/checkbox");
            }

            _dataSource.CharacterList.ExecuteSelectNextItem();
        }

        private void CloseScreen()
        {
            ScreenManager.PopScreen();

            if (_afterClose != null)
            {
                _afterClose();
                return;
            }
            CharacterScreenFocusAfterPopup.RequestFocusRestore("vanilla preset editor close");
        }
    }
}
