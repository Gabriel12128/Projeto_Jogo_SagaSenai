using Godot;
using Intefaces;
using System;

public partial class ProjetilGarrafa : Area3D, IProjetil
{
    [Export] private float speed = 10.0f;
    [Export] private int dano = 1;
    private Vector3 direction = Vector3.Zero;

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += direction * speed * (float)delta;
    }

    public void SetDirection(Vector3 dir)
    {
        
        direction = new Vector3(dir.X, 0, dir.Z).Normalized();
    }

   
    public async void Executar(Player player)
    {
        if (player == null || player.morto) return;
            player.LevarDano(dano, direction);

        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

        QueueFree();
    }
}
