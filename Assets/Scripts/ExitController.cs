using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitController : MonoBehaviour
{

    public void Exit()
    {
        Debug.Log("Exit");
        Application.Quit();
    }
}
