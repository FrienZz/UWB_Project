using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SubmitControl : MonoBehaviour
{

   public TMP_Dropdown dropdown;

    public void Submit()
   {
        if (dropdown.value == 1)
        {
            SceneManager.LoadScene("Room521");
        }
        if (dropdown.value == 2)
        {
            SceneManager.LoadScene("Room516");
        }
    }
}
