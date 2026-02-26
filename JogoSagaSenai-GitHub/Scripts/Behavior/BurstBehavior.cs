using Godot;
using PlayerC;
using EnemyC;
using Interfaces;

public partial class BurstBehavior : Node, IEnemyBehavior
{
	[Export] private CpuParticles2D ExplodePar;

	private float explodeTime = 0f;
	private bool charging = false;
	private bool exploded = false;
	
	

	
	private const float explodeDelay = 0.6f;

	private const float stopDistance = 40f;

	private Vector2 normalScale = Vector2.One;
	private Vector2 growScale = new Vector2(1.35f, 1.35f);
	private float growSpeed = 6f;

	public override void _Ready()
	{
		if (ExplodePar != null)
			ExplodePar.Emitting = false;
	}

	public void Execute(Player player, Enemy enemy, double delta)
	{
		if (exploded)
			return;


		if (player == null || !GodotObject.IsInstanceValid(player))
    	{
        	enemy.Velocity = Vector2.Zero;
        	return;
    	}

		float d = (float)delta;

		
		if (enemy.Velocity.Length() > enemy.speed + 10f)
		{
			charging = false;
			explodeTime = 0f;
			enemy.Scale = enemy.Scale.Lerp(normalScale, growSpeed * d);
			return;
		}

		Vector2 toPlayer = player.GlobalPosition - enemy.GlobalPosition;
		Vector2 dir = toPlayer.Normalized();
		float distance = toPlayer.Length();

		
		if (!charging)
		{
			if (distance > stopDistance && distance < 300f)
				enemy.Velocity = dir * enemy.speed;
			else
				enemy.Velocity = Vector2.Zero;

			enemy.MoveAndSlide();
		}

		
		if (distance <= stopDistance)
			charging = true;
		else
		{
			charging = false;
			explodeTime = 0f;
		}

		
		if (charging)
		{
			enemy.Scale = enemy.Scale.Lerp(growScale, growSpeed * d);

			explodeTime += d;
			enemy.Velocity = enemy.Velocity.Lerp(Vector2.Zero, 12f * d);

			if (explodeTime >= explodeDelay)
			{
				
				Explode(enemy, player);
			}	
		}
		else
		{
			enemy.Scale = enemy.Scale.Lerp(normalScale, growSpeed * d);
		}
	}

	private void Explode(Enemy enemy, Player player)
	{
		exploded = true;
		
		player.TakeDamage(enemy.damage, enemy.GlobalPosition);

		if (ExplodePar != null)
		{
			ExplodePar.Reparent(enemy.GetTree().CurrentScene);
			ExplodePar.GlobalPosition = enemy.GlobalPosition;
			ExplodePar.Emitting = true;
		}

		if (enemy.animt != null)
			enemy.animt.Visible = false;

		enemy.QueueFree();
	}
}