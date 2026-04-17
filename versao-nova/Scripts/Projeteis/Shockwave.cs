using Godot;
using Intefaces;

public partial class Shockwave : Area3D, IProjetil
{
    [Export] private float speed = 5.0f;
    [Export] private float duration = 0.8f;
    private Vector3 direction;

    public override void _Ready()
    {
        // ERRO 1: Você esqueceu de conectar o sinal de colisão!
        BodyEntered += OnBodyEntered;

        // ERRO 2: Segurança para evitar erro ao resetar a cena
        SceneTree timer = GetTree();
        if (timer != null)
        {
            timer.CreateTimer(duration).Connect("timeout", Callable.From(QueueFree));
        }
    }

    public void SetDirection(Vector3 dir) => direction = dir;

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += direction * speed * (float)delta;
    }

    
    private void OnBodyEntered(Node3D body)
    {
        if (body is Player player)
        {
            Executar(player);
            
        }
    }

    public void Executar(Player player)
    {
       
    }
}