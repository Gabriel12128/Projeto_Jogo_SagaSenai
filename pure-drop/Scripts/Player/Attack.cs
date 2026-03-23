using Godot;
using System.Threading.Tasks;

public partial class Attack
{
    private int comboStep = 0;
    private bool atacando = false;
    public bool EstaAtacando => atacando;

    private float damageDistance = 1.6f; // Alcance do soco (X)
    private float depthLimit = 0.7f;     // "Espessura" da linha no chão (Z)
    private int dano = 1;
    private float hitTime = 0.3f;

    public async Task AttackPlayer(AnimationPlayer animationPlayer, Player player)
    {
        if (atacando && comboStep != 1) return;

        if (!atacando)
        {
            atacando = true;
            comboStep = 1;
            animationPlayer.Play("attack2");

            await player.ToSignal(player.GetTree().CreateTimer(hitTime), "timeout");
            ApplyDamage(player);

            await player.ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
            if (comboStep == 1) ResetCombo();
        }
        else if (comboStep == 1)
        {
            comboStep = 2;
            animationPlayer.Play("attack1");

            await player.ToSignal(player.GetTree().CreateTimer(hitTime), "timeout");
            ApplyDamage(player);

            await player.ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
            ResetCombo();
        }
    }

    private void ApplyDamage(Player player)
    {
        var enemies = player.GetTree().GetNodesInGroup("enemy");
        
        // Se FlipH for true, o player olha para a Esquerda (-1), senão Direita (1)
        float facing = player.GetSprite().FlipH ? -1f : 1f;

        foreach (Node node in enemies)
        {
            if (node is Enemy1 enemy)
            {
                Vector3 diff = enemy.GlobalPosition - player.GlobalPosition;

                // 1. Verifica se o inimigo está do mesmo lado que o player olha (X)
                bool naFrenteX = (facing > 0 && diff.X > 0) || (facing < 0 && diff.X < 0);
                
                // 2. Verifica distância horizontal (X)
                bool noAlcanceX = Mathf.Abs(diff.X) <= damageDistance;

                // 3. Verifica se estão na mesma linha de profundidade (Z)
                bool mesmaLinhaZ = Mathf.Abs(diff.Z) <= depthLimit;

                if (naFrenteX && noAlcanceX && mesmaLinhaZ)
                {
                    // Direção do knockback baseada no olhar do player
                    Vector3 knockDir = new Vector3(facing, 0, 0);
                    enemy.LevarDano(dano, knockDir);
                }
            }
        }
    }

    private void ResetCombo()
    {
        atacando = false;
        comboStep = 0;
    }
}