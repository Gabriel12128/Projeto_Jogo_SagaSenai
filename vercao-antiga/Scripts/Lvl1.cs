using Godot;
using System;
using PlayerC;

public partial class Lvl1 : Node2D
{
	
	[Export] private Player player;

	
	public void _on_win_body_entered(Node2D body)
	{
		if(body.IsInGroup("Player"))
		{
			player.win();
		}
	}

	
}
