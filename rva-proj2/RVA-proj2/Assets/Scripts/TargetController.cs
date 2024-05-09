using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetController : MonoBehaviour
{
    public GameObject monsterUI;
    public GameObject monster;
    public Target target;
    public delegate void TargetEnabled(GameObject monsterUI, GameObject monster, Target target);

    public static event TargetEnabled onTargetEnable;
    
    public delegate void TargetDisabled(GameObject monsterUI, GameObject monster, Target target);

    public static event TargetDisabled onTargetDisabled;

    private void Start()
    {
        TargetHandler.OnMultipleTargetsDetected += OnMultipleTargetsDetected;
        TargetHandler.OnOneTargetDetected += OnOneTargetDetected;
    }

    private void OnEnable()
    {
        TargetHandler.OnMultipleTargetsDetected += OnMultipleTargetsDetected;
        TargetHandler.OnOneTargetDetected += OnOneTargetDetected;

    }

    private void OnDisable()
    {
        TargetHandler.OnMultipleTargetsDetected -= OnMultipleTargetsDetected;
        TargetHandler.OnOneTargetDetected -= OnOneTargetDetected;
    }

    private void OnDestroy()
    {
        TargetHandler.OnMultipleTargetsDetected -= OnMultipleTargetsDetected;
        TargetHandler.OnOneTargetDetected -= OnOneTargetDetected;
    }


    public void EnableGeneric()
    {
        onTargetEnable(monsterUI, monster, target);
    }

    public void DisableGeneric()
    {
        onTargetDisabled(monsterUI, monster, target);

    }

    private void OnMultipleTargetsDetected()
    {
        if (monster.activeSelf)
        {
            if (monsterUI.activeSelf)
            {
                monsterUI.SetActive(false);
            }
        }
    }
    
    private void OnOneTargetDetected(HashSet<Target> capturedTargets)
    {
        if (monster.activeSelf)
        {
            if (!capturedTargets.Contains(target))
            {
                monsterUI.SetActive(true);
            }
        }
    }

}
