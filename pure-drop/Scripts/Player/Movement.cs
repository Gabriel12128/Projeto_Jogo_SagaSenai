using Godot;

public class Movement
{
    
    private float speed = 220f;
    private float jumpSpeed = 5f;

    
    public void ApplyGravity(ref Vector3 velocity, double delta, CharacterBody3D player)
    {
        if (!player.IsOnFloor())
        {
            velocity += player.GetGravity() * (float)delta;
        }
    }

    public void HandleJump(ref Vector3 velocity, CharacterBody3D player, bool atacando)
    {
        

        if (atacando) return;

        if (Input.IsActionJustPressed("jump") && player.IsOnFloor())
        {
            velocity.Y = jumpSpeed;
            
        }
       
        
    }

    public void HandleMovement(ref Vector3 velocity, AnimationPlayer animationPlayer, Sprite3D sprite, CharacterBody3D player, bool atacando, double delta)
    {
        if (atacando)
        {
            velocity.X = 0;
            velocity.Z = 0;
            return;
        }

        Vector2 inputDirection = Input.GetVector("esquerda", "direita", "cima", "baixo");

        Vector3 direction = (player.Transform.Basis * new Vector3(inputDirection.X, 0, inputDirection.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * speed * (float)delta;
            velocity.Z = direction.Z * speed * (float)delta  *2;
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, speed * (float)delta);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, speed * (float)delta);
        }

        if (!player.IsOnFloor())
        {
            if (animationPlayer.CurrentAnimation != "Jump")
                animationPlayer.Play("Jump");
        }
        else if (direction != Vector3.Zero)
        {
            if (animationPlayer.CurrentAnimation != "Walk")
                animationPlayer.Play("Walk");
        }
        else
        {
            if (animationPlayer.CurrentAnimation != "Idle")
                animationPlayer.Play("Idle");
        }

        HandleSpriteFlip(direction, sprite);
    }

    private void HandleSpriteFlip(Vector3 direction, Sprite3D sprite)
    {
        if (direction.X < 0)
            sprite.FlipH = true;
        else if (direction.X > 0)
            sprite.FlipH = false;
    }
}