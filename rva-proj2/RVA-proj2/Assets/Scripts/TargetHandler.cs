using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;

public class TargetHandler : MonoBehaviour
{

    public List<Target> targetsOnScreen;
    
    public GameObject AquarhinoMarker;
    public GameObject PyroscarabMarker;
    public GameObject PricklashMarker;

    public GameObject detectMessage;
    public GameObject capturedMessage;
    public GameObject battleStartUI;

    private bool checkForBattle;
    public delegate void MultipleTargetsDetected();

    public static event MultipleTargetsDetected OnMultipleTargetsDetected;

    public delegate void OneTargetDetected(HashSet<Target> capturedTargets);

    public static event OneTargetDetected OnOneTargetDetected;
    
    private void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        checkForBattle = true;
        targetsOnScreen = new List<Target>();
        TargetController.onTargetEnable += EnableTarget;
        TargetController.onTargetDisabled += DisableTarget;
        CaptureRhinoController.OnCaptured += OnCaptured;
        CaptureCactusController.OnCaptured += OnCaptured;
        CaptureInsectController.OnCaptured += OnCaptured;
        BattlePromptController.OnBattlePromptCooldown += OnBattlePromptCooldown;

        if (CaptureInfo.battleEnded)
        {
            OnBattlePromptCooldown();
            if (CaptureInfo.PlayerWon)
            {
                capturedMessage.GetComponent<TextMeshProUGUI>().text ="Your " + CaptureInfo.PlayerTarget + " won the battle!";
            }
            else
            {
                capturedMessage.GetComponent<TextMeshProUGUI>().text = CaptureInfo.EnemyTarget +" won the battle. Better luck next time.";

            }
            
            capturedMessage.SetActive(true);
            StartCoroutine(DeactivateCapturedImage());
        }
        
    }
    
        
    private void OnDestroy()
    {
        TargetController.onTargetEnable -= EnableTarget;
        TargetController.onTargetDisabled -= DisableTarget;
        CaptureRhinoController.OnCaptured -= OnCaptured;
        CaptureCactusController.OnCaptured -= OnCaptured;
        CaptureInsectController.OnCaptured -= OnCaptured;
        BattlePromptController.OnBattlePromptCooldown -= OnBattlePromptCooldown;
    }
    
    private void OnBattlePromptCooldown()
    {
        battleStartUI.SetActive(false);
        checkForBattle = false;
        StartCoroutine(ActivateBattleCheck());

    }

    private void EnableTarget(GameObject monsterUI, GameObject monster, Target target)
    {
        targetsOnScreen.Add(target);
        monster.SetActive(true);

        if (targetsOnScreen.Count == 1)
        {
            detectMessage.SetActive(false);

            if (!CaptureInfo.capturedTargets.Contains(target))
            {
                monsterUI.SetActive(true);
            }
        }

        if (targetsOnScreen.Count == 2)
        {
            OnMultipleTargetsDetected();
        }
    }

    private void DisableTarget(GameObject monsterUI, GameObject monster, Target target)
    {
        targetsOnScreen.Remove(target);
        monster.SetActive(false);

        if (monsterUI.activeSelf)
        {
            monsterUI.SetActive(false);
        }

        if (targetsOnScreen.Count == 1)
        {
            OnOneTargetDetected(CaptureInfo.capturedTargets);
        }

        if (targetsOnScreen.Count == 0)
        {
            detectMessage.SetActive(true);
        }
        
    }

    private void OnCaptured(GameObject monsterUI,Target target)
    {
        capturedMessage.GetComponent<TextMeshProUGUI>().text = "Congratulation!\n\nYou Captured " + (target == Target.Aquarhin ? "an " : "a ") + target +".";
        capturedMessage.SetActive(true);
        CaptureInfo.capturedTargets.Add(target);
        monsterUI.SetActive(false);
        StartCoroutine(DeactivateCapturedImage());
    }

    IEnumerator DeactivateCapturedImage()
    {
        yield return new WaitForSeconds(3);
        capturedMessage.SetActive(false);
    }
    
    IEnumerator ActivateBattleCheck()
    {
        yield return new WaitForSeconds(3);
        checkForBattle = true;
    }

    private void Update()
    {
        if (checkForBattle)
        {
            CheckIfBattle();

        }
        UpdateMessage();
    }


    private void UpdateMessage()
    {
        if (targetsOnScreen.Count == 0)
        {
            detectMessage.SetActive(true);
        }
        else
        {
            if (detectMessage.activeSelf)
            {
                detectMessage.SetActive(false);
            }
        }
    }


    private GameObject TargetEnumToTargetObject(Target target)
    {
        if (target == Target.Pyroscarab)
        {
            return PyroscarabMarker;
        }
        if (target == Target.Aquarhin)
        {
            return AquarhinoMarker;
        } 
        if (target == Target.Pricklash)
        {
            return PricklashMarker;
        }

        return null;
    }
    
    private void CheckIfBattle()
    {
        // Check if there are two markers on the screen
        if (targetsOnScreen.Count == 2)
        {
            List<Target> playerCards = new List<Target>();
            Target enemyCard = Target.Aquarhin;
            foreach (var target in targetsOnScreen)
            {
                if (CaptureInfo.capturedTargets.Contains(target))
                {
                    playerCards.Add(target);
                }
                else
                {
                    enemyCard = target;
                }

            }

            // If there are no player markers then return
            if (playerCards.Count == 0) return;
            
            // Getting positions and rotations of markers
            var target1 = targetsOnScreen[0];
            var target1Obj = TargetEnumToTargetObject(target1);
            
            var target2 = targetsOnScreen[1];
            var target2Obj = TargetEnumToTargetObject(target2);

            if (!DistanceUtils.IsBattlePosition(target1Obj, target2Obj))
            {
                return;
            }
            
            Target playerCard = playerCards[0];

            if (playerCards.Count == 2)
            {
                if (DistanceUtils.IsClosestToPlayerPlayer1(target1Obj, target2Obj))
                {
                    playerCard = target1;
                    enemyCard = target2;
                }
                else
                {
                    playerCard = target2;
                    enemyCard = target1;
                }
            }
            
            CaptureInfo.PlayerTarget = playerCard;
            CaptureInfo.EnemyTarget = enemyCard;
            battleStartUI.SetActive(true);

        } 
    }
}
