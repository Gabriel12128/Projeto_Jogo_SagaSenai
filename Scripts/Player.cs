using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private const float Speed = 300.0f;
	
	enum PlayerState
	{
		idle,
		walk,
		jump,
		atack
	}

	private PlayerState state;

	public override void _Ready()
	{
		Go_To_Idle();
		
	}

	public override void _PhysicsProcess(double delta)
	{
		switch (state)
		{
			case PlayerState.idle:
				Idle();
				break;

			case PlayerState.walk:
				Walk();
				break;

			default:
				break;
		}
	}

	private void Go_To_Idle()
	{
		state = PlayerState.idle;
		
	}

	private void Go_To_Walk()
	{
		state = PlayerState.walk;
		GD.Print("Walk");
	}

	private void Walk()
	{
		Vector2 input = InputMap(); 

		if(input == Vector2.Zero)
		{
			Go_To_Idle();
			return;
		}

		Velocity = input * Speed;
		MoveAndSlide();
	}

	private void Idle()
	{
		Vector2 input = InputMap();
		if(input != Vector2.Zero)
		{
			Go_To_Walk();
			return;
		}
	}

	private Vector2 InputMap()
	{
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		return direction.Normalized();
	}
}
