using Godot;
using System;


public partial class Hud : Control
{
	
	[Export] CharacterBody2D player;
	[Export] Label lifeText;

	public static Hud instance;


	public override void _Ready()
	{
		instance = this;
	}

	
	public override void _Process(double delta)
	{
	}

	public void UpdateLifeText(int life)
	{
		lifeText.Text = life.ToString();
	}
}
