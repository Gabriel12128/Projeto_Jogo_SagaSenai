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
		gameOverText.Visible = false;
		restartButton.Visible = false;
		fundo.Visible = false;

		instance = this;
	}


	public void ShowGameOver()
	{
        gameOverText.Visible = true;
        restartButton.Visible = true;
        fundo.Visible = true;
    }

	public void _on_retry_pressed()
	{
		GD.Print("apertou");
        GetTree().ReloadCurrentScene();
    }
}
