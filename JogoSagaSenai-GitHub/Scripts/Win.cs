using Godot;
using System;

public partial class Win : CanvasLayer
{
	[Export] private Label winText;
	[Export] private Button restartButton;
	[Export] private Button mainMenu;
	[Export] private ColorRect fundo;

	[Export] private string winScenePath = "res://Scenes/Game.tscn";

	public static Win instance;

	public override void _Ready()
	{
		instance = this;

		winText.Visible = false;
		restartButton.Visible = false;
		fundo.Visible = false;

		restartButton.Pressed += OnRetryPressed;
		mainMenu.Pressed += OnMainMenuPressed;
	}

	public void ShowWin()
	{
		winText.Visible = true;
		restartButton.Visible = true;
		fundo.Visible = true;
	}

	private void OnRetryPressed()
	{
		GetTree().ReloadCurrentScene();
	}

	private void OnMainMenuPressed()
	{
		GetTree().ChangeSceneToFile(winScenePath);
	}
}
