using Godot;
using System;


public partial class Hud : CanvasLayer
{
	
	[Export] CharacterBody2D player;

	[Export] private Sprite2D[] sprites;

	[Export] private Sprite2D[] gotas;

	public static Hud instance;


	public override void _Ready()
	{
		instance = this;
	}

	
	public void UpdateLife(int life)
	{
		if(life == 75)
		{
			sprites[0].Visible = false;
		}
		else if(life == 50)
		{
			sprites[1].Visible = false;
		}
		else if (life == 25)
		{
			sprites[2].Visible = false;
		}
		else if (life <= 0)
		{
			sprites[3].Visible = false;
		}
	}

	

	public void UpdateGotas(float g)
	{

		if (g == 1f)
		{
			this.gotas[0].Visible = false;
		}
		else if (g == 2f)
		{
			this.gotas[1].Visible = false;
		}
		else if (g == 3f)
		{
			this.gotas[2].Visible = false;
		}
		else if (g == 4f)
		{
			this.gotas[3].Visible = false;
		}
		else if (g == 5f)
		{
			this.gotas[4].Visible = false;
		}
		else if (g == 6f)
		{
			this.gotas[5].Visible = false;
		}

	}

}
