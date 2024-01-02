﻿using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using Helpers;

using SandBox;
using SandBox.Conversation;
using SandBox.Missions.AgentBehaviors;

using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Adoption.Behaviors
{
    public class AdoptionCampaignBehavior : CampaignBehaviorBase
    {
        private static readonly AccessTools.FieldRef<Agent, TextObject>? AgentName = AccessTools2.FieldRefAccess<Agent, TextObject>("_name");

        private delegate void SetHeroObjectDelegate(CharacterObject instance, Hero @value);
        private static readonly SetHeroObjectDelegate? SetHeroObject = AccessTools2.GetPropertySetterDelegate<SetHeroObjectDelegate>(typeof(CharacterObject), "HeroObject");

        private delegate void SetHeroStaticBodyPropertiesDelegate(Hero instance, StaticBodyProperties @value);
        private static readonly SetHeroStaticBodyPropertiesDelegate? SetHeroStaticBodyProperties = AccessTools2.GetPropertySetterDelegate<SetHeroStaticBodyPropertiesDelegate>(typeof(Hero), "StaticBodyProperties");

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

            AdoptionState adoptionState;
            if (!_previousAdoptionAttempts.TryGetValue(agent, out adoptionState))
            {
                _previousAdoptionAttempts.Add(agent, AdoptionState.Untested);
            }

            if (adoptionState == AdoptionState.CanAdopt)
            {
                Debug.Print($"Can adopt {agent}");
                return true;
            }
            if (adoptionState == AdoptionState.Ended || adoptionState == AdoptionState.Adopted)
            {
                Debug.Print($"Cannot adopt {agent}");
                return false;
            }

            // TODO: uncomment age flag
            //if (agent.Age < Campaign.Current.Models.AgeModel.HeroComesOfAge)
            //{
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
            //}

            return false;
        }

        private void conversation_adopt_child_on_consequence()
        {
        }

        private void FollowMainAgent()
        {
            DailyBehaviorGroup behaviorGroup = ConversationMission.OneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();

            behaviorGroup.AddBehavior<FollowAgentBehavior>().SetTargetAgent(Agent.Main);
            behaviorGroup.SetScriptedBehavior<FollowAgentBehavior>();
        }

        private void OnHeroAdopted(Hero adopter, Hero adoptedHero)
        {
            TextObject text = new("{=DjzDTNHw}{ADOPTER.LINK} adopted {ADOPTED_HERO.LINK}.");
            text.SetTextVariable("ADOPTER", adopter.Name)
                .SetTextVariable("ADOPTED_HERO", adoptedHero.Name);

            InformationManager.DisplayMessage(new(text.ToString()));
        }

        public void ResetAdoptionAttempts()
        {
            foreach (var pair in _previousAdoptionAttempts)
            {
                if (_previousAdoptionAttempts[pair.Key] == AdoptionState.Ended)
                {
                    _previousAdoptionAttempts[pair.Key] = AdoptionState.Untested;
                }
            }
        }
    }
}