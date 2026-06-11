using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.TwoDimension;

namespace ProperSkillDistributor
{
    internal sealed class CharacterDeveloperSpriteScope
    {
        private SpriteCategory _characterDeveloperSprites;
        private SpriteCategory _navalCharacterDeveloperSprites;

        private bool _unloadCharacterDeveloperSprites;
        private bool _unloadNavalCharacterDeveloperSprites;

        public void Load()
        {
            _characterDeveloperSprites = UIResourceManager.GetSpriteCategory("ui_characterdeveloper");
            _unloadCharacterDeveloperSprites = _characterDeveloperSprites != null && !_characterDeveloperSprites.IsLoaded;

            if (_unloadCharacterDeveloperSprites)
            {
                _characterDeveloperSprites.Load(UIResourceManager.ResourceContext, UIResourceManager.ResourceDepot);
            }

            _navalCharacterDeveloperSprites = UIResourceManager.GetSpriteCategory("ui_naval_character_developer");
            _unloadNavalCharacterDeveloperSprites = _navalCharacterDeveloperSprites != null && !_navalCharacterDeveloperSprites.IsLoaded;

            if (_unloadNavalCharacterDeveloperSprites)
            {
                _navalCharacterDeveloperSprites.Load(UIResourceManager.ResourceContext, UIResourceManager.ResourceDepot);
            }
        }

        public void Unload()
        {
            if (_navalCharacterDeveloperSprites != null && _unloadNavalCharacterDeveloperSprites)
            {
                _navalCharacterDeveloperSprites.Unload();
            }

            if (_characterDeveloperSprites != null && _unloadCharacterDeveloperSprites)
            {
                _characterDeveloperSprites.Unload();
            }

            _navalCharacterDeveloperSprites = null;
            _characterDeveloperSprites = null;

            _unloadNavalCharacterDeveloperSprites = false;
            _unloadCharacterDeveloperSprites = false;
        }
    }
}