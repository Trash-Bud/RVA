using System;
using DefaultNamespace;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FightController : MonoBehaviour
{
    public Slider playerHealth;
    public Image playerHealthFill;

    public Slider enemyHealth;
    public Image enemyHealthFill;

    public TextMeshProUGUI turnText;
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI enemyName;

    public Image playerBurned;
    public Image playerPoisoned;
    public Image playerConfused;
    public Image enemyBurned;
    public Image enemyPoisoned;
    public Image enemyConfused;
    public GameObject runAwayScreen;
    public TextMeshProUGUI messageObj;
    public TextMeshProUGUI time;
    public Button undo;
    public TextMeshProUGUI fightInfo;

    public GameObject markerErrorScreen;
    public TextMeshProUGUI messageMarkerError;
    public TextMeshProUGUI titleMarkerError;

    private MonsterGeneric monster1;
    private MonsterGeneric monster2;
    private int turn = 1;
    private int winner;
    private bool isOver = false;
    private bool midTurn = false;
    private bool firstMoved = false;
    private float timeout = 1.5f;
    private bool playerTurn;
    private Stack<UndoController> turns = new Stack<UndoController>();

    private List<Target> _targetsOnScreen;
    private List<Attacks> _attackMarkersOnScreen;
    private Target _player;
    private Target _enemy;
    private GameObject _playerObj;
    private GameObject _enemyObj;

    public GameObject AquarihnObj;
    public GameObject PyroscarabObj;
    public GameObject PricklashObj;

    private bool Paused;
    public GameObject UI;

    public void OnFindTarget(String target)
    {
        Debug.Log(target);
        Enum.TryParse(target, out Target targetEnum);
        _targetsOnScreen.Add(targetEnum);
    }

    public void OnLoseTarget(String target)
    {
        Debug.Log(target);

        Enum.TryParse(target, out Target targetEnum);
        _targetsOnScreen.Remove(targetEnum);
    }



    private bool PlayerAttack()
    {
        playerTurn = true;
        Debug.Log(_attackMarkersOnScreen.Count);
        int playerAttack;
        if (_attackMarkersOnScreen.Count > 1)
        {
            markerErrorScreen.SetActive(true);
            messageMarkerError.text = "Make sure to only have one attack on screen at the any time";
            titleMarkerError.text = "Too many attack indicators are on screen";
            return false;
        }
        if (_attackMarkersOnScreen.Count == 0)
        {
            markerErrorScreen.SetActive(true);
            titleMarkerError.text = "You have not selected an attack";
            messageMarkerError.text = "Put an attack marker in the field of view to perform an attack";
            return false;
        }
        
        switch (_attackMarkersOnScreen[0])
        {
            case Attacks.Normal:
                playerAttack = 0;
                break;
            case Attacks.Special:
                playerAttack = 1;
                break;
            default:
                playerAttack = -1;
                break;
        }
        
        
        if (playerAttack == -1) return false;
        markerErrorScreen.SetActive(false);
        turns.Push(monster1.Fight(playerAttack, monster2, fightInfo));
        playerTurn = false;

        return true;
        
    }
    

    public void OnFindAttack(String attack)
    {
        Enum.TryParse(attack, out Attacks attackEnum);
        _attackMarkersOnScreen.Add(attackEnum);

    }
    
    public void OnLoseAttack(String attack)
    {
        Enum.TryParse(attack, out Attacks attackEnum);

        _attackMarkersOnScreen.Remove(attackEnum);
    }

    private void CheckForMarkers()
    {
        
        var differentTarget = _targetsOnScreen.Exists((target => (target != _player && target != _enemy)));
        if (differentTarget)
        {
            StartCoroutine(RunAwayTimer("Only the monsters battling should be on the field. Remove the extra cards."));
            return;
        }

        if (_targetsOnScreen.Count > 2)
        {
            StartCoroutine(RunAwayTimer("Only the monsters battling should be on the field. Remove the extra cards."));
            return;
        }

        if (_targetsOnScreen.Count < 2)
        {
            if (!_targetsOnScreen.Contains(_player))
            {
                StartCoroutine(RunAwayTimer("Cannot find the player card (" + _player + "). Add it back to the field."));
            }else if (!_targetsOnScreen.Contains(_enemy))
            {
                StartCoroutine(RunAwayTimer("Cannot find the enemy card (" + _enemy + "). Add it back to the field."));
            }

            return;
        }
        
        if (!DistanceUtils.IsBattlePosition(_playerObj, _enemyObj))
        {
            StartCoroutine(RunAwayTimer("The cards must be positioned close together and facing each other to battle."));
            return;
        }
                    
        DontRunAway();
    }
    
    IEnumerator RunAwayTimer(String message)
    {
        Paused = true;
        UI.SetActive(false);
        runAwayScreen.SetActive(true);
        messageObj.text = message;
        time.text = "5s";
        yield return new WaitForSeconds(1);
        time.text = "4s";

        yield return new WaitForSeconds(1);
        time.text = "3s";

        yield return new WaitForSeconds(1);
        time.text = "2s";

        yield return new WaitForSeconds(1);
        time.text = "1s";

        yield return new WaitForSeconds(1);
        time.text = "0s";

        RunAway();
    }

    public void OnUndo()
    {
        if (!midTurn && turn > 1)
        {
            turn--;
        }
        midTurn = !midTurn;
        turns.Pop().Undo();
    }

    public void RunAway()
    {
        SceneManager.LoadScene("SampleScene");
    }

    private void DontRunAway()
    {
        StopAllCoroutines();
        runAwayScreen.SetActive(false);
        UI.SetActive(true);
        Paused = false;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.AutoRotation;
        CaptureInfo.battleEnded = false;
        _targetsOnScreen = new List<Target>();
        _attackMarkersOnScreen = new List<Attacks>();
        _player = CaptureInfo.PlayerTarget;
        _enemy = CaptureInfo.EnemyTarget;
        _targetsOnScreen.Add(_player);
        _targetsOnScreen.Add(_enemy);
        
        switch (CaptureInfo.PlayerTarget)
        {
            case Target.Aquarhin:
                _playerObj = AquarihnObj;
                monster1 = _playerObj.GetComponentInChildren<FightRhinoController>();

                playerName.text = "Aquarhin";
                
                break;

            case Target.Pricklash:
                _playerObj = PricklashObj;
                playerName.text = "Pricklash";
                monster1 = _playerObj.GetComponentInChildren<FightCactusController>();

                break;

            case Target.Pyroscarab:
                _playerObj = PyroscarabObj;
                monster1 = _playerObj.GetComponentInChildren<FightInsectController>();

                playerName.text = "Pyroscarab";
                break;
        }

        _playerObj.SetActive(true);

        
        switch (CaptureInfo.EnemyTarget)
        {
            case Target.Aquarhin:
                _enemyObj = AquarihnObj;
                enemyName.text = "Aquarhin";
                monster2 = _enemyObj.GetComponentInChildren<FightRhinoController>();

                break;

            case Target.Pricklash:
                _enemyObj = PricklashObj;
                enemyName.text = "Pricklash";
                monster2 = _enemyObj.GetComponentInChildren<FightCactusController>();

                break;

            case Target.Pyroscarab:
                _enemyObj = PyroscarabObj;
                monster2 = _enemyObj.GetComponentInChildren<FightInsectController>();

                enemyName.text = "Pyroscarab";
                break;
        }
        
        _enemyObj.SetActive(true);


    }

    // Update is called once per frame
    void Update()
    {
        CheckForMarkers();

        if (!Paused && !isOver)
        {
            UpdateUI();
            if (timeout < 0)
            {
                FightTurn();
                CheckHealth();
                timeout = 1.5f;
            }
            else
            {
                timeout -= Time.deltaTime;
            }
        }

        if (isOver)
        {
            SceneManager.LoadScene("SampleScene");
        }
        
    }

    private void FightTurn()
    {
        if (monster1.Speed > monster2.Speed)
        {
            if (midTurn)
            {
                playerTurn = false;
                turns.Push(monster2.Fight(RandomAttack(), monster1, fightInfo));
                midTurn = false;
                turn++;
                playerTurn = true;

            }
            else
            {
                if (!PlayerAttack()) return;
                midTurn = true;
            }
        }
        else if (monster1.Speed == monster2.Speed)
        {
            if (midTurn && firstMoved)
            {
                playerTurn = false;
                turns.Push(monster2.Fight(RandomAttack(), monster1, fightInfo));
                midTurn = false;
                turn++;
                playerTurn = true;

            }
            else if (midTurn)
            {
                if (!PlayerAttack()) return;
                midTurn = false;
                turn++;
            }
            else
            {
                if (RNG(0.5f))
                {
                    if (!PlayerAttack()) return;
                    firstMoved = true;
                    midTurn = true;
                    
                }
                else
                {
                    playerTurn = false;

                    turns.Push(monster2.Fight(RandomAttack(), monster1, fightInfo));
                    firstMoved = false;
                    midTurn = true;
                    playerTurn = true;

                }
            }
        }
        else
        {
            if (midTurn)
            {
                if (!PlayerAttack()) return;
                midTurn = false;
                turn++;
            }
            else
            {
                playerTurn = false;

                turns.Push(monster2.Fight(RandomAttack(), monster1, fightInfo));
                midTurn = true;
                playerTurn = true;

            }
        }
    }

    private bool RNG(float threshold)
    {
        return Random.Range(0f, 1f) > threshold;
    }

    private int RandomAttack()
    {
        if (RNG(0.7f))
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    private void CheckHealth()
    {
        if (monster1.Health == 0)
        {
            isOver = true;
            winner = 1;
            monster1.Animator.SetBool("IsDead", true);
            CaptureInfo.PlayerWon = false;
            CaptureInfo.battleEnded = true;
            return;
        }
        if (monster2.Health == 0)
        {
            isOver = true;
            CaptureInfo.capturedTargets.Add(_enemy);
            CaptureInfo.PlayerWon = true;
            monster2.Animator.SetBool("IsDead", true);
            winner = 2;
            CaptureInfo.battleEnded = true;
        }
        
        
    }

    private void UpdateUI()
    {
        playerHealth.value = (float)monster1.Health / monster1.MaxHealth;
        enemyHealth.value = (float)monster2.Health / monster2.MaxHealth;

        if (playerHealth.value < 0.25f)
        {
            playerHealthFill.color = Color.red;
        }
        else
        {
            playerHealthFill.color = Color.green;
        }
        if (enemyHealth.value < 0.25f)
        {
            enemyHealthFill.color = Color.red;
        }
        else
        {
            enemyHealthFill.color = Color.green;
        }

        if (turn == 1 && !midTurn)
        {
            undo.enabled = false;
        }
        else
        {
            undo.enabled = true;
        }

        turnText.text = (playerTurn ? "It's your " : "It's the enemy's ") + "turn";

        switch(monster1.Effect)
        {
            case null:
            case "None":
                playerBurned.enabled = false;
                playerPoisoned.enabled = false;
                playerConfused.enabled = false;
                break;

            case "Burned":
                playerBurned.enabled = true;
                playerPoisoned.enabled = false;
                playerConfused.enabled = false;
                break;

            case "Poisoned":
                playerBurned.enabled = false;
                playerPoisoned.enabled = true;
                playerConfused.enabled = false;
                break;

            case "Confused":
                playerBurned.enabled = false;
                playerPoisoned.enabled = false;
                playerConfused.enabled = true;
                break;
        }

        switch (monster2.Effect)
        {
            case null:
            case "None":
                enemyBurned.enabled = false;
                enemyPoisoned.enabled = false;
                enemyConfused.enabled = false;
                break;

            case "Burned":
                enemyBurned.enabled = true;
                enemyPoisoned.enabled = false;
                enemyConfused.enabled = false;
                break;

            case "Poisoned":
                enemyBurned.enabled = false;
                enemyPoisoned.enabled = true;
                enemyConfused.enabled = false;
                break;

            case "Confused":
                enemyBurned.enabled = false;
                enemyPoisoned.enabled = false;
                enemyConfused.enabled = true;
                break;
        }
    }
}
