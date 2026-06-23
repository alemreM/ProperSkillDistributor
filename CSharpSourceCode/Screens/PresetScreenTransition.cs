using TaleWorlds.GauntletUI.BaseTypes;

namespace ProperSkillDistributor
{
    public enum PresetScreenTransitionOrigin
    {
        CharacterScreen,
        PresetManager,
        PresetEditor
    }

    internal sealed class PresetScreenTransition
    {
        private readonly Widget _movieRoot;
        private readonly PresetScreenTransitionOrigin _movieScreen;

        private float _timer;
        private float _until;
        private float _alphaOld;
        private float _alphaNew;
        private float _offsetOld;
        private float _offsetNew;

        internal bool IsHoldingScreenSwap { get; private set; }

        internal PresetScreenTransition(Widget movieRoot, PresetScreenTransitionOrigin movieScreen)
        {
            _movieRoot = movieRoot;
            _movieScreen = movieScreen;
        }

        internal void PlayPresetScreenSwap(PresetScreenTransitionOrigin from, PresetScreenTransitionOrigin to)
        {
            var movieIsEntering = to == _movieScreen;
            var offset = 42f;

            if (movieIsEntering)
            {
                if (_movieScreen == PresetScreenTransitionOrigin.PresetManager &&
                    from == PresetScreenTransitionOrigin.PresetEditor)
                {
                    offset = -42f;
                }

                _timer = 0f;
                _until = 0.09f;
                _alphaOld = 0.18f;
                _alphaNew = 1f;
                _offsetOld = offset;
                _offsetNew = 0f;

                _movieRoot.AlphaFactor = _alphaOld;
                _movieRoot.PositionXOffset = _offsetOld;
                IsHoldingScreenSwap = true;
                return;
            }

            if (_movieScreen == PresetScreenTransitionOrigin.PresetManager &&
                to == PresetScreenTransitionOrigin.PresetEditor)
            {
                offset = -42f;
            }
            else if (_movieScreen == PresetScreenTransitionOrigin.PresetEditor &&
                     to == PresetScreenTransitionOrigin.CharacterScreen)
            {
                offset = -42f;
            }

            _timer = 0f;
            _until = 0.08f;
            _alphaOld = _movieRoot.AlphaFactor;
            _alphaNew = 0.18f;
            _offsetOld = _movieRoot.PositionXOffset;
            _offsetNew = offset;

            IsHoldingScreenSwap = true;
        }

        internal bool TickScreenSwap(float dt)
        {
            if (!IsHoldingScreenSwap)
            {
                return false;
            }

            _timer += dt;

            var part = _timer / _until;
            if (part >= 1f)
            {
                _movieRoot.AlphaFactor = _alphaNew;
                _movieRoot.PositionXOffset = _offsetNew;
                IsHoldingScreenSwap = false;
                return true;
            }

            part = part * part * (3f - 2f * part);

            _movieRoot.AlphaFactor = _alphaOld + (_alphaNew - _alphaOld) * part;
            _movieRoot.PositionXOffset = _offsetOld + (_offsetNew - _offsetOld) * part;

            return false;
        }
    }
}