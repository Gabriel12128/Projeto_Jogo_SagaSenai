using Godot;
using System;
using Interfaces;
using PlayerC;
using EnemyC;

public partial class ChaseBehavior : Node , IEnemyBehavior
{

	
	[Export] private CollisionShape2D atack;
	private bool charging = false;
	private bool atackT = false;

	private float timeAtack = 0;

	public void Execute(Player player, Enemy enemy, double delta)
	{
		Vector2 dir = (player.GlobalPosition - enemy.GlobalPosition).Normalized();
		float distance = enemy.GlobalPosition.DistanceTo(player.GlobalPosition);

		if(distance < 300)
		{
			enemy.Velocity = dir * enemy.speed;
			enemy.MoveAndSlide();
		}
		else
		{
			enemy.Velocity = Vector2.Zero;
		}


        if (atackT && charging)
		{
			timeAtack += (float)delta;

			if (timeAtack > 1f)
			{
				Atack(player, enemy);
				timeAtack = 0f;
			}
		}
		
        
	}

	private void Atack(Player player, Enemy enemy)
    {
		player.SetLife(player.GetLife() - enemy.damage);
    }


	//ataque
	public void _on_hit_box_atack_body_entered(Node2D body)
    {
        if(body.IsInGroup("Player"))
        {
			atackT = true;
        }
    }

	public void _on_hit_box_atack_body_exited(Node2D body)
    {
        if(body.IsInGroup("Player"))
        {
			atackT = false;
        }
    }

	public void _on_hit_box_check_atack_body_entered(Node body)
    {
        if(body.IsInGroup("Player"))
        {
			charging = true;
        }
    }
	
	public void _on_hit_box_check_atack_body_exited(Node body)
    {
        if(body.IsInGroup("Player"))
        {
			charging = false;
        }
    }
}

