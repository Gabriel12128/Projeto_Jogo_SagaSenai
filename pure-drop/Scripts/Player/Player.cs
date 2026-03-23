using Godot;
using System.Threading.Tasks;

public partial class Player : CharacterBody3D
{
    [Export] private Sprite3D sprite;
    [Export] private AnimationPlayer animationPlayer;

    private Attack attack = new Attack();
    private Movement movement = new Movement();

    public int vidaMax = 5;
    public int vida = 5;

    [Export] private float invencibilidadeTempo = 1.0f;
    [Export] private float blinkInterval = 0.1f;
    [Export] private float knockbackForce = 8f;
    [Export] private float knockbackDecay = 10f;

    private bool tomandoDano = false;
    private bool invencivel = false;
    private Vector3 knockbackVelocity = Vector3.Zero;

    [Export] private Area3D col;

    // --- NOVIDADE: Método para o Attack.cs acessar o sprite ---
    public Sprite3D GetSprite() => sprite;

    public override void _Ready()
    {
        AddToGroup("player");
        col.AreaEntered += OnAreaEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;
        movement.ApplyGravity(ref velocity, delta, this);
        ApplyKnockback(ref velocity, (float)delta);

        bool travado = attack.EstaAtacando || tomandoDano || knockbackVelocity.Length() > 0.1f;

        if (!travado)
        {
            movement.HandleJump(ref velocity, this, false);
            movement.HandleMovement(ref velocity, animationPlayer, sprite, this, false);
        }
        else
        {
            velocity.X = knockbackVelocity.X;
            velocity.Z = knockbackVelocity.Z;
        }

        if (Input.IsActionJustPressed("ataque") && !tomandoDano)
        {
            // Passamos 'this' (o próprio Player) para o Attack
            _ = attack.AttackPlayer(animationPlayer, this);
        }

        Velocity = velocity;
        MoveAndSlide();
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

    public async void LevarDano(int dano, Vector3 direcao)
    {
        if (invencivel) return;
        vida -= dano;
        tomandoDano = true;
        invencivel = true;
        knockbackVelocity = direcao * knockbackForce;
        animationPlayer.Play("Hit");
        _ = BlinkEffect();

        await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        tomandoDano = false;

        await ToSignal(GetTree().CreateTimer(invencibilidadeTempo), "timeout");
        invencivel = false;
        sprite.Visible = true;

        if (vida <= 0) Morrer();
    }

    private async Task BlinkEffect()
    {
        float tempo = 0f;
        while (invencivel)
        {
            sprite.Visible = !sprite.Visible;
            await ToSignal(GetTree().CreateTimer(blinkInterval), "timeout");
            tempo += blinkInterval;
            if (tempo >= invencibilidadeTempo) break;
        }
        sprite.Visible = true;
    }

    private void Morrer()
    {
        animationPlayer.Play("Death");
        SetProcess(false);
        SetPhysicsProcess(false);
    }

    private void OnAreaEntered(Area3D area)
    {

        var coletavel = area as IColetaveis;



        if (coletavel == null)

        {

            coletavel = area.GetParent() as IColetaveis;

        }



        if (coletavel != null)

        {

            GD.Print("encostou");

            coletavel.Execute(this);

        }

    }
}