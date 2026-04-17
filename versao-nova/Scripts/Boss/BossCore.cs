using Godot;
using System;
using System.Threading.Tasks;

public partial class BossCore : CharacterBody3D
{
    private enum State { Summoning, Invulnerable, Vulnerable, Attacking, Hit, Death }
    private State currentState = State.Summoning;

    [ExportGroup("Referências")]
    [Export] private AnimationPlayer anim;
    [Export] private Sprite3D sprite;
    [Export] private PackedScene minionScene;
    [Export] private Marker3D[] spawnPoints;
    [Export] private PackedScene shockwaveScene;
    [Export] private AudioStreamPlayer2D somDano;

    [ExportGroup("Configurações")]
    [Export] private int maxHealth = 30;
    [Export] private float stunDuration = 3.0f;
    [Export] private int minionsPerWave = 3;
    [Export] private int maxSummonWaves = 3; 
    [Export] private float shockwaveDistance = 3.0f;

    private int currentHealth;
    private int minionsAlive = 0;
    private int wavesDone = 0;
    private bool isInvulnerable = true;
    private Node3D player;

    public override void _Ready()
    {
        currentHealth = maxHealth;
        player = GetTree().GetFirstNodeInGroup("player") as Node3D;
        AddToGroup("enemy");
        ChangeState(State.Summoning);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (currentState == State.Death) return;

        // Gravidade básica
        if (!IsOnFloor())
        {
            Vector3 v = Velocity;
            v.Y += (float)GetGravity().Y * (float)delta;
            Velocity = v;
            MoveAndSlide();
        }

        UpdateStateLogic();
    }

    private void UpdateStateLogic()
    {
        if (player == null || currentState == State.Attacking || currentState == State.Hit) return;

        if (currentState == State.Invulnerable)
        {
            if (minionsAlive <= 0)
                ChangeState(State.Vulnerable);
        }
        else if (currentState == State.Vulnerable)
        {
            // TRAVA: Só ataca se a distância for curta E não estiver ocupado
            if (GlobalPosition.DistanceTo(player.GlobalPosition) < shockwaveDistance)
            {
                _ = ExecuteShockwaveAction();
            }
        }
    }

    private async void ChangeState(State newState)
    {
        if (currentState == State.Death) return;
        currentState = newState;

        switch (currentState)
        {
            case State.Summoning:
                isInvulnerable = true;
                PlayAnimation("Summon");
                await SummonMinions();
                if (IsInstanceValid(this)) ChangeState(State.Invulnerable);
                break;

            case State.Invulnerable:
                isInvulnerable = true;
                sprite.Modulate = new Color(0.3f, 0.5f, 1.0f);
                PlayAnimation("Idle");
                break;

            case State.Vulnerable:
                isInvulnerable = false;
                sprite.Modulate = new Color(1, 1, 1);
                PlayAnimation("Vulnerable_Idle");
                break;
        }
    }

    public void LevarDano(int dano, Vector3 direcao)
    {
        // Removido o 'currentState != State.Vulnerable' para permitir dano durante o ataque
        // Mas mantemos a invulnerabilidade de quando os minions estão vivos
        if (isInvulnerable || currentState == State.Death || currentState == State.Hit) return;

        if (somDano != null) somDano.Play();

        currentHealth -= dano;
        GD.Print("Vida do Boss: " + currentHealth); // Adicione esse log para testar no console

        if (currentHealth <= 0)
        {
            ChangeState(State.Death);
            _ = Morte();
            return;
        }

        _ = ProcessHit();
    }

    private async Task ProcessHit()
    {
        currentState = State.Hit;
        PlayAnimation("Hit");
        await ToSignal(GetTree().CreateTimer(stunDuration), "timeout");
        
        if (IsInstanceValid(this) && currentState != State.Death)
            await ExecuteShockwaveAction();
    }

    private async Task ExecuteShockwaveAction()
    {
        // Se já estiver atacando ou morto, ignora
        if (currentState == State.Death) return;
        
        currentState = State.Attacking;
        PlayAnimation("Shockwave_Attack");
        
        // Espera um pouco para a animação chegar no frame do golpe
        await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
        if (!IsInstanceValid(this)) return;

        SpawnWave(Vector3.Left);
        SpawnWave(Vector3.Right);

        // Tempo de espera para o ataque terminar de fato
        await ToSignal(GetTree().CreateTimer(1.5f), "timeout");
        
        if (IsInstanceValid(this) && currentState != State.Death)
        {
            if (wavesDone < maxSummonWaves)
                ChangeState(State.Summoning);
            else
                ChangeState(State.Vulnerable);
        }
    }

    private async Task SummonMinions()
    {
        if (!IsInstanceValid(this)) return;
        wavesDone++;
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

        for (int i = 0; i < minionsPerWave; i++)
        {
            if (minionScene == null || !IsInstanceValid(this)) break;
            
            var minion = minionScene.Instantiate<Node3D>();
            GetParent().AddChild(minion);
            
            int spawnIdx = i % spawnPoints.Length;
            minion.GlobalPosition = spawnPoints[spawnIdx].GlobalPosition;

            minionsAlive++;
            minion.TreeExited += () => { if (IsInstanceValid(this)) minionsAlive--; };
        }
    }

    private async Task Morte()
    {
        PlayAnimation("Death");
        await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
        if (IsInstanceValid(this)) QueueFree();
    }
    private void SpawnWave(Vector3 dir)
    {
        if (shockwaveScene == null) return;
        var wave = shockwaveScene.Instantiate<Node3D>();
        GetParent().AddChild(wave);
        wave.GlobalPosition = GlobalPosition;
        if (wave.HasMethod("SetDirection")) wave.Call("SetDirection", dir);
    }

    private void PlayAnimation(string name)
    {
        if (anim != null && anim.HasAnimation(name)) anim.Play(name);
    }
}