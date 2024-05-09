using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightInsectController : MonsterGeneric
{
    // Start is called before the first frame update
    void Start()
    {
        Health = 100;
        MaxHealth = 100;
        Attack = 12;
        Defense = 12;
        Speed = 10;
        Type = "Fire";
        Name = "Pyroscarab";
        Animator = GetComponent<Animator>();

        var attack1 = gameObject.AddComponent<MonsterAttack>();
        attack1.Type = "Normal";
        attack1.Slots = 10;
        attack1.Power = 10;
        attack1.HasSpecialEffect = false;
        attack1.Name = "Smash";

        var attack2 = gameObject.AddComponent<MonsterAttack>();
        attack2.Type = "Fire";
        attack2.Slots = 5;
        attack2.Power = 13;
        attack2.Name = "Fire";
        attack2.HasSpecialEffect = true;
        attack2.SpecialEffect = (enemy) =>
        {
            if (enemy.Effect != "Burned")
            {
                enemy.Effect = "Burned";
                enemy.Defense -= 3;
                enemy.EffectTurns = 5;
            }
        };

        Attacks = new List<MonsterAttack> { attack1, attack2 };
    }
}
