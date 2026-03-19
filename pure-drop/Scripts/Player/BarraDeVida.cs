using Godot;
using System;

public partial class BarraDeVida : TextureProgressBar
{
	private Node3D player;
	public override void _Ready()
	{
		player = GetTree().GetFirstNodeInGroup("player") as Node3D;
	}

	
	public override void _Process(double delta)
	{
		if(player is Player p)
		{
			MaxValue = p.vidaMax;
			Value = p.vida;
		}
	}
}
