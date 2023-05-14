// Copyright © Pixel Crushers. All rights reserved.

using Unity.VisualScripting;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.VisualScriptingSupport
{

    /// <summary>
    /// Handles Dialogue System messages as Visual Scripting custom events.
    /// </summary>
    [AddComponentMenu("Pixel Crushers/Dialogue System/Third Party/Visual Scripting/Dialogue System Visual Scripting Custom Events")]
    public class DialogueSystemVisualScriptingCustomEvents : MonoBehaviour
    {

        #region Conversation Events

        public void OnConversationStart(Transform actor)
        {
            CustomEvent.Trigger(gameObject, "OnConversationStart", actor);
        }

        public void OnConversationEnd(Transform actor)
        {
            CustomEvent.Trigger(gameObject, "OnConversationEnd", actor);
        }

        public void OnConversationCancelled(Transform actor)
        {
            CustomEvent.Trigger(gameObject, "OnConversationCancelled", actor);
        }

        public void OnConversationLine(Subtitle subtitle)
        {
            CustomEvent.Trigger(gameObject, "OnConversationLine", subtitle);
        }

        public void OnConversationLineEnd(Subtitle subtitle)
        {
            CustomEvent.Trigger(gameObject, "OnConversationLineEnd", subtitle);
        }

        public void OnConversationLineCancelled(Subtitle subtitle)
        {
            CustomEvent.Trigger(gameObject, "OnConversationLineCancelled", subtitle);
        }

        public void OnConversationResponseMenu(Response[] responses)
        {
            CustomEvent.Trigger(gameObject, "OnConversationResponseMenu", responses);
        }

        public void OnConversationTimeout()
        {
            CustomEvent.Trigger(gameObject, "OnConversationTimeout");
        }

        public void OnLinkedConversationStart(Transform actor)
        {
            CustomEvent.Trigger(gameObject, "OnLinkedConversationStart", actor);
        }

        #endregion

        #region Bark Events

        public void OnBarkStart(Transform actor)
        {
            CustomEvent.Trigger(gameObject, "OnBarkStart", actor);
        }

        public void OnBarkEnd(Transform actor)
        {
            CustomEvent.Trigger(gameObject, "OnBarkEnd", actor);
        }

        public void OnBarkLine(Subtitle subtitle)
        {
            CustomEvent.Trigger(gameObject, "OnBarkLine", subtitle);
        }

        #endregion

        #region Sequence Events

        public void OnSequenceStart(Transform actor)
        {
            CustomEvent.Trigger(gameObject, "OnSequenceStart", actor);
        }

        public void OnSequenceEnd(Transform actor)
        {
            CustomEvent.Trigger(gameObject, "OnSequenceEnd", actor);
        }

        #endregion

        #region Quest Events

        public void OnQuestStateChange(string title)
        {
            CustomEvent.Trigger(gameObject, "OnQuestStateChange", title);
        }

        public void OnQuestTrackingEnabled(string title)
        {
            CustomEvent.Trigger(gameObject, "OnQuestTrackingEnabled", title);
        }

        public void OnQuestTrackingDisabled(string title)
        {
            CustomEvent.Trigger(gameObject, "OnQuestTrackingDisabled", title);
        }

        public void UpdateTracker()
        {
            CustomEvent.Trigger(gameObject, "UpdateTracker");
        }

        #endregion

        #region Pause Events

        public void OnDialogueSystemPause()
        {
            CustomEvent.Trigger(gameObject, "OnDialogueSystemPause");
        }

        public void OnDialogueSystemUnpause()
        {
            CustomEvent.Trigger(gameObject, "OnDialogueSystemUnpause");
        }

        #endregion
    }
}