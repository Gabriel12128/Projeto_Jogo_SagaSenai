using Godot;
using System.Threading.Tasks;

public class Attack
{
    private int comboStep = 0;
    private bool atacando = false;
    public bool EstaAtacando => atacando;

    private int dano = 1;
    private float hitTime = 0.2f;

    public async Task AttackPlayer(AnimationPlayer animationPlayer, Player player, Area3D hitArea)
    {
        if (atacando && comboStep != 1) return;

        
        DeterminarDirecao(player, hitArea);

        if (!atacando)
        {
            atacando = true;
            comboStep = 1;
            animationPlayer.Play("attack2");

            await player.ToSignal(player.GetTree().CreateTimer(hitTime), "timeout");
            ApplyDamage(player, hitArea);

            await player.ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
            if (comboStep == 1) ResetCombo();
        }
        else if (comboStep == 1)
        {
            comboStep = 2;
            animationPlayer.Play("attack1");

            await player.ToSignal(player.GetTree().CreateTimer(hitTime), "timeout");
            ApplyDamage(player, hitArea);

            await player.ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
            ResetCombo();
        }
    }

    public void DeterminarDirecao(Player player, Area3D hitArea)
    {
        float inputX = Input.GetAxis("ui_left", "ui_right");
        Sprite3D sprite = player.GetSprite();

        
        if (inputX > 0) sprite.FlipH = false;
        else if (inputX < 0) sprite.FlipH = true;

     
        Vector3 rot = hitArea.RotationDegrees;
        rot.Y = sprite.FlipH ? 180f : 0f;
        hitArea.RotationDegrees = rot;
    }

    private void ApplyDamage(Player player, Area3D hitArea)
    {
      
        var corpos = hitArea.GetOverlappingBodies();
        var areas = hitArea.GetOverlappingAreas();

        float facing = player.GetSprite().FlipH ? -1f : 1f;
        Vector3 knockDir = new Vector3(facing, 0, 0);

      
        foreach (Node3D corpo in corpos)
        {
            ProcessarDano(corpo, knockDir);
        }

      
        foreach (Node3D areaInimiga in areas)
        {
            ProcessarDano(areaInimiga, knockDir);
        }
    }

    private void ProcessarDano(Node3D alvo, Vector3 knockDir)
    {
        
        if (alvo.IsInGroup("player")) return;

        if (alvo.IsInGroup("enemy") || alvo.GetParent().IsInGroup("enemy"))
        {
           
            if (alvo.HasMethod("LevarDano")) 
                alvo.Call("LevarDano", dano, knockDir);
            else if (alvo.GetParent().HasMethod("LevarDano"))
                alvo.GetParent().Call("LevarDano", dano, knockDir);
        }
    }

    private void ResetCombo()
    {
        atacando = false;
        comboStep = 0;
    }
}