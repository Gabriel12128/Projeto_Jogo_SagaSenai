using Godot;
using System;

public partial class BarraDePureza : TextureProgressBar
{
	public int quantidade = 0;
	public override void _Ready()
	{
		Value = 0;
	}

	public override void _Process(double delta)
	{
		Value = quantidade;
	}


}
