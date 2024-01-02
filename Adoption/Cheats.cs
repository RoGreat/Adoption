using Adoption.Behaviors;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Adoption
{
    internal class Cheats
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("reset_adoption_attempts", "adoption")]
        public static string ResetAdoptionAttempts(List<string> strings)
        {
            if (CampaignCheats.CheckParameters(strings, 0) && !CampaignCheats.CheckHelp(strings))
            {
                Campaign.Current.GetCampaignBehavior<AdoptionCampaignBehavior>().ResetAdoptionAttempts();
                return "Success";
            }
            return "Format is \"adoption.reset_adoption_attempts\"";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("create_parent_for_hero", "adoption")]
        public static string CreateParentForHero(List<string> strings)
        {
            if (CampaignCheats.CheckHelp(strings))
            {
                return "Format is \"adoption.create_parent_for_hero [HeroName]\".";
            }
            string text = CampaignCheats.ConcatenateString(strings);
            Hero hero = CampaignCheats.GetHero(text);
            if (hero is not null)
            {
                string result = "Success";
                int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
                int num = MBRandom.RandomInt(heroComesOfAge + (int)hero.Age, heroComesOfAge * 2 + (int)hero.Age);
                CharacterObject randomElementWithPredicate;
                if (hero.Father is null)
                {
                    randomElementWithPredicate = Hero.MainHero.CurrentSettlement.Culture.NotableAndWandererTemplates.GetRandomElementWithPredicate((CharacterObject x) => x.Occupation == Occupation.Wanderer && !x.IsFemale);
                    hero.Father = HeroCreator.CreateSpecialHero(randomElementWithPredicate, Hero.MainHero.CurrentSettlement, Hero.MainHero.Clan, null, num);
                    KillCharacterAction.ApplyByRemove(hero.Father);
                    return result;
                }
                else if (hero.Mother is null)
                {
                    randomElementWithPredicate = Hero.MainHero.CurrentSettlement.Culture.NotableAndWandererTemplates.GetRandomElementWithPredicate((CharacterObject x) => x.Occupation == Occupation.Wanderer && x.IsFemale);
                    hero.Mother = HeroCreator.CreateSpecialHero(randomElementWithPredicate, Hero.MainHero.CurrentSettlement, Hero.MainHero.Clan, null, num);
                    KillCharacterAction.ApplyByRemove(hero.Mother);
                    return result;
                }
                else
                {
                    return "Hero already has both parents";
                }
            }
            return "Hero is not Found.\nFormat is \"adoption.create_parent_for_hero [HeroName]\".";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("set_adoption_chance", "adoption")]
        public static string SetAdoptionChance(List<string> strings)
        {
            Settings settings = new();
            if (CampaignCheats.CheckParameters(strings, 0) || CampaignCheats.CheckHelp(strings))
            {
                return "Format is \"adoption.set_adoption_chance [0-1]\".";
            }
            if (float.TryParse(strings[0], out float num) && num >= 0f && num <= 1f)
            {
                settings.AdoptionChance = num;
                return "Success";
            }
            return "Format is \"adoption.set_adoption_chance [0-1]\".";
        }
    }
}
