using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class MonsterGeneric : MonoBehaviour
{
    public int Health { get; set; }

    public int MaxHealth { get; set; }

    public int Speed { get; set; }

    public int Attack { get; set; }

    public int Defense { get; set; }

    public string Type { get; set; }

    public string Name { get; set; }

    public int EffectTurns { get; set; }

    public string Effect { get; set; }

    public Animator Animator { get; set; }

    public List<MonsterAttack> Attacks { get; set; }

    public UndoController Fight(int index, MonsterGeneric monster, TextMeshProUGUI info)
    {
        UndoController turn = new UndoController();
        turn.attacker = this;
        turn.defender = monster;
        turn.attack = index;
        info.text = "";

        switch (Name)
        {
            case "Aquarhin":
                info.color = new Color(0, 0, 0.5f);
                break;

            case "Pricklash":
                info.color = new Color(0, 0.5f, 0);
                break;

            case "Pyroscarab":
                info.color = new Color(0.5f, 0, 0);
                break;
        }

        if (EffectTurns != 0 && Effect == "Confused") 
        {
            turn.effected = true;
            turn.firstEffectedTurn = false;
            turn.effect = "Confused";

            info.text += Name + " is confused\n";

            EffectTurns--;

            if (Random.Range(0, 1) > 0.75f)
            {
                int deficit1 = 0;

                Health -= (int)(Attack / 2.5f);
                if (Health < 0)
                {
                    deficit1 = Health;
                    Health = 0;
                }

                turn.effectedDamage = (int)(Attack / 2.5f) + deficit1;
                turn.damage = 0;

                info.text += "It hit itself";

                return turn;
            }
        }

        switch(index)
        {
            case 0:
                Animator.SetTrigger("Attack1");
                break;

            case 1:
                Animator.SetTrigger("Attack2");
                break;
        }

        MonsterAttack attack = Attacks[index];

        float defenseModified = monster.Defense * TypeVariation(attack.Type, monster.Type);

        float damage = CalculateDamage(Attack, attack.Power, defenseModified, Stab(attack.Type));

        info.text += Name + " used " + attack.Name + "\n" + monster.Name + " took " + (int)damage;

        monster.Health -= (int)damage;
        monster.Animator.SetTrigger("IsHit");

        int deficit2 = 0;

        if (monster.Health < 0)
        {
            deficit2 = Health;
            monster.Health = 0;
        }

        turn.damage = (int)(damage) + deficit2;

        if (attack.HasSpecialEffect) 
        {
            attack.SpecialEffect?.Invoke(monster);
            
            if (monster.EffectTurns == 5)
            {
                turn.firstEffectedTurn = true;
                turn.effected = true;
                turn.effectedDamage = 0;
                turn.effect = monster.Effect;
            }
        }

        if (EffectTurns != 0 && Effect == "Poisoned")
        {
            turn.effected = true;
            turn.firstEffectedTurn = false;
            turn.effect = "Poisoned";

            int deficit3 = 0;

            Health -= MaxHealth / 9;
            if (Health < 0)
            {
                deficit3 = Health;
                Health = 0;
            }

            turn.effectedDamage = MaxHealth / 9 + deficit3;

            info.text += "\n" + Name + " is poisoned";

            EffectTurns--;
        }

        if (EffectTurns != 0 && Effect == "Burned")
        {
            turn.effected = true;
            turn.firstEffectedTurn = false;
            turn.effect = "Burned";
            turn.effectedDamage = 0;

            info.text += "\n" + Name + " is burned";

            EffectTurns--;
        }

        if (EffectTurns == 0 && (Effect == "Poisoned" || Effect == "Confused" || Effect == "Burned"))
        {
            Effect = "None";
        }

        return turn;
    }

    float CalculateDamage(int userAttack, int movePower, float enemyDefense, bool hasStab)
    {
        if (hasStab)
        {
            return 1.25f * userAttack * movePower * Random.Range(0.85f, 1.15f) / enemyDefense;
        }
        else
        {
            return userAttack * movePower * Random.Range(0.85f, 1.15f) / enemyDefense;
        }
    }

    bool Stab(string attackType)
    {
        return attackType == Type;
    }

    float TypeVariation(string attackType, string enemyType)
    {
        if (attackType == "Fire" && enemyType == "Water" ||
            attackType == "Water" && enemyType == "Grass" ||
            attackType == "Grass" && enemyType == "Fire")
        {
            return 1.25f;
        }

        if (attackType == "Fire" && enemyType == "Grass" ||
            attackType == "Grass" && enemyType == "Water" ||
            attackType == "Water" && enemyType == "Fire")
        {
            return 0.75f;
        }

        return 1.0f;
    }
}
