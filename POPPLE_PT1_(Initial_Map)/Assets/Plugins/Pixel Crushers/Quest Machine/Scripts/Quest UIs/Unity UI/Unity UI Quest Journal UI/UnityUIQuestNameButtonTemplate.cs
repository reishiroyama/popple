﻿// Copyright © Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.QuestMachine
{

    /// <summary>
    /// Unity UI template for a quest name button with a toggle for progress tracking.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class UnityUIQuestNameButtonTemplate : UnityUIContentTemplate
    {

        [SerializeField]
        private UnityUIButtonTemplate m_buttonTemplate;

        [SerializeField]
        private UnityUIToggleTemplate m_trackToggleTemplate;

        public UnityUIButtonTemplate buttonTemplate
        {
            get { return m_buttonTemplate; }
            set { m_buttonTemplate = value; }
        }

        public UnityUIToggleTemplate trackToggleTemplate
        {
            get { return m_trackToggleTemplate; }
            set { m_trackToggleTemplate = value; }
        }

        public virtual void Awake()
        {
            if (buttonTemplate == null && Debug.isDebugBuild) Debug.LogError("Quest Machine: UI Button Template is unassigned.", this);
            if (trackToggleTemplate == null && Debug.isDebugBuild) Debug.LogError("Quest Machine: UI Track Toggle Template is unassigned.", this);
        }

        public virtual void Assign(Quest quest, ToggleChangedDelegate trackToggleDelegate)
        {
            buttonTemplate.Assign(quest.icon, 1, StringField.GetStringValue(quest.title));
            var canTrack = (quest.GetState() == QuestState.Active) && quest.isTrackable;
            trackToggleTemplate.Assign(canTrack, quest.showInTrackHUD, quest, trackToggleDelegate);
        }

    }
}
