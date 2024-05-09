using System.ComponentModel;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;

public class CaptureCactusController : MonoBehaviour
{
    public float shakeThreshold = 1.5f;
    public float fillSpeed = 0.05f;
    public float timeThreshold = 1.0f;
    public float fillPenalty = 0.1f;
    public Slider progressBar;

    private float totalFill;
    private float timeLapse;
    private bool isCaptured;
    private Animator animator;

    public GameObject PricklashCaptureUI;
    public delegate void Captured(GameObject monsterUI,Target target);
    public static event Captured OnCaptured;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isCaptured)
        {
            CaptureLogic();
            UpdateUI();
        }

    }

    private void CaptureLogic()
    {
        Vector3 shake = Input.acceleration;
        float shakeMag = shake.magnitude;

        if (shakeMag > shakeThreshold)
        {
            totalFill += fillSpeed * Time.deltaTime;
            totalFill = Mathf.Clamp01(totalFill);
            timeLapse = 0.0f;
        }
        else
        {
            timeLapse += Time.deltaTime;
        }

        if (timeLapse > timeThreshold)
        {
            totalFill -= fillPenalty * Time.deltaTime;
            totalFill = Mathf.Clamp01(totalFill);
        }

        if (totalFill == 1.0f)
        {
            //Debug.Log("Captured");
            isCaptured = true;
            animator.SetTrigger("Died");
            OnCaptured(PricklashCaptureUI,Target.Pricklash);
        }

        //Debug.Log("Magnitude = " + shakeMag + " Total Fill = " + totalFill);
    }
    


    private void UpdateUI()
    {
        progressBar.value = totalFill;
    }
}
