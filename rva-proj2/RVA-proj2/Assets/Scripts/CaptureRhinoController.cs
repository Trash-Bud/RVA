using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class CaptureRhinoController : MonoBehaviour
{
    public GameObject arrowPrefab;
    public float arrowSpeed = 10.0f;
    public Slider progressBar;
    public float arrowDamage = 0.2f;

    private Camera cam;
    private bool isCaptured;
    private float hp = 1;
    private Animator animator;
    
    public delegate void Captured(GameObject monsterUI,Target target);
    public static event Captured OnCaptured;
    
    public GameObject AquarhinoCaptureUI;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCaptured)
        {
            updateUI();
        }
    }

    public void CreateArrow()
    {
        if (!isCaptured)
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

            GameObject arrow = Instantiate(arrowPrefab, ray.origin, Quaternion.identity);

            Quaternion rotation = Quaternion.LookRotation(ray.direction.normalized, Vector3.up);

            Quaternion tipRightRotation = Quaternion.AngleAxis(-90f, rotation * Vector3.forward);

            arrow.transform.rotation = rotation * tipRightRotation;

            //Debug.Log("Direction = " + ray.direction.normalized);
            Rigidbody arrowRb = arrow.GetComponent<Rigidbody>();
            arrowRb.AddForce(ray.direction.normalized * arrowSpeed, ForceMode.Impulse);
        }
    }

    private void updateUI()
    {
        progressBar.value = hp;
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            hp -= arrowDamage;
            Destroy(other.gameObject);
            animator.SetTrigger("IsHit");
        }

        if (hp <= 0)
        {
            isCaptured = true;
            animator.SetBool("IsDead", true);
            OnCaptured(AquarhinoCaptureUI, Target.Aquarhin);
        }
        
    }
}
