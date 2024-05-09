using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CaptureInsectController : MonoBehaviour
{
    public float volumeThreshold = 1.0f;
    public int waveLength = 256;
    public float offsetAngle = 15;

    public Transform parent;

    public GameObject PyroscarabCaptureUI;
    
    public GameObject progressBarObj;
    public Slider progressBar;
    public Image checkmark;
    public Image cross;
    public TextMeshProUGUI BehindTextMeshProUGUI;


    private AudioClip waveAudio;
    private bool isCaptured;
    private float soundPercentage;
    private float limit1;
    private float limit2;
    private Animator animator;
    
    public delegate void Captured(GameObject monsterUI,Target target);
    public static event Captured OnCaptured;
    

    // Start is called before the first frame update
    void Start()
    {
        StartMicrophone();
        limit1 = 90 - offsetAngle;
        limit2 = 270 + offsetAngle;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCaptured && PyroscarabCaptureUI.activeSelf)
        {
            Capture();
            UpdateUI();
        }
    }

    void Capture()
    {
        float y = parent.localRotation.eulerAngles.y;

        if (y < limit1 || y > limit2)
        {

            float volume = GetVolume(Microphone.GetPosition(Microphone.devices[0]), waveAudio);

            soundPercentage = Mathf.Clamp01(volume / volumeThreshold);

            if (volume > volumeThreshold)
            {
                isCaptured = true;

                animator.SetTrigger("Died");
                OnCaptured(PyroscarabCaptureUI, Target.Pyroscarab);
            }
        }

    }

    void StartMicrophone()
    {
        string micName = Microphone.devices[0];
        waveAudio = Microphone.Start(micName, true, 20, AudioSettings.outputSampleRate);
    }

    float GetVolume(int clipPosition, AudioClip clip)
    {
        int startingPosition = clipPosition - waveLength;

        if (startingPosition < 0)
        {
            return 0.0f;
        }

        float[] waveData = new float[waveLength];
        clip.GetData(waveData, startingPosition);

        float volumeTotal = 0;

        foreach (var wave in waveData)
        {
            volumeTotal += Mathf.Abs(wave);
        }

        return volumeTotal;
    }

    void UpdateUI()
    {
        if(PyroscarabCaptureUI.activeSelf)
        {
            float y = parent.localRotation.eulerAngles.y;

            if (y < limit1 || y > limit2)
            {
                checkmark.enabled = true;
                BehindTextMeshProUGUI.text = "You are behind the Pyroscarab\nMake sound to capture it!";
                cross.enabled = false;
                progressBarObj.SetActive(true);
                progressBar.value = soundPercentage;
            }
            else
            {
                checkmark.enabled = false;
                BehindTextMeshProUGUI.text = "You cannot capture the Pyroscarab\nTry to ambush it from behind";
                progressBarObj.SetActive(false);
                cross.enabled = true;
                progressBar.value = 0.0f;
            }
        }
    }
}
