using HarmonyLib.BUTR.Extensions;

using Helpers;

using SandBox;
using SandBox.Conversation;
using SandBox.Missions.AgentBehaviors;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

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

            float adoptionChance = Settings.Instance!.AdoptionChance;
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

            _previousAdoptionAttempts[agent] = AdoptionState.Adopted;

            // Create hero object from character
            Settlement settlement = Hero.MainHero.CurrentSettlement;
            int age = MBMath.ClampInt((int)agent.Age, Campaign.Current.Models.AgeModel.BecomeChildAge, Campaign.Current.Models.AgeModel.HeroComesOfAge);
            Hero hero = HeroCreator.CreateSpecialHero(character, settlement, Clan.PlayerClan, null, age);
            AdoptedHeroCreator.CreateAdoptedHero(hero, settlement);

            // Copy appearance from agent
            SetHeroStaticBodyProperties!(hero, agent.BodyPropertiesValue.StaticProperties);
            hero.Weight = agent.BodyPropertiesValue.Weight;
            hero.Build = agent.BodyPropertiesValue.Build;

            // Agent follows player character
            Campaign.Current.ConversationManager.ConversationEndOneShot += FollowMainAgent;
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