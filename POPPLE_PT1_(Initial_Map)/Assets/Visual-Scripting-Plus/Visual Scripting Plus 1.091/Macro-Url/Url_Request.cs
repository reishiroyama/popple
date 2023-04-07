using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Unity.VisualScripting;

public class Url_Request : MonoBehaviour
{
    public UnityEvent Function_Url_Request_Return;
    public UnityEvent Function_Input_Return;

    public int var_input = 0;
    public string var_request = "";
    public string var_result = "";
    void Start()
    {
        var_input = 0;
        var_request = "";
        var_result = "";
    }

    // Update is called once per frame
    void Update()
    {
        GameObject MyObject = GameObject.Find("Macro-Url");

        var_input = Int32.Parse(Variables.Object(MyObject).Get("Url-Input").ToString());

        var_request = Variables.Object(MyObject).Get("Url-Request").ToString();

        if (var_input == 1)
        {
            var_input = 0;
            //-------url_request----------
            string url = var_request;


            StartCoroutine(GetRequest(url));

            IEnumerator GetRequest(string uri)
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
                {
                    // Request and wait for the desired page.
                    yield return webRequest.SendWebRequest();

                    string[] pages = uri.Split('/');
                    int page = pages.Length - 1;

                    switch (webRequest.result)
                    {
                        case UnityWebRequest.Result.ConnectionError:
                        case UnityWebRequest.Result.DataProcessingError:
                            Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                            break;
                        case UnityWebRequest.Result.ProtocolError:
                            Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                            break;
                        case UnityWebRequest.Result.Success:
                            var_result = webRequest.downloadHandler.text;
                            Function_Url_Request_Return.Invoke();
                            break;
                    }
                }
            }
            //-------End url_request----------



            Function_Input_Return.Invoke();
        }
    }
}
