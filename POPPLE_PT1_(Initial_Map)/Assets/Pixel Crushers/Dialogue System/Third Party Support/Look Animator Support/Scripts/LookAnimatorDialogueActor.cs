using FIMSpace.FLook;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.LookAnimatorSupport
{

    [RequireComponent(typeof(FLookAnimator))]
    public class LookAnimatorDialogueActor : MonoBehaviour
    {
        [Tooltip("Where listeners should look when this character is speaking.")]
        public Transform myLookTarget;

        [Tooltip("Only look for LookAnimatorDialogueActors on this layer.")]
        public LayerMask lookAnimatorLayerMask = 1;

        private FLookAnimator myLookAnimator;
        public Transform previousLookTarget;
        private List<LookAnimatorDialogueActor> listeners = new List<LookAnimatorDialogueActor>();

        public List<LookAnimatorDialogueActor> GetListeners() { return listeners; }

        private void Awake()
        {
            myLookAnimator = GetComponent<FLookAnimator>();
            if (myLookTarget == null)
            {
                myLookTarget = (myLookAnimator.LeadBone != null) ? myLookAnimator.LeadBone: this.transform;
            }
            previousLookTarget = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & lookAnimatorLayerMask) == 0) return;
            var otherLookAnimator = other.GetComponentInChildren<LookAnimatorDialogueActor>();
            if (otherLookAnimator == null || listeners.Contains(otherLookAnimator)) return;
            listeners.Add(otherLookAnimator);
        }

        private void OnTriggerExit(Collider other)
        {
            if (((1 << other.gameObject.layer) & lookAnimatorLayerMask) == 0) return;
            var otherLookAnimator = other.GetComponentInChildren<LookAnimatorDialogueActor>();
            if (otherLookAnimator == null) return;
            listeners.Remove(otherLookAnimator);
            otherLookAnimator.LookAt(null);
        }

        public void LookAt(Transform speaker)
        {
            if (speaker == null)
            {
                if (previousLookTarget != null) myLookAnimator.SetLookTarget(previousLookTarget);
                previousLookTarget = myLookAnimator.ObjectToFollow;
            }
            else
            {
                if (previousLookTarget == null) previousLookTarget = myLookAnimator.ObjectToFollow;
                var otherLookAnimator = speaker.GetComponent<LookAnimatorDialogueActor>();
                var newObjectToFollow = (otherLookAnimator != null) ? otherLookAnimator.myLookTarget : speaker;
                myLookAnimator.SetLookTarget(newObjectToFollow);
            }
        }

        private void OnConversationLine(Subtitle subtitle)
        {
            // Am I speaking? If not, exit.
            if (subtitle.speakerInfo.transform != this.transform) return;

            // Set myself to look at the listener:
            if (subtitle.listenerInfo.transform != null)
            {
                var listener = subtitle.listenerInfo.transform.GetComponentInChildren<LookAnimatorDialogueActor>();
                if (listener != null)
                {
                    LookAt(listener.myLookTarget);
                }
            }

            // Set other listeners to look at me:
            foreach (var listener in listeners)
            {
                listener.LookAt(myLookTarget);
            }
        }

        private void OnConversationEnd(Transform actor)
        {
            foreach (var listener in listeners)
            {
                listener.LookAt(null);
            }
        }

    }
}