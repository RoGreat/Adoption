using Helpers;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Extensions;

namespace Adoption
{
    public static class AdoptedHeroCreator
    {
        public static void CreateAdoptedHero(Hero hero, Settlement settlement)
        {
            // Parent assignments
            if (Hero.MainHero.IsFemale)
            {
                hero.Mother = Hero.MainHero;
            }
            else
            {
                hero.Father = Hero.MainHero;
            }
            CreateRandomLostParent(hero, settlement);
            // Traits derived from DeliverOffSpring method
            LordTraits(hero, hero.Mother, hero.Father);

            // Common updates after creating hero
            hero.SetNewOccupation(Occupation.Lord);
            hero.UpdateHomeSettlement();
            hero.HeroDeveloper.InitializeHeroDeveloper(true, null);
            // Equipment derived from OnNewGameCreatedPartialFollowUp
            EquipmentForChild(hero);

            // debug code for testing the cutscene trigger
            //hero.SetBirthDay(CampaignTime.Days(CampaignTime.Now.GetDayOfYear + 1) + CampaignTime.Years(CampaignTime.Now.GetYear - Campaign.Current.Models.AgeModel.HeroComesOfAge));

            // Custom notification for adoption
            OnHeroAdopted(Hero.MainHero, hero);
        }

        public static void CreateRandomLostParent(Hero hero, Settlement settlement)
        {
            int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
            int age = MBRandom.RandomInt(heroComesOfAge + (int)hero.Age, heroComesOfAge * 2 + (int)hero.Age);
            CharacterObject randomElementWithPredicate;

            if (Hero.MainHero.IsFemale)
            {
                randomElementWithPredicate = settlement.Culture.NotableAndWandererTemplates.GetRandomElementWithPredicate((CharacterObject x) => x.Occupation == Occupation.Wanderer && !x.IsFemale);
                hero.Father = HeroCreator.CreateSpecialHero(randomElementWithPredicate, hero.CurrentSettlement, Clan.PlayerClan, null, age);
                hero.Father.CharacterObject.HiddenInEncylopedia = true;
                KillCharacterAction.ApplyByRemove(hero.Father);
            }
            else
            {
                randomElementWithPredicate = settlement.Culture.NotableAndWandererTemplates.GetRandomElementWithPredicate((CharacterObject x) => x.Occupation == Occupation.Wanderer && x.IsFemale);
                hero.Mother = HeroCreator.CreateSpecialHero(randomElementWithPredicate, hero.CurrentSettlement, Clan.PlayerClan, null, age);
                hero.Mother.CharacterObject.HiddenInEncylopedia = true;
                KillCharacterAction.ApplyByRemove(hero.Mother);
            }
        }

        public static void LordTraits(Hero hero, Hero mother, Hero father)
        {
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
        }

        public static void EquipmentForChild(Hero hero)
        {
            // GetEquipmentRostersForInitialChildrenGeneration uses different equipment templates depending on if they are a child or teenager
            MBEquipmentRoster randomElementInefficiently = Campaign.Current.Models.EquipmentSelectionModel.GetEquipmentRostersForInitialChildrenGeneration(hero).GetRandomElementInefficiently();
            if (randomElementInefficiently is not null)
            {
                Equipment randomCivilianEquipment = randomElementInefficiently.GetRandomCivilianEquipment();
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, randomCivilianEquipment);
                Equipment equipment = new(false);
                equipment.FillFrom(randomCivilianEquipment, false);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment);
            }
        }

        public static void OnHeroAdopted(Hero adopter, Hero adoptedHero)
        {
            StringHelpers.SetCharacterProperties("ADOPTER", adopter.CharacterObject);
            StringHelpers.SetCharacterProperties("ADOPTED_HERO", adoptedHero.CharacterObject);
            MBInformationManager.AddQuickInformation(new("{=DjzDTNHw}{ADOPTER.LINK} adopted {ADOPTED_HERO.LINK}."));
        }
    }
}
