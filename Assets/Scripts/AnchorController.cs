using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class AnchorController : MonoBehaviour
{
    private string URL = "https://670d5835073307b4ee433e78.mockapi.io/anchor";
    public TextMeshProUGUI[] Anchor_Position;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetData());
    }

    // Update is called once per framep
    void Update()
    {
        
    }

    IEnumerator GetData()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(URL))
        {

            yield return request.SendWebRequest();


            if (request.result == UnityWebRequest.Result.ConnectionError)
                Debug.LogError(request.error);
            else
            {
                string json = request.downloadHandler.text;
                SimpleJSON.JSONNode data = SimpleJSON.JSON.Parse(json);

                for(int i = 0;i < Anchor_Position.Length; i++)
                {
                    Anchor_Position[i].text = "x : " + data[i]["position_x"] + "  " + "y : " + data[i]["position_y"] + "  " + "z : " + data[i]["position_z"];
                }
              
            }
        }
    }
}
