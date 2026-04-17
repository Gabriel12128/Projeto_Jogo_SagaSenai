using Godot;
using System;
using System.Threading.Tasks;

public partial class EnemyGarrafa : CharacterBody3D
{
    private enum State { Idle, Chase, KeepDistance, AttackMelee, AttackRange, Hit, Death }
    private State currentState = State.Idle;

    [ExportGroup("Referências")]
    [Export] private AnimationPlayer animationPlayer;
    [Export] private Sprite3D sprite;
    [Export] private PackedScene projetilScene;
    [Export] private Marker3D spawnProjetil;
    [Export] private AudioStreamPlayer2D somAtaque;
    [Export] private AudioStreamPlayer2D somDano;

    [ExportGroup("Visão e Percepção")]
    [Export] private float distanciaPercepcao = 12.0f;
    private bool playerDetectado = false;

    [ExportGroup("Configurações de Vida")]
    [Export] private int vida = 5;
    [Export] private int danoMelee = 1;

    [ExportGroup("Configurações de Distância")]
    [Export] private float speed = 3.5f;
    [Export] private float rangeIdealMax = 8.0f;  
    [Export] private float rangeIdealMin = 4.0f;  
    [Export] private float rangeMelee = 1.6f;     
    [Export] private float limiteFuga = 10.0f;    

    [ExportGroup("Tempos e Combate")]
    [Export] private float tempoParaFugir = 0.6f; 
    [Export] private float cooldownAtaque = 1.8f;
    [Export] private float knockbackForce = 6.0f;

    private Node3D player;
    private Vector3 currentVelocity = Vector3.Zero;
    private Vector3 knockbackVelocity = Vector3.Zero;
    private bool morto = false;
    private bool podeAtacar = true;
    private bool reagindoAFuga = false;
    private bool jaDropou = false;

    public override void _Ready()
    {
        AddToGroup("enemy");
        player = GetTree().GetFirstNodeInGroup("player") as Node3D;
    }

    public override void _PhysicsProcess(double delta)
    {
        currentVelocity = Velocity;
        ApplyGravity(ref currentVelocity, delta);

        if (morto)
        {
            ApplyFriction(delta);
            Velocity = currentVelocity;
            MoveAndSlide();
            return;
        }

        if (knockbackVelocity.Length() > 0.1f)
        {
            currentVelocity.X = knockbackVelocity.X;
            currentVelocity.Z = knockbackVelocity.Z;
            knockbackVelocity = knockbackVelocity.MoveToward(Vector3.Zero, 10.0f * (float)delta);
        }
        else
        {
            UpdateAI(delta);
        }

        Velocity = currentVelocity;
        MoveAndSlide();
    }

    private void UpdateAI(double delta)
    {
        if (player == null || (player is Player p && p.morto))
        {
            VoltarParaIdle();
            return;
        }

        float dist = GetDistanceToPlayer();

        if (!playerDetectado)
        {
            if (dist <= distanciaPercepcao)
            {
                playerDetectado = true;
            }
            else
            {
                VoltarParaIdle();
                return;
            }
        }
        else
        {
            if (dist > distanciaPercepcao * 1.5f)
            {
                playerDetectado = false;
                VoltarParaIdle();
                return;
            }
        }

        if (currentState == State.AttackMelee || currentState == State.AttackRange || currentState == State.Hit) return;

        Vector3 dirToPlayer = (player.GlobalPosition - GlobalPosition).Normalized();

        if (dist < rangeIdealMin)
        {
            if (dist <= rangeMelee && podeAtacar)
            {
                _ = AtacarMelee();
                return;
            }

            if (!reagindoAFuga && currentState != State.KeepDistance)
            {
                _ = IniciarReacaoFuga();
            }
        }

        if (currentState == State.KeepDistance)
        {
            if (dist >= rangeIdealMin + 1.0f || dist >= limiteFuga)
            {
                currentState = State.Idle;
            }
            else
            {
                Vector3 direcaoFuga = new Vector3(dirToPlayer.X, 0, dirToPlayer.Z * 0.1f).Normalized();
                MoverPara(direcaoFuga, -1); 
                FlipSprite(dirToPlayer);
                return;
            }
        }

        if (dist <= rangeIdealMax && dist >= rangeIdealMin)
        {
            StopMovement();
            _ = AtacarRange();
        }
        else if (dist > rangeIdealMax)
        {
            currentState = State.Chase;
            MoverPara(dirToPlayer, 1);
        }

        FlipSprite(dirToPlayer);
    }

    private void VoltarParaIdle()
    {
        currentState = State.Idle;
        PlayAnimation("Idle");
        StopMovement();
    }

