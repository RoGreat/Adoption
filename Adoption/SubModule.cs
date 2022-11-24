using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using Adoption.Settings;
using Adoption.Behaviors;

namespace Adoption
{
    public class SubModule : MBSubModuleBase
    {
        internal static string ModTitle = "Adoption";

        internal static string ModId = "Adoption";

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            new Harmony("mod.bannerlord.adoption").PatchAll();
            ModConfig.Initialize();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if (game.GameType is Campaign)
            {
                CampaignGameStarter campaignGameStarter = (CampaignGameStarter)gameStarterObject;
                campaignGameStarter.AddBehavior(new AdoptionCampaignBehavior());
            }
        }
    }
}