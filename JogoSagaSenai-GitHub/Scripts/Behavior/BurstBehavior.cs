using Godot;
using PlayerC;
using EnemyC;
using Interfaces;

public partial class BurstBehavior : Node, IEnemyBehavior
{

	[Export] private CpuParticles2D ExplodePar;

    private float explodeTime = 0f;
    private bool charging = false;

    [Export] private AnimatedSprite2D anim;
    private const float explodeDelay = 0.65f;
    private const float triggerDistance = 25f;


    public override void _Ready()
    {
        anim.Visible = false;
    }


    public void Execute(Player player, Enemy enemy, double delta)
    {

        Vector2 dir = (player.GlobalPosition - enemy.GlobalPosition).Normalized();

        float distance = enemy.GlobalPosition.DistanceTo(player.GlobalPosition);

        if (!charging)
        {
            if (distance < 300)
            {
                enemy.Velocity = dir * enemy.speed;
            }
            else
            {
                enemy.Velocity = Vector2.Zero;
            }

            enemy.MoveAndSlide();
        }

        
        if (distance < triggerDistance)
        {
            charging = true;
        }
        else
        {
            
            charging = false;
            explodeTime = 0f;
        }

       
        if (charging)
        {
            explodeTime += (float)delta;
            enemy.animt.Visible = false;
            
            enemy.Velocity = Vector2.Zero;
			anim.Play("Atack");
			if (anim.Animation == "Atack" && anim.Frame == 4 && explodeTime > 0.5f)
        	{
            	ExplodePar.Emitting = true;
        	}

            if (explodeTime >= explodeDelay)
            {
    			anim.Visible = true;
                Explode(player, enemy);
            }
        }
    }

    private void Explode(Player player, Enemy enemy)
    {
        player.SetLife(player.GetLife() - enemy.damage);
        enemy.QueueFree(); 
    }
}