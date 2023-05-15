using UnityEngine;
using PixelCrushers.DialogueSystem.LookAnimatorSupport;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    public class SequencerCommandLookAnimator : SequencerCommand
    {

        public void Awake()
        {
            var noTarget = string.Equals("none", GetParameter(0), System.StringComparison.OrdinalIgnoreCase);
            var target = noTarget ? null : GetSubject(0);
            var all = string.Equals("all", GetParameter(1), System.StringComparison.OrdinalIgnoreCase);
            var subject = GetSubject(1, speaker);
            var subjectActor = (subject != null) ? subject.GetComponentInChildren<LookAnimatorDialogueActor>() : null;
            var targetActor = (target != null) ? target.GetComponentInChildren<LookAnimatorDialogueActor>() : null;
            if (subjectActor == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Sequencer: LookAnimator(" + GetParameters() + ") the subject " + GetParameter(1) + " doesn't have a LookAnimatorDialogueActor");
            }
            else if (!noTarget && target == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Sequencer: LookAnimator(" + GetParameters() + ") can't find the look target");
            }
            else if (!all && subjectActor == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Sequencer: LookAnimator(" + GetParameters() + ") can't find the LookAnimator or it doesn't have a LookAnimatorDialogueActor component");
            }
            else
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Sequencer: LookAnimator(" + target + ", " + (all ? "all" : subject.name) + ")", target);
                var lookTarget = (targetActor != null) ? targetActor.myLookTarget : target;
                subjectActor.LookAt(lookTarget);
                if (all)
                {
                    foreach (var listener in subjectActor.GetListeners())
                    {
                        if (listener == targetActor) continue;
                        listener.LookAt(lookTarget);
                    }
                }
            }
            Stop();
        }
    }
}
