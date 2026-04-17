using Godot;
using System;

public partial class Game : Node2D
{
	
	[Export] private Button playButton;
    [Export] private Button exitButton;

    [Export] private string gameScenePath = "res://Scenes/Game.tscn";

    public override void _Ready()
    {
        playButton.Pressed += OnPlayPressed;
        exitButton.Pressed += OnExitPressed;
    }

    private void OnPlayPressed()
    {
        GetTree().ChangeSceneToFile(gameScenePath);
    }

    private void OnExitPressed()
    {
        GetTree().Quit();
    }
}
