using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;

public partial class Enemy1 : CharacterBody3D
{
    
    private enum State
    {
        Idle,
        Chase,
        Wait,
        Attack,
        Hit,
        Death
    }

    private State currentState = State.Idle;
    private Node3D player;

    [Export] private AnimationPlayer animationPlayer;
    [Export] private Sprite3D sprite;

    [ExportGroup("Configurações de Drop")]
    [Export] private PackedScene itemColetavel; // Arraste seu .tscn de item aqui

    [ExportGroup("Movimentação")]
    [Export] private float speed = 3f;
    [Export] private float detectionDistance = 6f;
    [Export] private float attackDistance = 1f;
    [Export] private float attackCooldown = 0.8f;
    [Export] private float attackDelay = 0.5f;
    [Export] private float waitBeforeRetry = 0.5f;

    [ExportGroup("Física e Combate")]
    [Export] private float separationDistance = 1f;
    [Export] private float separationForce = 1.5f;
    [Export] private float knockbackForce = 3f;
    [Export] private float knockbackDecay = 8f;
    [Export] private int vida = 3;
    [Export] private int dano = 1;

    private Vector3 knockbackVelocity = Vector3.Zero;
    private bool podeAtacar = true;
    private bool morto = false;

    // controle global
    private static bool alguemAtacando = false;

    public override void _Ready()
    {
        AddToGroup("enemy");
        player = GetTree().GetFirstNodeInGroup("player") as Node3D;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (player == null || morto)
        {
            Velocity = Vector3.Zero;
            MoveAndSlide();
            return;
        }

        Vector3 velocity = Velocity;
        ApplyGravity(ref velocity, delta);
        ApplyKnockback(ref velocity, (float)delta);

        switch (currentState)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.Chase:
                UpdateChase(ref velocity);
                break;
            case State.Wait:
                StopMovement(ref velocity);
                PlayAnimation("Idle");
                break;
            case State.Attack:
                StopMovement(ref velocity);
                break;
        }

        ApplySeparation(ref velocity);
        Velocity = velocity;
        MoveAndSlide();
    }

    private void ApplyGravity(ref Vector3 velocity, double delta)
    {
        if (!IsOnFloor())
            velocity += GetGravity() * (float)delta;
    }

    private void ApplyKnockback(ref Vector3 velocity, float delta)
    {
        if (knockbackVelocity.Length() > 0.01f)
        {
            velocity.X = knockbackVelocity.X;
            velocity.Z = knockbackVelocity.Z;
            knockbackVelocity = knockbackVelocity.MoveToward(Vector3.Zero, knockbackDecay * delta);
        }
    }

    private void UpdateIdle()
    {
        PlayAnimation("Idle");
        if (GetDistanceToPlayer() < detectionDistance)
            ChangeState(State.Chase);
    }

    private void UpdateChase(ref Vector3 velocity)
    {
        if (morto || vida <= 0) return;

        Vector3 direction = GetDirectionToPlayer();
        float distance = GetDistanceToPlayer();

        if (distance <= attackDistance && podeAtacar)
        {
            if (alguemAtacando)
            {
                ChangeState(State.Wait);
                _ = WaitAndRetry();
                return;
            }
            ChangeState(State.Attack);
            _ = Attack();
            return;
        }

        velocity.X = direction.X * speed;
        velocity.Z = direction.Z * speed;
        PlayAnimation("Walk");
        FlipSprite(direction);
    }

    private async Task WaitAndRetry()
    {
        podeAtacar = false;
        await ToSignal(GetTree().CreateTimer(waitBeforeRetry), "timeout");
        if (morto || vida <= 0) return;
        podeAtacar = true;
        ChangeState(State.Chase);
    }

    private async Task Attack()
    {
        if (morto || vida <= 0) return;
        podeAtacar = false;
        alguemAtacando = true;
        Velocity = new Vector3(0, Velocity.Y, 0);
        PlayAnimation("Idle");

        await ToSignal(GetTree().CreateTimer(attackDelay), "timeout");
        if (morto || vida <= 0) { alguemAtacando = false; return; }

        PlayAnimation("attack2");
        TryDamagePlayer();

        await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        await ToSignal(GetTree().CreateTimer(attackCooldown), "timeout");

        alguemAtacando = false;
        if (morto || vida <= 0) return;
        podeAtacar = true;
        ChangeState(State.Chase);
    }

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
                    repulsion += push / distance;
                }
            }
        }
        velocity += repulsion * separationForce;
    }

    private void TryDamagePlayer()
    {
        if (morto || vida <= 0) return;
        if (GetDistanceToPlayer() <= attackDistance && player is Player p)
        {
            Vector3 dir = (p.GlobalPosition - GlobalPosition).Normalized();
            p.LevarDano(dano, dir);
        }
    }

    public async void LevarDano(int dano, Vector3 direcao)
    {
        if (morto) return;
        vida -= dano;

        if (vida <= 0)
        {
            morto = true;
            podeAtacar = false;
            alguemAtacando = false;
            Velocity = Vector3.Zero;
            ChangeState(State.Death);
            PlayAnimation("Death");

            // --- LÓGICA DE DROP ADICIONADA ---
            DropItem();

            await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
            QueueFree();
            return;
        }

        ChangeState(State.Hit);
        knockbackVelocity = direcao * knockbackForce;
        PlayAnimation("Hit");
        await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        ChangeState(State.Chase);
    }

    private void DropItem()
    {
        if (itemColetavel == null) return;

        // 1. Instancia o item
        Node3D item = itemColetavel.Instantiate<Node3D>();

        // 2. Adiciona ao cenário (Pai do inimigo)
        GetParent().AddChild(item);

        // 3. DEFINE A ESCALA DESEJADA (Ajuste o 2.0f se precisar de mais ou menos)
        float tamanhoDesejado = 1.5f; 
        Vector3 escalaFinal = new Vector3(tamanhoDesejado, tamanhoDesejado, tamanhoDesejado);

        // 4. APLICA A TRANSFORMAÇÃO GLOBAL
        // Isso reseta rotação e aplica a escala 'escalaFinal' na posição do inimigo
        Transform3D t = item.GlobalTransform;
        t.Basis = Basis.Identity.Scaled(escalaFinal); 
        t.Origin = GlobalPosition + Vector3.Up * 0.5f;
        item.GlobalTransform = t;

        // 5. Inicia o pulo (Lógica da Area3D)
        if (item is Lixo coletavelScript)
        {
            float forcaSalto = 8.0f;
            float espalhar = 3.0f;
            
            Vector3 impulsoInicial = new Vector3(
                (float)GD.RandRange(-espalhar, espalhar),
                forcaSalto,
                (float)GD.RandRange(-espalhar, espalhar)
            );

            coletavelScript.IniciarSalto(impulsoInicial);
        }
    }

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

    private float GetDistanceToPlayer() => GlobalPosition.DistanceTo(player.GlobalPosition);

    private void PlayAnimation(string name)
    {
        if (animationPlayer.CurrentAnimation != name)
            animationPlayer.Play(name);
    }

    private void FlipSprite(Vector3 direction)
    {
        if (direction.X < 0) sprite.FlipH = true;
        else if (direction.X > 0) sprite.FlipH = false;
    }
}