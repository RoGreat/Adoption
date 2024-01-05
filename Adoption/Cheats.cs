using Adoption.CampaignBehaviors;

using Helpers;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
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
            AdoptionCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<AdoptionCampaignBehavior>();
            if (campaignBehavior is null)
            {
                return "Can not find Adoption Campaign Behavior!";
            }

            // Create hero from child character templates
            Settlement settlement = SettlementHelper.GetRandomTown();
            int age = MBRandom.RandomInt(Campaign.Current.Models.AgeModel.BecomeChildAge, Campaign.Current.Models.AgeModel.HeroComesOfAge);
            List<CharacterObject> character = new()
            {
                settlement.Culture.VillagerMaleChild,
                settlement.Culture.VillagerFemaleChild,
            };
            Hero hero = HeroCreator.CreateSpecialHero(character.GetRandomElement(), settlement, Clan.PlayerClan, null, age);

            // Parent assignments
            if (Hero.MainHero.IsFemale)
            {
                hero.Mother = Hero.MainHero;
            }
            else
            {
                hero.Father = Hero.MainHero;
            }
            campaignBehavior.CreateRandomLostParent(hero, settlement);
            Hero mother = hero.Mother;
            Hero father = hero.Father;

            // Traits from DeliverOffspring method
            hero.ClearTraits();
            float randomFloat = MBRandom.RandomFloat;
            int num;
            if ((double)randomFloat < 0.1)
            {
                num = 0;
            }
            else if ((double)randomFloat < 0.5)
            {
                num = 1;
            }
            else if ((double)randomFloat < 0.9)
            {
                num = 2;
            }
            else
            {
                num = 3;
            }
            List<TraitObject> list = DefaultTraits.Personality.ToList();
            list.Shuffle();
            for (int i = 0; i < Math.Min(list.Count, num); i++)
            {
                int num2 = ((double)MBRandom.RandomFloat < 0.5) ? MBRandom.RandomInt(list[i].MinValue, 0) : MBRandom.RandomInt(1, list[i].MaxValue + 1);
                hero.SetTraitLevel(list[i], num2);
            }
            foreach (TraitObject traitObject in TraitObject.All.Except(DefaultTraits.Personality))
            {
                hero.SetTraitLevel(traitObject, ((double)MBRandom.RandomFloat < 0.5) ? mother.GetTraitLevel(traitObject) : father.GetTraitLevel(traitObject));
            }

            // Common updates after creating hero
            hero.SetNewOccupation(Occupation.Lord);
            hero.ChangeState(Hero.CharacterStates.Active);
            hero.UpdateHomeSettlement();
            hero.HeroDeveloper.InitializeHeroDeveloper(true, null);

            // GetEquipmentRostersForInitialChildrenGeneration uses different equipment templates depending on age
            MBEquipmentRoster randomElementInefficiently = Campaign.Current.Models.EquipmentSelectionModel.GetEquipmentRostersForInitialChildrenGeneration(hero).GetRandomElementInefficiently();
            if (randomElementInefficiently is not null)
            {
                Equipment randomCivilianEquipment = randomElementInefficiently.GetRandomCivilianEquipment();
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, randomCivilianEquipment);
                Equipment equipment = new(false);
                equipment.FillFrom(randomCivilianEquipment, false);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment);
            }

            // Notification of adoption
            Campaign.Current.GetCampaignBehavior<AdoptionCampaignBehavior>().OnHeroAdopted(Hero.MainHero, hero);

            return "child has been adopted.";
        }
    }
}
