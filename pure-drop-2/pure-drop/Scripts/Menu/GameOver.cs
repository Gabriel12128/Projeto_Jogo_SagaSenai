using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

public partial class GameOver : CanvasLayer
{
	
	[Export] private AudioStreamPlayer2D somDeclick;
	[Export] private AudioStreamPlayer2D somDefundo;
	[Export] private TextureButton rec;
	[Export] private TextureButton menu;
	[Export] private TextureButton sair;

	[Export] private Label[] labels;
	[Export] private PackedScene menuScene;

	[Export] private PackedScene lvl_1;

	public static GameOver Instance {get; private set;}

	public override void _Ready()
	{
		Visible = false;
		Instance = this;
		
		
		somDefundo.Finished += OnSomDefundoFinished;

	 	rec.Visible = false;
		menu.Visible = false;
		sair.Visible = false;

		rec.Pressed += OnRecPressed;
		menu.Pressed += OnMenuPressed;
		sair.Pressed += OnSairPressed;

		foreach (var label in labels)
		{
			label.Visible = false;
		} 
	}

	private async void OnRecPressed()
	{
		GetTree().Paused = false;
		somDeclick.Play();

		Visible = false;

		await ToSignal(GetTree().CreateTimer(0.6f, true), "timeout");
		
		_ = Fade.Instance.ResetarCena(); 
		
	}

	private void OnMenuPressed()
	{
		somDeclick.Play();
		GetTree().ChangeSceneToPacked(menuScene);
	}

	private void OnSairPressed()
	{
		somDeclick.Play();
		GetTree().Quit();
	}

	public async Task ShowGameOver()
	{
		Visible = true;
		somDefundo.Play();
		await ToSignal(GetTree().CreateTimer(0.3f, true), "timeout");
		
	}

	private void OnSomDefundoFinished()
	{
		rec.Visible = true;
		menu.Visible = true;
		sair.Visible = true;
		foreach (var label in labels)
		{
			label.Visible = true;
		}
	}

}
