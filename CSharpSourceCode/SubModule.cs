using System.Reflection;
using Bannerlord.UIExtenderEx;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace ProperSkillDistributor
{
    public class SubModule : MBSubModuleBase
    {
        private static bool _harmonyPatched;

        private static UIExtender _uiExtender;
        private static bool _uiExtenderEnabled;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            if (!_harmonyPatched)
            {
                new Harmony("proper_skill_distributor").PatchAll();
                _harmonyPatched = true;
            }

            if (!_uiExtenderEnabled)
            {
                _uiExtender = new UIExtender("ProperSkillDistributor");
                _uiExtender.Register(Assembly.GetExecutingAssembly());
                _uiExtender.Enable();
                _uiExtenderEnabled = true;
            }
        }

        protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
        {
            base.InitializeGameStarter(game, starterObject);

            if (game.GameType is Campaign && starterObject is CampaignGameStarter campaignGameStarter)
            {
                campaignGameStarter.AddBehavior(new SkillPresetBehavior());
            }
        }
    }
}
