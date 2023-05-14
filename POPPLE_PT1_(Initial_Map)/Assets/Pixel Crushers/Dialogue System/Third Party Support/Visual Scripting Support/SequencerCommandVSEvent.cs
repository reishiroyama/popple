using UnityEngine;
using Unity.VisualScripting;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Sequencer command: VSEvent(eventName, [subject])
    /// </summary>
    public class SequencerCommandVSEvent : SequencerCommand
    {
        private void Awake()
        {
            var eventName = GetParameter(0);
            var subject = GetSubject(1, speaker);
            if (subject == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning($"Dialogue System: Sequencer: VSEvent({GetParameters()}): Can't find subject.");
            }
            else
            {
                if (DialogueDebug.logInfo) Debug.Log($"Dialogue System: Sequencer: VSEvent({eventName}, {subject})");
                CustomEvent.Trigger(subject.gameObject, eventName);
            }
            Stop();
        }
    }
}