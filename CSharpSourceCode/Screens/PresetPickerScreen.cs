using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace ProperSkillDistributor
{
    public class PresetPickerScreen : ScreenBase
    {
        private readonly SkillPresetBehavior _behavior;

        private SpriteCategory _clanCategory;
        private PresetPickerVM _dataSource;
        private GauntletLayer _gauntletLayer;

        public PresetPickerScreen(SkillPresetBehavior behavior)
        {
            _behavior = behavior;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            InformationManager.HideAllMessages();

            // unloading character ui vs. will lose its loaded sprites
            _clanCategory = UIResourceManager.LoadSpriteCategory("ui_clan");

            _dataSource = new PresetPickerVM(
                _behavior,
                CloseScreen,
                OpenPresetEditor);

            _gauntletLayer = new GauntletLayer("PresetPicker", 2, true);
            _gauntletLayer.InputRestrictions.SetInputRestrictions();
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
            _gauntletLayer.LoadMovie("PresetPicker", _dataSource);

            AddLayer(_gauntletLayer);

            _gauntletLayer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_gauntletLayer);
        }

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);

            if (_gauntletLayer.Input.IsHotKeyReleased("Exit"))
            {
                UISoundsHelper.PlayUISound("event:/ui/default");
                _dataSource.ExecuteCancel();
            }
            else if (_gauntletLayer.Input.IsHotKeyReleased("Confirm") && !_gauntletLayer.IsFocusedOnInput())
            {
                UISoundsHelper.PlayUISound("event:/ui/default");
                _dataSource.ExecuteContinue();
            }
        }

        protected override void OnFinalize()
        {
            base.OnFinalize();

            RemoveLayer(_gauntletLayer);
            _gauntletLayer.IsFocusLayer = false;
            ScreenManager.TryLoseFocus(_gauntletLayer);
            _gauntletLayer = null;

            _dataSource.OnFinalize();
            _dataSource = null;

            _clanCategory.Unload();
            _clanCategory = null;
        }

        private void CloseScreen()
        {
            CharacterScreenFocusAfterPopup.RequestFocusRestore("selector close");
            ScreenManager.PopScreen();
        }

        private void OpenPresetEditor(int slotIndex)
        {
            ScreenManager.PopScreen();

            ScreenManager.PushScreen(new PresetEditorScreen(
                _behavior,
                slotIndex,
                () => ScreenManager.PushScreen(new PresetPickerScreen(_behavior))));
        }
    }
}