using Godot;
using System.Threading.Tasks;

public partial class Enemy2 : CharacterBody3D
{
    private enum State { Idle, Chase, Attack }

    private State currentState = State.Idle;
    private Node3D player;

    [Export] private AnimationPlayer animationPlayer;
    [Export] private Sprite3D sprite;

    [ExportGroup("Configuracoes de Explosao")]
    [Export] private float explosionRadius = 3.5f;
    [Export] private int explosionDamage = 2;

    [ExportGroup("Configuracoes de Drop")]
    [Export] private PackedScene itemColetavel;

    [ExportGroup("Movimentacao")]
    [Export] private float speed = 3f;
    [Export] private float detectionDistance = 6f;
    [Export] private float attackDistance = 1.2f;
    [Export] private float attackDelay = 0.6f;

    [ExportGroup("Fisica e Combate")]
    [Export] private float separationDistance = 1f;
    [Export] private float separationForce = 1.5f;
    [Export] private float knockbackForce = 3f;
    [Export] private float knockbackDecay = 8f;
    [Export] private int vida = 3;

    private Vector3 knockbackVelocity = Vector3.Zero;
    private bool explodindo = false;
    private bool morto = false;

    public override void _Ready()
    {
        AddToGroup("enemy");
        player = GetTree().GetFirstNodeInGroup("player") as Node3D;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (player == null || morto || explodindo)
        {
            Velocity = new Vector3(0, IsOnFloor() ? 0 : GetGravity().Y * (float)delta, 0);
            MoveAndSlide();
            return;
        }

        Vector3 velocity = Velocity;
        
        if (!IsOnFloor())
            velocity += GetGravity() * (float)delta;

        if (knockbackVelocity.Length() > 0.01f)
        {
            velocity.X = knockbackVelocity.X;
            velocity.Z = knockbackVelocity.Z;
            knockbackVelocity = knockbackVelocity.MoveToward(Vector3.Zero, knockbackDecay * (float)delta);
        }

        switch (currentState)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.Chase:
                UpdateChase(ref velocity);
                break;
        }

        ApplySeparation(ref velocity);
        Velocity = velocity;
        MoveAndSlide();
    }

    private void UpdateIdle()
    {
        PlayAnimation("Idle");
        if (player != null && GetDistanceToPlayer() < detectionDistance)
            currentState = State.Chase;
    }

    private void UpdateChase(ref Vector3 velocity)
    {
        Vector3 direction = GetDirectionToPlayer();
        float distance = GetDistanceToPlayer();

        if (distance <= attackDistance)
        {
            _ = IniciarExplosao();
            return;
        }

        velocity.X = direction.X * speed;
        velocity.Z = direction.Z * speed;
        PlayAnimation("Walk");
        FlipSprite(direction);
    }

    private async Task IniciarExplosao()
    {
        if (explodindo || morto) return;
        explodindo = true;
        currentState = State.Attack;

        if (sprite != null)
            sprite.Modulate = new Color(10, 1, 1); 
        
        PlayAnimation("Idle");

        await ToSignal(GetTree().CreateTimer(attackDelay), "timeout");

        Explodir();
    }

    private void Explodir()
    {
        if (morto) return;
        morto = true;

        if (player != null && GetDistanceToPlayer() <= explosionRadius && player is Player p)
        {
            Vector3 dir = (p.GlobalPosition - GlobalPosition).Normalized();
            p.LevarDano(explosionDamage, dir * 2f);
        }

        DropItem();
        QueueFree();
    }

    private void ApplySeparation(ref Vector3 velocity)
    {
        var enemies = GetTree().GetNodesInGroup("enemy");
        Vector3 repulsion = Vector3.Zero;

        foreach (Node node in enemies)
        {
            if (node == this || node is not Node3D other) continue;
            float dist = GlobalPosition.DistanceTo(other.GlobalPosition);
            if (dist < separationDistance && dist > 0)
            {
                repulsion += (GlobalPosition - other.GlobalPosition).Normalized() / dist;
            }
        }
        velocity += repulsion * separationForce;
    }

    public void LevarDano(int dano, Vector3 direcao)
    {
        if (morto || explodindo) return;
        vida -= dano;

        if (vida <= 0)
        {
            Explodir();
            return;
        }

        knockbackVelocity = direcao * knockbackForce;
        PlayAnimation("Hit");
    }

    private void DropItem()
    {
        if (itemColetavel == null) return;

        Node3D item = itemColetavel.Instantiate<Node3D>();
        GetParent().AddChild(item);
        item.GlobalPosition = GlobalPosition + Vector3.Up * 0.5f;

        if (item is Lixo coletavelScript)
        {
            Vector3 impulso = new Vector3(
                (float)GD.RandRange(-3, 3),
                8.0f,
                (float)GD.RandRange(-3, 3)
            );
            coletavelScript.IniciarSalto(impulso);
        }
    }

    private Vector3 GetDirectionToPlayer()
    {
        if (player == null) return Vector3.Zero;
        Vector3 dir = player.GlobalPosition - GlobalPosition;
        dir.Y = 0;
        return dir.Normalized();
    }

    private float GetDistanceToPlayer() => player != null ? GlobalPosition.DistanceTo(player.GlobalPosition) : 999f;

    private void PlayAnimation(string name)
    {
        if (animationPlayer != null && animationPlayer.CurrentAnimation != name)
            animationPlayer.Play(name);
    }

    private void FlipSprite(Vector3 direction)
    {
        if (sprite != null && direction.X != 0)
            sprite.FlipH = direction.X < 0;
    }
}