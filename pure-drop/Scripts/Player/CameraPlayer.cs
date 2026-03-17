using Godot;
using System;

public partial class CameraPlayer : Camera3D
{
	
	public override void _Ready()
	{
	}

	[Export] private Node3D player;

	[Export] private float minX = -7.5f;
	[Export] private float maxX = 7.5f;

	public override void _Process(double delta)
	{
		Vector3 pos = GlobalPosition;

		float targetX = player.GlobalPosition.X;

		pos.X = Mathf.Clamp(targetX, minX, maxX);

		
	   GlobalPosition = pos;
	}
}
