using Godot;
using System;

public partial class GameOver : CanvasLayer
{ 
	[Export] private Label gameOverText;
	[Export] private Button restartButton;
	[Export] private ColorRect fundo;

	public static GameOver instance;

	public override void _Ready()
	{
		instance = this;

		gameOverText.Visible = false;
		restartButton.Visible = false;
		fundo.Visible = false;

		restartButton.Pressed += OnRetryPressed;
	}

	public void ShowGameOver()
	{
		gameOverText.Visible = true;
		restartButton.Visible = true;
		fundo.Visible = true;
	}

	private void OnRetryPressed()
	{
		GetTree().ReloadCurrentScene();
	}
}