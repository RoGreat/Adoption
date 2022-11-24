using HarmonyLib;
using Helpers;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using SandBox;
using SandBox.Conversation;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.Library;
using static Adoption.Debug;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using HarmonyLib.BUTR.Extensions;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Actions;

namespace Adoption.Behaviors
{
    internal class AdoptionCampaignBehavior : CampaignBehaviorBase
    {
        private List<Agent>? _adoptableAgents;

        private List<Agent>? _notAdoptableAgents;

        /* Outside */
        /* Fields */
        private static readonly AccessTools.FieldRef<Agent, TextObject>? AgentName = AccessTools2.FieldRefAccess<Agent, TextObject>("_name");

        /* Property Setters */
        private delegate void SetHeroObjectDelegate(CharacterObject instance, Hero @value);
        private static readonly SetHeroObjectDelegate? SetHeroObject = AccessTools2.GetPropertySetterDelegate<SetHeroObjectDelegate>(typeof(CharacterObject), "HeroObject");

        private delegate void SetHeroStaticBodyPropertiesDelegate(Hero instance, StaticBodyProperties @value);
        private static readonly SetHeroStaticBodyPropertiesDelegate? SetHeroStaticBodyProperties = AccessTools2.GetPropertySetterDelegate<SetHeroStaticBodyPropertiesDelegate>(typeof(Hero), "StaticBodyProperties");

        /* Methods */
        private delegate void CompanionAdjustEquipmentDelegate(CompanionsCampaignBehavior instance, Hero companion);
        private static readonly CompanionAdjustEquipmentDelegate? CompanionAdjustEquipment = AccessTools2.GetDelegate<CompanionAdjustEquipmentDelegate>(typeof(CompanionsCampaignBehavior), "AdjustEquipment");

        public AdoptionCampaignBehavior()
        {
            _adoptableAgents = new();
            _notAdoptableAgents = new();
        }

        protected void AddDialogs(CampaignGameStarter starter)
        {
            // Children
            starter.AddPlayerLine("adoption_discussion", "town_or_village_children_player_no_rhyme", "adoption_child", "{=adoption_offer_child}I can tell you have no parents to go back to child. I can be your {?PLAYER.GENDER}mother{?}father{\\?} if that is the case.", new ConversationSentence.OnConditionDelegate(conversation_adopt_child_on_condition), null, 120, null, null);
            starter.AddDialogLine("character_adoption_response", "adoption_child", "close_window", "{=adoption_response_child}You want to be my {?PLAYER.GENDER}Ma{?}Pa{\\?}? Okay then![rf:happy][rb:very_positive]", null, new ConversationSentence.OnConsequenceDelegate(conversation_adopt_child_on_consequence), 100, null);
            // Teens
            starter.AddPlayerLine("adoption_discussion", "town_or_village_player", "adoption_teen", "{=adoption_offer_teen}Do you not have any parents to take care of you young {?CONVERSATION_CHARACTER.GENDER}woman{?}man{\\?}? You are welcome to be a part of my family.", new ConversationSentence.OnConditionDelegate(conversation_adopt_child_on_condition), null, 120, null, null);
            starter.AddDialogLine("character_adoption_response", "adoption_teen", "close_window", "{=adoption_response_teen}Thanks for allowing me to be a part of your family {?PLAYER.GENDER}milady{?}sir{\\?}. I gratefully accept![rf:happy][rb:very_positive]", null, new ConversationSentence.OnConsequenceDelegate(conversation_adopt_child_on_consequence), 100, null);
        }

        private void RemoveAgents()
        {
            // If the list is empty then return
            if (_notAdoptableAgents.IsEmpty() && _adoptableAgents.IsEmpty())
            {
                return;
            }
            // Clear all items in _heroes if none of the agents are in the current mission
            foreach (Agent agent in Mission.Current.Agents)
            {
                // If there is an agent present then return
                if (_notAdoptableAgents!.Contains(agent))
                {
                    return;
                }
                if (_adoptableAgents!.Contains(agent))
                {
                    return;
                }
            }
            _notAdoptableAgents!.Clear();
            _adoptableAgents!.Clear();
        }

        private bool conversation_adopt_child_on_condition()
        {
            ModSettings settings = new();
            StringHelpers.SetCharacterProperties("CONVERSATION_CHARACTER", CharacterObject.OneToOneConversationCharacter);

            Agent conversationAgent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;

            if (_notAdoptableAgents!.Contains(conversationAgent))
            {
                Print("Cannot Adopt");
                return false;
            }
            if (_adoptableAgents!.Contains(conversationAgent))
            {
                Print("Can Adopt");
                return true;
            }
            if (Campaign.Current.ConversationManager.OneToOneConversationAgent.Age < Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                Print("Adoption Chance: " + settings.AdoptionChance);
                // You only roll once!
                float random = MBRandom.RandomFloat;
                Print("Random Number: " + random);
                if (random < settings.AdoptionChance)
                {
                    _adoptableAgents.Add(conversationAgent);
                    return true;
                }
                else
                {
                    _notAdoptableAgents.Add(conversationAgent);
                }
            }
            return false;
        }

