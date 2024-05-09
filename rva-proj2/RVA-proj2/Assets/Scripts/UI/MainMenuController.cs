using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{

    public GameObject credits;
    public GameObject instructions;

    private void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
    }

    public void Play()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void HowToPlay()
    {
        instructions.SetActive(true);
        this.GameObject().SetActive(false);
    }

    public void Credits()
    {
        credits.SetActive(true);
        this.GameObject().SetActive(false);
    }
    
}
