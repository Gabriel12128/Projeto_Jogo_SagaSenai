using Godot;
using System.Threading.Tasks;

public partial class Attack
{
    private int comboStep = 0;
    private bool atacando = false;

    public bool EstaAtacando => atacando;

    private float damageDistance = 1.8f;
    private int dano = 1;

    public async Task AttackPlayer(AnimationPlayer animationPlayer, Node3D player)
    {
        if (!atacando)
        {
            atacando = true;
            comboStep = 1;

            animationPlayer.Play("attack2");

            ApplyDamage(player);

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

            ApplyDamage(player);
        }
    }

    private void ApplyDamage(Node3D player)
    {
        var enemies = player.GetTree().GetNodesInGroup("enemy");

        foreach (Node node in enemies)
        {
            if (node is Enemy1 enemy)
            {
                float distance = player.GlobalPosition.DistanceTo(enemy.GlobalPosition);

                if (distance <= damageDistance)
                {
                    Vector3 dir = (enemy.GlobalPosition - player.GlobalPosition).Normalized();
                    enemy.LevarDano(dano, dir);
                }
            }
        }
    }
}