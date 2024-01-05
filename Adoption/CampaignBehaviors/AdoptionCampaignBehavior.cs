using HarmonyLib.BUTR.Extensions;

using Helpers;

using SandBox;
using SandBox.Conversation;
using SandBox.Missions.AgentBehaviors;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.TwoDimension;

namespace Adoption.CampaignBehaviors
{
    public class AdoptionCampaignBehavior : CampaignBehaviorBase
    {
        public delegate void SetHeroStaticBodyPropertiesDelegate(Hero instance, StaticBodyProperties @value);
        public readonly SetHeroStaticBodyPropertiesDelegate? SetHeroStaticBodyProperties = AccessTools2.GetPropertySetterDelegate<SetHeroStaticBodyPropertiesDelegate>(typeof(Hero), "StaticBodyProperties");

        private readonly Dictionary<Agent, AdoptionState> _previousAdoptionAttempts = new();

        private enum AdoptionState
        {
            Ended = -1,
            Untested,
            CanAdopt,
            Adopted,
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore) { }

        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            StringHelpers.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject);
            AddDialogs(campaignGameStarter);
        }

        public void AddDialogs(CampaignGameStarter starter)
        {
            AddChildrenDialogs(starter);
            AddTeenagerDialogs(starter);
        }

        protected void AddChildrenDialogs(CampaignGameStarter starter)
        {
            starter.AddPlayerLine(
                "adoption_discussion",
                "town_or_village_children_player_no_rhyme", "adoption_child",
                "{=Sm4JdIxx}I can tell you have no parents to go back to child. I can be your {?PLAYER.GENDER}mother{?}father{\\?} if that is the case.",
                conversation_adopt_child_on_condition, null, 120);
            starter.AddDialogLine(
                "character_adoption_response",
                "adoption_child", "close_window",
                "{=P2m6bJg6}You want to be my {?PLAYER.GENDER}mom{?}dad{\\?}? Okay then![rf:happy][rb:very_positive]",
                null, conversation_adopt_child_on_consequence, 100);
        }

        protected void AddTeenagerDialogs(CampaignGameStarter starter)
        {
            starter.AddPlayerLine(
                "adoption_discussion",
                "town_or_village_player", "adoption_teen",
                "{=Na4j2oGk}Do you not have any parents to take care of you young {?CONVERSATION_CHARACTER.GENDER}woman{?}man{\\?}? You are welcome to be a part of my family.",
                conversation_adopt_child_on_condition, null, 120);
            starter.AddDialogLine(
                "character_adoption_response",
                "adoption_teen", "close_window",
                "{=NoHJAxWx}Thanks for allowing me to be a part of your family {?PLAYER.GENDER}madam{?}sir{\\?}. I gratefully accept![rf:happy][rb:very_positive]",
                null, conversation_adopt_child_on_consequence, 100);
        }

        private bool conversation_adopt_child_on_condition()
        {
            Agent agent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;

            if (agent.Age >= Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                return false;
            }

            if (!_previousAdoptionAttempts.TryGetValue(agent, out AdoptionState adoptionState))
            {
                // Clear out any old attempts
                RemoveUnneededAdoptionAttempts();

                _previousAdoptionAttempts.Add(agent, AdoptionState.Untested);
            }

            if (adoptionState == AdoptionState.CanAdopt)
            {
                return true;
            }
            if (adoptionState == AdoptionState.Ended || adoptionState == AdoptionState.Adopted)
            {
                return false;
            }

            double adoptionChance = Settings.Instance!.AdoptionChance;
            Debug.Print($"Adoption chance: {adoptionChance}");
               
            float random = MBRandom.RandomFloat;
            Debug.Print($"Random number: {random}");

            if (random < adoptionChance)
            {
                Debug.Print($"Can adopt {agent}");
                _previousAdoptionAttempts[agent] = AdoptionState.CanAdopt;
                return true;
            }
            else
            {
                Debug.Print($"Cannot adopt {agent}");
                _previousAdoptionAttempts[agent] = AdoptionState.Ended;
            }
            return false;
        }

        private void conversation_adopt_child_on_consequence()
        {
            Agent agent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;
            CharacterObject character = Campaign.Current.ConversationManager.OneToOneConversationCharacter;

            int becomeChildAge = Campaign.Current.Models.AgeModel.BecomeChildAge;
            int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;

            _previousAdoptionAttempts[agent] = AdoptionState.Adopted;

            // Create hero object for agent
            Settlement settlement = Hero.MainHero.CurrentSettlement;
            Hero hero = HeroCreator.CreateSpecialHero(character, settlement, Clan.PlayerClan, null, (int)Mathf.Clamp(agent.Age, becomeChildAge, heroComesOfAge));

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

            // Appearance
            SetHeroStaticBodyProperties!(hero, agent.BodyPropertiesValue.StaticProperties);
            hero.Weight = agent.BodyPropertiesValue.Weight;
            hero.Build = agent.BodyPropertiesValue.Build;

            // Notification of adoption
            OnHeroAdopted(Hero.MainHero, hero);

            // Agent will follow the adopter
            Campaign.Current.ConversationManager.ConversationEndOneShot += FollowMainAgent;
        }

        public void CreateRandomLostParent(Hero hero, Settlement settlement)
        {
            int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
            int age = MBRandom.RandomInt(heroComesOfAge + (int)hero.Age, heroComesOfAge * 2 + (int)hero.Age);
            CharacterObject randomElementWithPredicate;

            if (Hero.MainHero.IsFemale)
            {
                randomElementWithPredicate = settlement.Culture.NotableAndWandererTemplates.GetRandomElementWithPredicate((CharacterObject x) => x.Occupation == Occupation.Wanderer && !x.IsFemale);
                hero.Father = HeroCreator.CreateSpecialHero(randomElementWithPredicate, hero.CurrentSettlement, null, null, age);
                hero.Father.CharacterObject.HiddenInEncylopedia = true;
                KillCharacterAction.ApplyByRemove(hero.Father);
            }
            else
            {
                randomElementWithPredicate = settlement.Culture.NotableAndWandererTemplates.GetRandomElementWithPredicate((CharacterObject x) => x.Occupation == Occupation.Wanderer && x.IsFemale);
                hero.Mother = HeroCreator.CreateSpecialHero(randomElementWithPredicate, hero.CurrentSettlement, null, null, age);
                hero.Mother.CharacterObject.HiddenInEncylopedia = true;
                KillCharacterAction.ApplyByRemove(hero.Mother);
            }
        }

        public void RemoveUnneededAdoptionAttempts()
        {
            foreach (var pair in _previousAdoptionAttempts.ToList())
            {
                if (!Mission.Current.Agents.Contains(pair.Key))
                {
                    _previousAdoptionAttempts.Remove(pair.Key);
                }
            }
        }

        public void FollowMainAgent()
        {
            DailyBehaviorGroup behaviorGroup = ConversationMission.OneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
            FollowAgentBehavior followAgentBehavior = behaviorGroup.AddBehavior<FollowAgentBehavior>();
            behaviorGroup.SetScriptedBehavior<FollowAgentBehavior>();
            followAgentBehavior.SetTargetAgent(Agent.Main);
        }

        public void OnHeroAdopted(Hero adopter, Hero adoptedHero)
        {
            TextObject text = new("{=DjzDTNHw}{ADOPTER.LINK} adopted {ADOPTED_HERO.LINK}.");
            StringHelpers.SetCharacterProperties("ADOPTER", adopter.CharacterObject, text);
            StringHelpers.SetCharacterProperties("ADOPTED_HERO", adoptedHero.CharacterObject, text);

            InformationManager.DisplayMessage(new(text.ToString()));
        }

        public void ResetAdoptionAttempts()
        {
            foreach (var pair in _previousAdoptionAttempts.ToList())
            {
                if (_previousAdoptionAttempts[pair.Key] == AdoptionState.Ended)
                {
                    _previousAdoptionAttempts[pair.Key] = AdoptionState.Untested;
                }
            }
        }
    }
}