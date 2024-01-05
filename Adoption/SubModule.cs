using Adoption.CampaignBehaviors;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Adoption
{
    public class SubModule : MBSubModuleBase
    {
        protected override void InitializeGameStarter(Game game, IGameStarter gameStarterObject)
        {
            if (game.GameType is Campaign && gameStarterObject is CampaignGameStarter campaignGameStarter)
            {
                campaignGameStarter.AddBehavior(new AdoptionCampaignBehavior());
            }
        }
    }
}