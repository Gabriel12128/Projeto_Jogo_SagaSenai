using Godot;
using System;

public partial class Cutscene1 : Node3D
{
    
    [Export] private VideoStreamPlayer intro;
    [Export] private PackedScene cenaTutorial;

    [Export] private Label interagirLabel;

    private bool isFinished = false;

    public override void _Ready()
    {
        intro.Play();
        intro.Finished += OnFinished;
        interagirLabel.Visible = false;
    }


    public override void _Input(InputEvent @event)
    {
        
        if (@event.IsActionPressed("sairCutscene") && isFinished) 
        {
            _ = Fade.Instance.MudarCena(cenaTutorial);
            intro.Stop();
        }
    }

    private void OnFinished()
    {
        interagirLabel.Visible = true;
        isFinished = true;
    }
   

}