    private async Task IniciarReacaoFuga()
    {
        if (reagindoAFuga || !IsInsideTree()) return;
        reagindoAFuga = true;
        await ToSignal(GetTree().CreateTimer(tempoParaFugir), "timeout");
        if (IsInstanceValid(this) && !morto && currentState != State.Hit)
        {
            if (GetDistanceToPlayer() < rangeIdealMin) currentState = State.KeepDistance;
        }
        reagindoAFuga = false;
    }

    private void MoverPara(Vector3 direcao, int sinal)
    {
        Vector3 vel = Vector3.Zero;
        if (sinal == -1)
        {
            vel.X = direcao.X * speed * sinal;
            vel.Z = (direcao.Z * 0.2f) * speed * sinal; 
        }
        else 
        {
            vel.X = direcao.X * speed;
            vel.Z = direcao.Z * speed;
        }
        currentVelocity.X = vel.X;
        currentVelocity.Z = vel.Z;
        PlayAnimation("Walk");
    }

    private async Task AtacarMelee()
    {
        if (!podeAtacar || morto || !IsInsideTree()) return;
        podeAtacar = false;
        currentState = State.AttackMelee;
        StopMovement();
        PlayAnimation("AttackSoco");
        await ToSignal(GetTree().CreateTimer(0.4f), "timeout");
        if (IsInstanceValid(this) && !morto && GetDistanceToPlayer() < rangeMelee + 0.5f)
        {
            if (player.HasMethod("LevarDano"))
                player.Call("LevarDano", danoMelee, (player.GlobalPosition - GlobalPosition).Normalized());
        }
        await ToSignal(GetTree().CreateTimer(cooldownAtaque), "timeout");
        podeAtacar = true;
        if (IsInstanceValid(this) && !morto) currentState = State.Idle;
    }

    private async Task AtacarRange()
    {
        if (!podeAtacar || morto || !IsInsideTree()) return;
        podeAtacar = false;
        currentState = State.AttackRange;
        StopMovement();
        PlayAnimation("AttackDis");
        await ToSignal(GetTree().CreateTimer(0.6f), "timeout");
        if (IsInstanceValid(this) && !morto) DispararProjetil();
        await ToSignal(GetTree().CreateTimer(cooldownAtaque), "timeout");
        podeAtacar = true;
        if (IsInstanceValid(this) && !morto) currentState = State.Idle;
    }

    private void DispararProjetil()
    {
        if (projetilScene == null || spawnProjetil == null) return;
        var proj = projetilScene.Instantiate<Area3D>();
        GetParent().AddChild(proj);
        proj.GlobalPosition = spawnProjetil.GlobalPosition;
        Vector3 dir = (player.GlobalPosition - GlobalPosition).Normalized();
        if (proj.HasMethod("SetDirection")) proj.Call("SetDirection", dir);
        if (somAtaque != null) somAtaque.Play();
    }

    public void LevarDano(int danoRecebido, Vector3 direcao)
    {
        if (morto) return;
        vida -= danoRecebido;
        if (somDano != null) somDano.Play();
        knockbackVelocity = direcao * knockbackForce;
        if (vida <= 0) { Morrer(); return; }
        currentState = State.Hit;
        PlayAnimation("Hit");
        SceneTree tree = GetTree();
        if (tree != null)
        {
            tree.CreateTimer(0.4f).Connect("timeout", Callable.From(() => {
                if (IsInstanceValid(this) && !morto) currentState = State.Idle;
            }));
        }
    }

    private void Morrer()
    {
        if (jaDropou) return;
        jaDropou = true;
        morto = true;
        currentState = State.Death;
        PlayAnimation("Death");
        CollisionLayer = 0;
        CollisionMask = 1; 
        GetTree().CreateTimer(1.5f).Connect("timeout", Callable.From(QueueFree));
    }

    private void ApplyGravity(ref Vector3 vel, double delta)
    {
        if (!IsOnFloor()) vel.Y += (float)GetGravity().Y * (float)delta;
        else vel.Y = 0;
    }

    private void StopMovement()
    {
        currentVelocity.X = Mathf.MoveToward(currentVelocity.X, 0, 1.5f);
        currentVelocity.Z = Mathf.MoveToward(currentVelocity.Z, 0, 1.5f);
    }

    private void ApplyFriction(double delta)
    {
        currentVelocity.X = Mathf.MoveToward(currentVelocity.X, 0, 2.0f * (float)delta);
        currentVelocity.Z = Mathf.MoveToward(currentVelocity.Z, 0, 2.0f * (float)delta);
    }

    private void PlayAnimation(string name)
    {
        if (animationPlayer != null && animationPlayer.HasAnimation(name) && animationPlayer.CurrentAnimation != name)
            animationPlayer.Play(name);
    }

    private void FlipSprite(Vector3 dir)
    {
        if (sprite != null)
        {
            if (dir.X < 0) sprite.FlipH = false;
            else if (dir.X > 0) sprite.FlipH = true;
        }
    }

    private float GetDistanceToPlayer() => GlobalPosition.DistanceTo(player.GlobalPosition);
}