using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightRhinoController : MonsterGeneric
{
    // Start is called before the first frame update
    void Start()
    {
        Health = 110;
        MaxHealth = 110;
        Attack = 10;
        Defense = 15;
        Speed = 8;
        Type = "Water";
        Name = "Aquarhin";
        Animator = GetComponent<Animator>();
        
        var attack1 = gameObject.AddComponent<MonsterAttack>();
        attack1.Type = "Normal";
        attack1.Slots = 10;
        attack1.Power = 11;
        attack1.HasSpecialEffect = false;
        attack1.Name = "Skewer";

        var attack2 = gameObject.AddComponent<MonsterAttack>();
        attack2.Type = "Water";
        attack2.Slots = 5;
        attack2.Power = 14;
        attack2.Name = "Shout";
        attack2.HasSpecialEffect = true;
        attack2.SpecialEffect = (enemy) =>
        {
            if (enemy.Effect != "Confused")
            {
                enemy.Effect = "Confused";
                enemy.EffectTurns = 5;
            }
        };

        Attacks = new List<MonsterAttack> { attack1, attack2 };
    }
}