        // HeroCreator -> DeliverOffSpring
        private void conversation_adopt_child_on_consequence()
        {
            ModSettings settings = new();

            Agent conversationAgent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;

            // Add to non adoptable agents to not adopt same kid again
            _notAdoptableAgents!.Add(conversationAgent);

            Agent agent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;
            CharacterObject character = CharacterObject.OneToOneConversationCharacter;

            CharacterObject conversationCharacter = Campaign.Current.ConversationManager.OneToOneConversationCharacter;

            // Remove agents not in scene
            RemoveAgents();

            CharacterObject template = conversationCharacter;

            // Create a new hero!
            Hero hero = HeroCreator.CreateSpecialHero(template, Hero.MainHero.CurrentSettlement, null, null, (int)conversationAgent.Age);

            int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
            int becomeChildAge = Campaign.Current.Models.AgeModel.BecomeChildAge;

            // Mother and father assignment
            // Necessary for an education

            // NotableWantsDaughterFoundIssueBehavior -> NotableWantsDaughterFoundIssueQuest
            int num = MBRandom.RandomInt(heroComesOfAge + (int)hero.Age, heroComesOfAge * 2 + (int)hero.Age);
            CharacterObject randomElementWithPredicate;

            if (Hero.MainHero.IsFemale)
            {
                // You
                hero.Mother = Hero.MainHero;
                // Randomly generate a father
                randomElementWithPredicate = Hero.MainHero.CurrentSettlement.Culture.NotableAndWandererTemplates.GetRandomElementWithPredicate((CharacterObject x) => x.Occupation == Occupation.Wanderer && !x.IsFemale);
                hero.Father = HeroCreator.CreateSpecialHero(randomElementWithPredicate, Hero.MainHero.CurrentSettlement, Hero.MainHero.Clan, null, num);
                KillCharacterAction.ApplyByRemove(hero.Father);
            }
            else
            {
                // You
                hero.Father = Hero.MainHero;
                // Randomly generate a mother
                randomElementWithPredicate = Hero.MainHero.CurrentSettlement.Culture.NotableAndWandererTemplates.GetRandomElementWithPredicate((CharacterObject x) => x.Occupation == Occupation.Wanderer && x.IsFemale);
                hero.Mother = HeroCreator.CreateSpecialHero(randomElementWithPredicate, Hero.MainHero.CurrentSettlement, Hero.MainHero.Clan, null, num);
                KillCharacterAction.ApplyByRemove(hero.Mother);
            }

            EquipmentFlags customFlags = EquipmentFlags.IsNobleTemplate | EquipmentFlags.IsChildEquipmentTemplate;
            MBEquipmentRoster randomElementInefficiently = MBEquipmentRosterExtensions.GetAppropriateEquipmentRostersForHero(hero, customFlags, true).GetRandomElementInefficiently();
            if (randomElementInefficiently != null)
            {
                Equipment randomElementInefficiently2 = randomElementInefficiently.GetCivilianEquipments().GetRandomElementInefficiently();
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, randomElementInefficiently2);
                Equipment equipment = new(false);
                equipment.FillFrom(randomElementInefficiently2, false);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment);
            }

            // Name permanence from the adoption module of old
            //AgentName!.SetValue(conversationAgent, _companionHero.Name);
            AgentName!(conversationAgent) = hero.Name;

            // Meet character for first time
            hero.HasMet = true;

            // Give hero the agent's appearance
            //SetHeroStaticBodyProperties!.SetValue(_companionHero, conversationAgent.BodyPropertiesValue.StaticProperties);
            SetHeroStaticBodyProperties!(hero, conversationAgent.BodyPropertiesValue.StaticProperties);

            HeroHelper.DetermineInitialLevel(hero);

            CharacterDevelopmentCampaignBehavior characterDevelopmentCampaignBehaviorInstance = Campaign.Current.CampaignBehaviorManager.GetBehavior<CharacterDevelopmentCampaignBehavior>();
            characterDevelopmentCampaignBehaviorInstance.DevelopCharacterStats(hero);

            //character.HeroObject = _companionHero;
            //SetHeroObject!.SetValue(conversationCharacter, _companionHero);
            SetHeroObject!(conversationCharacter, hero);

            // Cool idea. Might put this into Recruit Everyone, too!
            AccessTools.Field(typeof(Agent), "_name").SetValue(agent, hero.Name);
            OnHeroAdopted(Hero.MainHero, hero);
            // Follows you! I like this feature :3
            Campaign.Current.ConversationManager.ConversationEndOneShot += FollowMainAgent;

            if (hero.Age > becomeChildAge || (hero.Age == becomeChildAge && hero.BirthDay.GetDayOfYear < CampaignTime.Now.GetDayOfYear))
            {
                CampaignEventDispatcher.Instance.OnHeroGrowsOutOfInfancy(hero);
            }
            if (hero.Age > heroComesOfAge || (hero.Age == heroComesOfAge && hero.BirthDay.GetDayOfYear < CampaignTime.Now.GetDayOfYear))
            {
                CampaignEventDispatcher.Instance.OnHeroComesOfAge(hero);
            }
        }

        private static void FollowMainAgent()
        {
            DailyBehaviorGroup behaviorGroup = ConversationMission.OneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
            behaviorGroup.AddBehavior<FollowAgentBehavior>().SetTargetAgent(Agent.Main);
            behaviorGroup.SetScriptedBehavior<FollowAgentBehavior>();
        }

        // PregnancyCampaignBehavior -> CheckOffspringsToDeliver
        private void OnHeroAdopted(Hero adopter, Hero adoptedHero)
        {
            TextObject textObject = new("{=adopted}{ADOPTER.LINK} adopted {ADOPTED_HERO.LINK}.", null);
            StringHelpers.SetCharacterProperties("ADOPTER", adopter.CharacterObject, textObject);
            StringHelpers.SetCharacterProperties("ADOPTED_HERO", adoptedHero.CharacterObject, textObject);
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString()));
        }

        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialogs(campaignGameStarter);
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}