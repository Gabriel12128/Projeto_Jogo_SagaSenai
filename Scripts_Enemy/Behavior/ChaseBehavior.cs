using Godot;
using System;
using Interfaces;
using PlayerC;
using EnemyC;

public partial class ChaseBehavior : Node , IEnemyBehavior
{
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
	}

	
}

