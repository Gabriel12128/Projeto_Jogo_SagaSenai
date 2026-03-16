using Godot;
using System.Threading.Tasks;

public partial class Attack
{
    private int comboStep = 0;
    private bool atacando = false;

    public bool EstaAtacando => atacando;

    public async Task AttackPlayer(AnimationPlayer animationPlayer)
    {
        if (!atacando)
        {
            atacando = true;
            comboStep = 1;

            animationPlayer.Play("attack2");

            await animationPlayer.ToSignal(
                animationPlayer,
                AnimationPlayer.SignalName.AnimationFinished
            );

            atacando = false;
            comboStep = 0;
        }
        else if (comboStep == 1)
        {
            comboStep = 2;

            animationPlayer.Play("attack1");
        }
    }
}