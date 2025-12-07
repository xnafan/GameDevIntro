
using Microsoft.Xna.Framework;

namespace GameDevIntro.SimpleZuul.Components;
internal class Player
{
    public int Score { get; set; }
    public int HitPoints { get; set; } = MAX_HITPOINTS;
    public int AttackRollBonus { get; set; }
    public const int MAX_HITPOINTS = 25;
    public Point Position { get; set; }
    public void Reset()
    {
        Score = 0;
        HitPoints = MAX_HITPOINTS;
        AttackRollBonus = 0;
    }

    public void Heal(int amount)
    {
        HitPoints += amount;
        if (HitPoints > MAX_HITPOINTS)
        {
            HitPoints = MAX_HITPOINTS;
        }
    }
}