using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using Helpers;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

using SandBox;
using SandBox.Conversation;
using SandBox.Missions.AgentBehaviors;

namespace Adoption.Behaviors
{
    internal class AdoptionCampaignBehavior : CampaignBehaviorBase
    {
        private static readonly AccessTools.FieldRef<Agent, TextObject>? AgentName = AccessTools2.FieldRefAccess<Agent, TextObject>("_name");

        private delegate void SetHeroObjectDelegate(CharacterObject instance, Hero @value);
        private static readonly SetHeroObjectDelegate? SetHeroObject = AccessTools2.GetPropertySetterDelegate<SetHeroObjectDelegate>(typeof(CharacterObject), "HeroObject");

        private delegate void SetHeroStaticBodyPropertiesDelegate(Hero instance, StaticBodyProperties @value);
        private static readonly SetHeroStaticBodyPropertiesDelegate? SetHeroStaticBodyProperties = AccessTools2.GetPropertySetterDelegate<SetHeroStaticBodyPropertiesDelegate>(typeof(Hero), "StaticBodyProperties");

        public AdoptionCampaignBehavior()
        {
        }

        protected void AddDialogs(CampaignGameStarter starter)
        {
            // Children
            starter.AddPlayerLine("adoption_discussion", "town_or_village_children_player_no_rhyme", "adoption_child", "{=Sm4JdIxx}I can tell you have no parents to go back to child. I can be your {?PLAYER.GENDER}mother{?}father{\\?} if that is the case.", new ConversationSentence.OnConditionDelegate(conversation_adopt_child_on_condition), null, 120, null, null);
            starter.AddDialogLine("character_adoption_response", "adoption_child", "close_window", "{=P2m6bJg6}You want to be my {?PLAYER.GENDER}Ma{?}Pa{\\?}? Okay then![rf:happy][rb:very_positive]", null, new ConversationSentence.OnConsequenceDelegate(conversation_adopt_child_on_consequence), 100, null);
            // Teens
            starter.AddPlayerLine("adoption_discussion", "town_or_village_player", "adoption_teen", "{=Na4j2oGk}Do you not have any parents to take care of you young {?CONVERSATION_CHARACTER.GENDER}woman{?}man{\\?}? You are welcome to be a part of my family.", new ConversationSentence.OnConditionDelegate(conversation_adopt_child_on_condition), null, 120, null, null);
            starter.AddDialogLine("character_adoption_response", "adoption_teen", "close_window", "{=NoHJAxWx}Thanks for allowing me to be a part of your family {?PLAYER.GENDER}milady{?}sir{\\?}. I gratefully accept![rf:happy][rb:very_positive]", null, new ConversationSentence.OnConsequenceDelegate(conversation_adopt_child_on_consequence), 100, null);
        }

        private bool conversation_adopt_child_on_condition()
        {
            return true;
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
            TextObject textObject = new("{=DjzDTNHw}{ADOPTER.LINK} adopted {ADOPTED_HERO.LINK}.", null);
            StringHelpers.SetCharacterProperties("ADOPTER", adopter.CharacterObject, textObject);
            StringHelpers.SetCharacterProperties("ADOPTED_HERO", adoptedHero.CharacterObject, textObject);
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString()));
        }

        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            StringHelpers.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject);
            StringHelpers.SetCharacterProperties("CONVERSATION_CHARACTER", CharacterObject.OneToOneConversationCharacter);
            AddDialogs(campaignGameStarter);
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore) { }
    }
}