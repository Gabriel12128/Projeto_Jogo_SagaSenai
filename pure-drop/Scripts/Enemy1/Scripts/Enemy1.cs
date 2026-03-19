using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;

public partial class Enemy1 : CharacterBody3D
{
    private enum State
    {
        Idle,
        Chase,
        Attack,
        Hit,
        Death
    }

    private State currentState = State.Idle;

    private Node3D player;

    [Export] private AnimationPlayer animationPlayer;
    [Export] private Sprite3D sprite;

    [Export] private float speed = 3f;
    [Export] private float detectionDistance = 6f;
    [Export] private float attackDistance = 1f;
    [Export] private float attackCooldown = 0.8f;

    [Export] private float separationDistance = 1f;
    [Export] private float separationForce = 1.5f;

    [Export] private int vida = 3;
    [Export] private int dano = 1;

    private bool podeAtacar = true;
    private bool morto = false;

    public override void _Ready()
    {
        AddToGroup("enemy");
        player = GetTree().GetFirstNodeInGroup("player") as Node3D;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (player == null || morto) return;

        Vector3 velocity = Velocity;

        ApplyGravity(ref velocity, delta);

        switch (currentState)
        {
            case State.Idle:
                UpdateIdle();
                break;

            case State.Chase:
                UpdateChase(ref velocity);
                break;

            case State.Attack:
                StopMovement(ref velocity);
                break;
        }

        // aplica separação SEMPRE (menos morto)
        ApplySeparation(ref velocity);

        Velocity = velocity;
        MoveAndSlide();
    }

    // =========================
    // GRAVIDADE
    // =========================

    private void ApplyGravity(ref Vector3 velocity, double delta)
    {
        if (!IsOnFloor())
            velocity += GetGravity() * (float)delta;
    }

    // =========================
    // IA
    // =========================

    private void UpdateIdle()
    {
        PlayAnimation("Idle");

        if (GetDistanceToPlayer() < detectionDistance)
            ChangeState(State.Chase);
    }

    private void UpdateChase(ref Vector3 velocity)
    {
        Vector3 direction = GetDirectionToPlayer();
        float distance = GetDistanceToPlayer();

        if (distance <= attackDistance && podeAtacar)
        {
            ChangeState(State.Attack);
            _ = Attack();
            return;
        }

        velocity.X = direction.X * speed;
        velocity.Z = direction.Z * speed;

        PlayAnimation("Walk");
        FlipSprite(direction);
    }

    private async Task Attack()
    {
        podeAtacar = false;

        Velocity = new Vector3(0, Velocity.Y, 0);

        PlayAnimation("attack2");

        TryDamagePlayer();

        await animationPlayer.ToSignal(
            animationPlayer,
            AnimationPlayer.SignalName.AnimationFinished
        );

        await ToSignal(GetTree().CreateTimer(attackCooldown), "timeout");

        podeAtacar = true;

        if (!morto)
            ChangeState(State.Chase);
    }

    // =========================
    // SEPARAÇÃO (ANTI-GRUDAR)
    // =========================

    private void ApplySeparation(ref Vector3 velocity)
    {
        var enemies = GetTree().GetNodesInGroup("enemy");

        Vector3 repulsion = Vector3.Zero;

        foreach (Node node in enemies)
        {
            if (node == this) continue;

            if (node is Enemy1 other)
            {
                float distance = GlobalPosition.DistanceTo(other.GlobalPosition);

                if (distance < separationDistance && distance > 0)
                {
                    Vector3 push = (GlobalPosition - other.GlobalPosition).Normalized();
                    repulsion += push / distance; // mais forte quando mais perto
                }
            }
        }

        velocity += repulsion * separationForce;
    }

    // =========================
    // DANO
    // =========================

    private void TryDamagePlayer()
    {
        float distance = GetDistanceToPlayer();

        if (distance <= attackDistance)
        {
            if (player is Player p)
            {
                Vector3 dir = (p.GlobalPosition - GlobalPosition).Normalized();
                p.LevarDano(dano, dir);
            }
        }
    }

    public async void LevarDano(int dano, Vector3 direcao)
    {
        if (morto) return;

        vida -= dano;

        ChangeState(State.Hit);

        Velocity = direcao * 5f;

        PlayAnimation("Hit");

        await animationPlayer.ToSignal(
            animationPlayer,
            AnimationPlayer.SignalName.AnimationFinished
        );

        if (vida <= 0)
            await Die();
        else
            ChangeState(State.Chase);
    }

    private async Task Die()
    {
        morto = true;
        ChangeState(State.Death);

        PlayAnimation("Death");

        await animationPlayer.ToSignal(
            animationPlayer,
            AnimationPlayer.SignalName.AnimationFinished
        );

        QueueFree();
    }

    // =========================
    // UTIL
    // =========================

    private void StopMovement(ref Vector3 velocity)
    {
        velocity.X = 0;
        velocity.Z = 0;
    }

    private void ChangeState(State newState)
    {
        currentState = newState;
    }

    private Vector3 GetDirectionToPlayer()
    {
        Vector3 dir = player.GlobalPosition - GlobalPosition;
        dir.Y = 0;
        return dir.Normalized();
    }

    private float GetDistanceToPlayer()
    {
        return GlobalPosition.DistanceTo(player.GlobalPosition);
    }

    private void PlayAnimation(string name)
    {
        if (animationPlayer.CurrentAnimation != name)
            animationPlayer.Play(name);
    }

    private void FlipSprite(Vector3 direction)
    {
        if (direction.X < 0)
            sprite.FlipH = true;
        else if (direction.X > 0)
            sprite.FlipH = false;
    }
}