using UnityEngine;
using PixelCrushers.DialogueSystem;

public class LogConversation : MonoBehaviour
{
    public void OnConversationStart(Transform actor)
    {
        Debug.Log(string.Format("Starting conversation with {0}", actor.name));
    }
    public void OnConversationLine(Subtitle subtitle)
    {
        Debug.Log(string.Format("{0}: {1}", subtitle.speakerInfo.transform.name, subtitle.formattedText.text));
    }
    public void OnConversationEnd(Transform actor)
    {
        Debug.Log(string.Format("Ending conversation with {0}", actor.name));
    }
}