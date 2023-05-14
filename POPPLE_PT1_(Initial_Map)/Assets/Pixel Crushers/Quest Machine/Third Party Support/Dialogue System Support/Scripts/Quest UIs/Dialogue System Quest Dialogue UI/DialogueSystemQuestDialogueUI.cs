﻿// Copyright © Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.QuestMachine.DialogueSystemSupport
{

    /// <summary>
    /// Interface for quest dialogue UIs. Uses the Dialogue System for
    /// DialogueSystemConversationQuestContent. Uses the original quest
    /// dialogue UI for other content.
    /// </summary>
    [AddComponentMenu("Pixel Crushers/Quest Machine/Third Party/Dialogue System/UI/Dialogue System Quest Dialogue UI")]
    public class DialogueSystemQuestDialogueUI : MonoBehaviour, IQuestDialogueUI
    {

        #region Serialized fields

        [Tooltip("When starting conversation, hide Quest Dialogue UI originally assigned to Quest Machine.")]
        [SerializeField]
        private bool m_hideOriginalUIDuringConversations = true;

        [Tooltip("When starting dialogue with multiple quests, show quest list as conversation. If unticked, use Quest Dialogue UI originally assigned to Quest Machine.")]
        [SerializeField]
        private bool m_generateConversationForQuestLists = true;

        [Tooltip("Show this text in response button player can use cancel quest list conversation.")]
        [SerializeField]
        private StringField m_cancelQuestListText = new StringField("Goodbye.");

        #endregion

        #region Properties

        /// <summary>
        /// When starting conversation, hide Quest Dialogue UI originally assigned 
        /// to Quest Machine.
        /// </summary>
        public bool hideOriginalUIDuringConversations
        {
            get { return m_hideOriginalUIDuringConversations; }
            set { m_hideOriginalUIDuringConversations = value; }
        }

        /// <summary>
        /// When starting dialogue with multiple quests, show quest list as conversation. 
        /// If false, use Quest Dialogue UI originally assigned to Quest Machine.
        /// </summary>
        public bool generateConversationForQuestLists
        {
            get { return m_generateConversationForQuestLists; }
            set { m_generateConversationForQuestLists = value; }
        }

        /// <summary>
        /// Show this text in response button player can use cancel quest list conversation.
        /// </summary>
        public StringField cancelQuestListText
        {
            get { return m_cancelQuestListText; }
            set { m_cancelQuestListText = value; }
        }

        /// <summary>
        /// True if the dialogue UI is visible (i.e., already in conversation), 
        /// otherwise the state of the original UI.
        /// </summary>
        public virtual bool isVisible
        {
            get
            {
                return DialogueManager.IsConversationActive ? true
                    : (originalUI != null) ? originalUI.isVisible : false;
            }
        }

        /// <summary>
        /// Fallback dialogue UI that the DialogueSystemQuestDialogueUI replaced.
        /// </summary>
        protected IQuestDialogueUI originalUI { get; set; }

        /// <summary>
        /// Current quest for which the dialogue UI is showing content.
        /// </summary>
        protected Quest selectedQuest { get; set; }

        /// <summary>
        /// Delegate to call when player selects a quest from the list.
        /// </summary>
        protected QuestParameterDelegate currentSelectHandler { get; set; }

        /// <summary>
        /// Delegate to call when the player accepts a quest.
        /// </summary>
        protected QuestParameterDelegate currentAcceptHandler { get; set; }

        protected const string GeneratedConversationTitle = "Quest Machine Generated Conversation";
        protected const int GeneratedConversationID = 9000;
        protected const int GeneratedNPCID = 9000;
        protected int PlayerID { get; set; }
        protected Conversation m_generatedConversation = null;
        protected Conversation generatedConversation
        {
            get
            {
                if (m_generatedConversation == null)
                {
                    m_generatedConversation = CreateGeneratedConversation();
                }
                return m_generatedConversation;
            }
        }

        protected Template m_template = null;
        protected Template template
        {
            get { if (m_template == null) m_template = Template.FromDefault();  return m_template; }
        }

        #endregion

        #region Initialization

        protected virtual void Start()
        {
            var qmConfig = GetComponentInChildren<QuestMachineConfiguration>() ?? GetComponentInParent<QuestMachineConfiguration>() ?? FindObjectOfType<QuestMachineConfiguration>();
            if (qmConfig == null) return;
            originalUI = qmConfig.questDialogueUI;
            qmConfig.questDialogueUI = this;
        }

        void OnEnable()
        {
            if (generateConversationForQuestLists)
            {
                Lua.RegisterFunction("OnSelectQuestMachineQuest", this, SymbolExtensions.GetMethodInfo(() => OnSelectQuestMachineQuest(string.Empty)));
            }
            Lua.RegisterFunction("AcceptQuest", this, SymbolExtensions.GetMethodInfo(() => AcceptQuest()));
            Lua.RegisterFunction("CompleteQuest", this, SymbolExtensions.GetMethodInfo(() => CompleteQuest()));
        }

        void OnDisable()
        {
            if (generateConversationForQuestLists)
            {
                Lua.UnregisterFunction("OnSelectQuestMachineQuest");
            }
            Lua.UnregisterFunction("AcceptQuest");
            Lua.UnregisterFunction("CompleteQuest");
        }

        #endregion

        #region Conversation Handling

        protected DialogueSystemConversationQuestContent FindConversationContent(List<QuestContent> contents)
        {
            if (contents == null) return null;
            for (int i = 0; i < contents.Count; i++)
            {
                var content = contents[i];
                if (content is DialogueSystemConversationQuestContent) return content as DialogueSystemConversationQuestContent;
            }
            return null;
        }

        /// <summary>
        /// Starts a Dialogue System conversation between the quest giver and the player.
        /// Sets the conversation's QuestID if selectedQuest is assigned.
        /// </summary>
        /// <param name="speaker">Quest giver.</param>
        /// <param name="conversationContent">Conversation info.</param>
        protected void ShowConversationContent(QuestParticipantTextInfo speaker, DialogueSystemConversationQuestContent conversationContent)
        {
            if (conversationContent == null) return;
            ShowConversation(speaker, conversationContent.conversation);
        }

        protected void ShowConversation(QuestParticipantTextInfo speaker, string title)
        {
            if (string.IsNullOrEmpty(title)) return;
            if (hideOriginalUIDuringConversations && originalUI != null && originalUI.isVisible) originalUI.Hide();
            var actor = GetPlayerActorTransform();
            var conversant = (speaker != null) ? GetActorTransform(StringField.GetStringValue(speaker.id)) : null;
            var conversation = DialogueManager.MasterDatabase.GetConversation(title);
            if (conversation != null && selectedQuest != null && string.IsNullOrEmpty(DialogueSystemQuestMachineBridge.GetConversationField(conversation, "QuestID")))
            {
                // Set conversation's QuestID, used to replace Quest Machine tags:
                Lua.Run("Conversation[" + conversation.id + "].QuestID = \"" + selectedQuest.id + "\"");
            }
            DialogueManager.StartConversation(title, actor, conversant);
        }

        public Transform GetActorTransform(string actorName, bool tryTag = false)
        {
            var actor = DialogueSystem.CharacterInfo.GetRegisteredActorTransform(actorName);
            if (actor != null) return actor;
            var go = GameObject.Find(actorName);
            if (go != null) return go.transform;
            go = tryTag ? GameObject.FindWithTag(actorName) : null;
            if (go == null) go = GameObject.Find(actorName);
            return (go != null) ? go.transform : null;
        }

        public Transform GetPlayerActorTransform()
        {
            var actor = FindMainPlayerActor();
            var playerActorName = (actor != null) ? actor.Name : "Player";
            return GetActorTransform(playerActorName, true);
        }

        protected virtual Conversation CreateGeneratedConversation()
        {
            var database = ScriptableObject.CreateInstance<DialogueDatabase>();

            // Find existing player in database or add new one:
            var playerActor = FindMainPlayerActor();
            if (playerActor != null)
            {
                PlayerID = playerActor.id;
            }
            else
            {
                PlayerID = GeneratedNPCID + 1;
                database.actors.Add(template.CreateActor(PlayerID, "Player", true));
            }

            // Add NPC:
            database.actors.Add(template.CreateActor(GeneratedNPCID, "Generated NPC", false));

            // Create a conversation: [ID 0=START] --> [ID 1=Hello]
            var conversation = template.CreateConversation(GeneratedConversationID, GeneratedConversationTitle);
            conversation.ActorID = PlayerID;
            conversation.ConversantID = GeneratedNPCID;
            database.conversations.Add(conversation);

            // Add to runtime database:
            DialogueManager.AddDatabase(database);
            return conversation;
        }

        protected virtual Actor FindMainPlayerActor()
        {
            var actors = DialogueManager.MasterDatabase.actors;
            for (int i = 0; i < actors.Count; i++)
            {
                if (actors[i].IsPlayer) return actors[i];
            }
            return null;
        }

        protected virtual Actor FindActor(string id)
        {
            var actors = DialogueManager.MasterDatabase.actors;
            for (int i = 0; i < actors.Count; i++)
            {
                if (string.Equals(actors[i].Name, id)) return actors[i];
            }
            return null;
        }

        /// <summary>
        /// Generates and starts a new conversation that lets the player choose a quest.
        /// </summary>
        /// <param name="speaker">Speaker.</param>
        /// <param name="activeQuestsContents">Content introducing the list of active quests.</param>
        /// <param name="activeQuests">Active quests.</param>
        /// <param name="offerableQuestsContents">Content introducing the list of offerable quests.</param>
        /// <param name="offerableQuests">Offerable quests.</param>
        protected virtual void ShowGeneratedQuestListConversation(QuestParticipantTextInfo speaker, List<QuestContent> activeQuestsContents, List<Quest> activeQuests, List<QuestContent> offerableQuestsContents, List<Quest> offerableQuests)
        {
            var conversation = generatedConversation;
            conversation.dialogueEntries.Clear();

            // Determine NPC:
            var npcActor = FindActor(StringField.GetStringValue(speaker.id));
            var npcID = (npcActor != null) ? npcActor.id : GeneratedNPCID;
            conversation.ConversantID = npcID;

            // START node:
            var startNode = template.CreateDialogueEntry(0, conversation.id, "START");
            startNode.ActorID = PlayerID;
            startNode.ConversantID = npcID;
            startNode.Sequence = "None()";
            conversation.dialogueEntries.Add(startNode);

            // NPC line:
            var dialogueText = (activeQuests.Count > 0 && activeQuestsContents.Count > 0) ? GetDialogueText(activeQuestsContents)
                : (offerableQuests.Count > 0 && offerableQuestsContents.Count > 0) ? GetDialogueText(offerableQuestsContents)
                : StringField.GetStringValue(speaker.displayName);
            var npcNode = template.CreateDialogueEntry(1, conversation.id, string.Empty);
            npcNode.ActorID = npcID;
            npcNode.ConversantID = PlayerID;
            npcNode.DialogueText = dialogueText;
            conversation.dialogueEntries.Add(npcNode);
            var link = new Link(conversation.id, startNode.id, conversation.id, npcNode.id);
            startNode.outgoingLinks.Add(link);

            // Responses (quests):
            AddResponseNodes(conversation, npcNode, activeQuests);
            AddResponseNodes(conversation, npcNode, offerableQuests);
            AddResponseNode(conversation, npcNode, null);

            // Clear SimStatus:
            Lua.Run("Conversation[" + conversation.id + "].Dialog = {}");

            // Show conversation:
            ShowConversation(speaker, GeneratedConversationTitle);
        }

        protected virtual string GetDialogueText(List<QuestContent> contents)
        {
            if (contents == null) return "Hello.";
            var first = true;
            var sb = new StringBuilder();
            for (int i = 0; i < contents.Count; i++)
            {
                var content = contents[i];
                if (content == null) continue;
                if (!first) sb.Append("\n");
                first = false;
                var text = content.runtimeText;
                if (content is HeadingTextQuestContent) text = "<b>" + text + "</b>";
                sb.Append(text);
            }
            return sb.ToString();
        }

        protected virtual void AddResponseNodes(Conversation conversation, DialogueEntry npcNode, List<Quest> quests)
        {
            if (quests == null) return;
            for (int i = 0; i < quests.Count; i++)
            {
                AddResponseNode(conversation, npcNode, quests[i]);
            }
        }

        protected virtual void AddResponseNode(Conversation conversation, DialogueEntry npcNode, Quest quest)
        {
            if (conversation == null || npcNode == null) return;

            var node = template.CreateDialogueEntry(conversation.dialogueEntries.Count, conversation.id, string.Empty);
            node.ActorID = PlayerID;
            node.ConversantID = npcNode.ActorID;
            if (quest != null)
            {
                node.DialogueText = StringField.GetStringValue(quest.title);
                node.userScript = "OnSelectQuestMachineQuest(\"" + StringField.GetStringValue(quest.id).Replace("\"", "\\\"") + "\")";
            }
            else
            {
                node.DialogueText = StringField.GetStringValue(cancelQuestListText);
            }
            conversation.dialogueEntries.Add(node);
            var link = new Link(conversation.id, npcNode.id, conversation.id, node.id);
            npcNode.outgoingLinks.Add(link);
        }

        /// <summary>
        /// Player just selected a quest from a list presented in a conversation.
        /// </summary>
        /// <param name="questID">ID of selected quest.</param>
        protected virtual void OnSelectQuestMachineQuest(string questID)
        {
            StartCoroutine(StopConversationAndSelectQuest(questID));
        }

        protected IEnumerator StopConversationAndSelectQuest(string questID)
        { 
            DialogueManager.StopConversation();
            yield return WaitUntilDialogueUIHidden();
            if (currentSelectHandler != null) currentSelectHandler.Invoke(QuestMachine.GetQuestInstance(questID));
        }

        protected IEnumerator WaitUntilDialogueUIHidden()
        {
            int safeguard = 0;
            var unityUIDialogueUI = DialogueManager.DialogueUI as UnityUIDialogueUI;
            if (unityUIDialogueUI != null)
            {
                while (unityUIDialogueUI.dialogue.panel != null && unityUIDialogueUI.dialogue.panel.gameObject.activeInHierarchy && safeguard < 999)
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Player just accepted the selected quest in a conversation.
        /// </summary>
        protected virtual void AcceptQuest()
        {
            currentAcceptHandler(selectedQuest);
        }

        /// <summary>
        /// Lua function to complete the selected quest. This function doesn't normally
        /// need to be called. Generated quests will listen for the DiscussQuest:Giver
        /// message outside of the conversation.
        /// </summary>
        protected virtual void CompleteQuest()
        {
            if (selectedQuest == null) return;

            // Set the bottom active node (typically 'Return To NPC') to true:
            for (int i = selectedQuest.nodeList.Count - 1; i >= 0; i--)
            {
                var node = selectedQuest.nodeList[i];
                if (node.GetState() == QuestNodeState.Active)
                {
                    node.SetState(QuestNodeState.True);
                    break;
                }
            }

            // Make sure quest itself is successful.
            if (selectedQuest.GetState() != QuestState.Successful) selectedQuest.SetState(QuestState.Successful);
        }

        #endregion

        #region IQuestDialogueUI methods

        /// <summary>
        /// Shows UI content.
        /// </summary>
        /// <param name="speaker">Speaker.</param>
        /// <param name="contents">Content being spoken by speaker.</param>
        public virtual void ShowContents(QuestParticipantTextInfo speaker, List<QuestContent> contents)
        {
            var conversationContent = FindConversationContent(contents);
            if (conversationContent != null)
            {
                ShowConversationContent(speaker, conversationContent);
            }
            else
            {
                if (originalUI == null)
                {
                    ReportNoOriginalUI();
                }
                else
                {
                    originalUI.ShowContents(speaker, contents);
                }
            }
        }

        protected void ReportNoOriginalUI()
        {
            Debug.LogWarning("Quest Machine: " + GetType().Name + " wants to show non-conversation content, but a non-conversation UI was not assigned to the Quest Machine Configuration's Quest Dialogue UI field.", this);
        }

        /// <summary>
        /// Shows content explaining that all quests' offer conditions are unmet.
        /// </summary>
        /// <param name="speaker">Speaker.</param>
        /// <param name="contents">Content explaining that all quests' offer conditions are unmet.</param>
        /// <param name="quests">List of quests.</param>
        public virtual void ShowOfferConditionsUnmet(QuestParticipantTextInfo speaker, List<QuestContent> contents, List<Quest> quests)
        {
            ShowContents(speaker, contents);
        }

        /// <summary>
        /// Shows a list of quests.
        /// </summary>
        /// <param name="speaker">Speaker.</param>
        /// <param name="activeQuestsContents">Content introducing the list of active quests.</param>
        /// <param name="activeQuests">Active quests.</param>
        /// <param name="offerableQuestsContents">Content introducing the list of offerable quests.</param>
        /// <param name="offerableQuests">Offerable quests.</param>
        /// <param name="selectHandler">Method to invoke when the player selects a quest.</param>
        public virtual void ShowQuestList(QuestParticipantTextInfo speaker, List<QuestContent> activeQuestsContents, List<Quest> activeQuests,
            List<QuestContent> offerableQuestsContents, List<Quest> offerableQuests, QuestParameterDelegate selectHandler)
        {
            if (generateConversationForQuestLists)
            {
                selectedQuest = null;
                currentSelectHandler = selectHandler;
                ShowGeneratedQuestListConversation(speaker, activeQuestsContents, activeQuests, offerableQuestsContents, offerableQuests);
            }
            else if (originalUI != null)
            {
                originalUI.ShowQuestList(speaker, activeQuestsContents, activeQuests, offerableQuestsContents, offerableQuests, selectHandler);
            }
            else
            {
                ReportNoOriginalUI();
            }
        }

        /// <summary>
        /// Shows a quest offer.
        /// </summary>
        /// <param name="speaker">Speaker.</param>
        /// <param name="quest">Quest to offer.</param>
        /// <param name="acceptHandler">Method to invoke if the player accepts the quest.</param>
        /// <param name="declineHandler">Method to invoke if the player declines the quest.</param>
        public virtual void ShowOfferQuest(QuestParticipantTextInfo speaker, Quest quest, QuestParameterDelegate acceptHandler, QuestParameterDelegate declineHandler)
        {
            SetSelectedQuest(speaker, quest);
            currentAcceptHandler = acceptHandler;
            ShowContents(speaker, quest.offerContentList);
        }

        /// <summary>
        /// Shows an active quest.
        /// </summary>
        /// <param name="speaker">Speaker.</param>
        /// <param name="quest">Active quest.</param>
        /// <param name="continueHandler">Method to invoke if the player clicks the continue button.</param>
        /// <param name="backHandler">Method to invoke if the player clicks the back button.</param>
        public virtual void ShowActiveQuest(QuestParticipantTextInfo speaker, Quest quest, QuestParameterDelegate continueHandler, QuestParameterDelegate backHandler)
        {
            SetSelectedQuest(speaker, quest);
            ShowContents(speaker, quest.GetContentList(QuestContentCategory.Dialogue, speaker));
        }

        /// <summary>
        /// Shows completed quests.
        /// </summary>
        /// <param name="speaker">Speaker</param>
        /// <param name="quests">Completed quests.</param>
        public virtual void ShowCompletedQuest(QuestParticipantTextInfo speaker, List<Quest> quests)
        {
            if (quests == null || quests.Count == 0) return;
            var quest = quests[0];
            SetSelectedQuest(speaker, quest);
            ShowContents(speaker, quest.GetContentList(QuestContentCategory.Dialogue));
        }

        protected virtual void SetSelectedQuest(QuestParticipantTextInfo speaker, Quest quest)
        {
            if (quest != null && StringField.IsNullOrEmpty(quest.questGiverID)) quest.AssignQuestGiver(speaker);
            selectedQuest = quest;
            DialogueLua.SetVariable("QUESTID", StringField.GetStringValue(quest.id));
        }

        /// <summary>
        /// Hides the dialogue UI.
        /// </summary>
        public virtual void Hide()
        {
            DialogueManager.StopConversation();
        }

        #endregion

    }

}
