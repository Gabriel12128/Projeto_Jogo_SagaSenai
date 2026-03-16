using Godot;

public partial class Player : CharacterBody3D
{
    [Export] private Sprite3D sprite;
    [Export] private AnimationPlayer animationPlayer;

    private Attack attack = new Attack();
    private Movement movement = new Movement();

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        movement.ApplyGravity(ref velocity, delta, this);
        movement.HandleJump(ref velocity, this, attack.EstaAtacando);

        movement.HandleMovement(
            ref velocity,
            animationPlayer,
            sprite,
            this,
            attack.EstaAtacando
        );

        if (Input.IsActionJustPressed("ataque"))
        {
            _ = attack.AttackPlayer(animationPlayer);
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}