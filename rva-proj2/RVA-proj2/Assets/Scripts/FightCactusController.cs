using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightCactusController : MonsterGeneric
{
    // Start is called before the first frame update
    void Start()
    {
        Health = 120;
        MaxHealth = 120;
        Attack = 17;
        Defense = 7;
        Speed = 6;
        Type = "Grass";
        Name = "Pricklash";
        Animator = GetComponent<Animator>();

        var attack1 = gameObject.AddComponent<MonsterAttack>();
        attack1.Type = "Normal";
        attack1.Slots = 10;
        attack1.Power = 15;
        attack1.HasSpecialEffect = false;
        attack1.Name = "Punch";

        var attack2 = gameObject.AddComponent<MonsterAttack>();
        attack2.Type = "Grass";
        attack2.Slots = 4;
        attack2.Power = 8;
        attack2.Name = "Venom";
        attack2.HasSpecialEffect = true;
        attack2.SpecialEffect = (enemy) =>
        {
            if (enemy.Effect != "Poisoned")
            {
                enemy.Effect = "Poisoned";
                enemy.EffectTurns = 5;
            }
        };

        Attacks = new List<MonsterAttack> { attack1, attack2 };
    }
}
