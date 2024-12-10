using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartController : MonoBehaviour
{

    public Color wantedColor;
    public Button button;
    public TextMeshProUGUI buttonText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeButtonTextAndColor()
    {
        if(buttonText.text == "Start")
        {
            buttonText.text = "Stop";
            button.image.color = wantedColor;   
            TagController.active = true;
            Debug.Log("Enable");
        }
        else
        {
            buttonText.text = "Start";
            button.image.color = new Color(0, 128, 0);
            TagController.active = false;
            Debug.Log("Disable");

        }
      
    }

}
