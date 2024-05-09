
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;



    public class BattlePromptController: MonoBehaviour
    {
        
        public Button buttonYes;
        public Button buttonNo;


        public delegate void BattlePromptCooldown();

        public static event BattlePromptCooldown OnBattlePromptCooldown;

        
        private void Start()
        {
            buttonNo.onClick.AddListener(Close);
            buttonYes.onClick.AddListener(StartBattle);
        }
        

        private void Close()
        {
            OnBattlePromptCooldown();
        }
        
        private void StartBattle()
        {
            SceneManager.LoadScene("FightScene");
        }
        
    }
