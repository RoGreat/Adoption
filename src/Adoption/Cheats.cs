using Adoption.CampaignBehaviors;

using Helpers;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Adoption
{
    public static class Cheats
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("reset_adoption_attempts", "adoption")]
        public static string ResetAdoptionAttempts(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            {
                return CampaignCheats.ErrorType;
            }
            if (CampaignCheats.CheckParameters(strings, 0) && !CampaignCheats.CheckHelp(strings))
            {
                AdoptionCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<AdoptionCampaignBehavior>();
                if (campaignBehavior is null)
                {
                    return "Can not find Adoption Campaign Behavior!";
                }
                campaignBehavior.ResetAdoptionAttempts();
                return "Success";
            }
            return "Format is \"adoption.reset_adoption_attempts\"";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("set_adoption_chance", "adoption")]
        public static string SetAdoptionChance(List<string> strings)
        {
            if (CampaignCheats.CheckParameters(strings, 0) || CampaignCheats.CheckHelp(strings))
            {
                return "Format is \"adoption.set_adoption_chance [0-1]\".";
            }
            if (float.TryParse(strings[0], out float num) && num >= 0f && num <= 1f)
            {
                Settings.Instance!.AdoptionChance = num;
                return "Success";
            }
            return "Format is \"adoption.set_adoption_chance [0-1]\".";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("adopt_children", "adoption")]
        public static string AdoptChildren(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            {
                return CampaignCheats.ErrorType;
            }
            string text = "Format is \"adoption.adopt_children [Number]\".";
            if (!CampaignCheats.CheckParameters(strings, 1) || CampaignCheats.CheckHelp(strings))
            {
                return text;
            }
            if (!int.TryParse(strings[0], out int num))
            {
                return "Invalid number.\n" + text;
            }
            if (num <= 0)
            {
                return "Please enter a positive number\n" + text;
            }
            for (int i = 0; i < num; i++)
            {
                AdoptChild(new List<string>());
            }
            return "Success";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("adopt_child", "adoption")]
        public static string AdoptChild(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            {
                return CampaignCheats.ErrorType;
            }
            if (!CampaignCheats.CheckParameters(strings, 0) || CampaignCheats.CheckHelp(strings))
            {
                return "Format is \"adoption.adopt_child\".";
            }

            // Create hero from child character templates
            Settlement settlement = SettlementHelper.GetRandomTown();
            int age = MBRandom.RandomInt(Campaign.Current.Models.AgeModel.BecomeChildAge, Campaign.Current.Models.AgeModel.HeroComesOfAge);
            List<CharacterObject> character = new()
            {
                settlement.Culture.VillagerMaleChild,
                settlement.Culture.VillagerFemaleChild,
            };
            // OnHeroCreated event is fired at end of CreateSpecialHero
            // refer to AgingCampaignBehavior -> OnHeroCreated for the list.
            // Must also fulfill DefaultCutscenesCampaignBehavior -> OnHeroComesOfAge
            // which means there must be:
            // A) a mother and father
            // B) both from Player Clan
            Hero hero = HeroCreator.CreateSpecialHero(character.GetRandomElement(), settlement, Clan.PlayerClan, null, age);
            AdoptedHeroCreator.CreateAdoptedHero(hero, settlement);

            return "child has been adopted.";
        }
    }
}
