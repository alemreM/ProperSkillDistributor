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
        private enum CloseTarget
        {
            None,
            CharacterScreen,
            PresetEditor
        }

        private readonly SkillPresetBehavior _behavior;
        private readonly PresetScreenTransitionOrigin _transitionOrigin;

        private SpriteCategory _clanCategory;
        private PresetPickerVM _dataSource;
        private GauntletLayer _gauntletLayer;
        private PresetScreenTransition _screenTransition;
        private CloseTarget _closeTarget;
        private int _slotIndexForEditor;

        public PresetPickerScreen(
            SkillPresetBehavior behavior,
            PresetScreenTransitionOrigin transitionOrigin = PresetScreenTransitionOrigin.CharacterScreen)
        {
            _behavior = behavior;
            _transitionOrigin = transitionOrigin;
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

            _screenTransition = new PresetScreenTransition(
                _gauntletLayer.GetMovieIdentifier("PresetPicker").Movie.RootWidget,
                PresetScreenTransitionOrigin.PresetManager);

            _screenTransition.PlayPresetScreenSwap(
                _transitionOrigin,
                PresetScreenTransitionOrigin.PresetManager);

            AddLayer(_gauntletLayer);

            _gauntletLayer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_gauntletLayer);
        }

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);

            if (TickScreenTransition(dt))
            {
                return;
            }

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
            _screenTransition = null;

            _dataSource.OnFinalize();
            _dataSource = null;

            _clanCategory.Unload();
            _clanCategory = null;
        }

        private bool TickScreenTransition(float dt)
        {
            if (_screenTransition == null || !_screenTransition.IsHoldingScreenSwap)
            {
                return false;
            }

            bool transitionFinished = _screenTransition.TickScreenSwap(dt);

            if (transitionFinished && _closeTarget != CloseTarget.None)
            {
                FinishCloseTransition();
            }

            return true;
        }

        private void CloseScreen()
        {
            if (_closeTarget != CloseTarget.None)
            {
                return;
            }

            _closeTarget = CloseTarget.CharacterScreen;
            CharacterScreenFocusAfterPopup.RequestFocusRestore("selector close");

            _screenTransition.PlayPresetScreenSwap(
                PresetScreenTransitionOrigin.PresetManager,
                PresetScreenTransitionOrigin.CharacterScreen);
        }

        private void OpenPresetEditor(int slotIndex)
        {
            if (_closeTarget != CloseTarget.None)
            {
                return;
            }

            _slotIndexForEditor = slotIndex;
            _closeTarget = CloseTarget.PresetEditor;

            _screenTransition.PlayPresetScreenSwap(
                PresetScreenTransitionOrigin.PresetManager,
                PresetScreenTransitionOrigin.PresetEditor);
        }

        private void FinishCloseTransition()
        {
            CloseTarget finishedTarget = _closeTarget;
            _closeTarget = CloseTarget.None;

            ScreenManager.PopScreen();

            if (finishedTarget == CloseTarget.PresetEditor)
            {
                ScreenManager.PushScreen(new PresetEditorScreen(
                    _behavior,
                    _slotIndexForEditor,
                    () => ScreenManager.PushScreen(new PresetPickerScreen(_behavior, PresetScreenTransitionOrigin.PresetEditor)),
                    PresetScreenTransitionOrigin.PresetManager));
            }
        }
    }
}