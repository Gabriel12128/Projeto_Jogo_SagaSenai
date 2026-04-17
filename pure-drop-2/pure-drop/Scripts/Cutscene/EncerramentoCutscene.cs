using Godot;
using System;

public partial class EncerramentoCutscene : Node3D
{
    [Export] private VideoStreamPlayer creditos;
    [Export] private PackedScene cenaCreditos;

    [Export] private Label interagirLabel;

    private bool isFinished = false;

    public override void _Ready()
    {
        creditos.Play();
        creditos.Finished += OnFinished;
        interagirLabel.Visible = false;
    }


    public override void _Input(InputEvent @event)
    {
        
        if (@event.IsActionPressed("sairCutscene") && isFinished) 
        {
            _ = Fade.Instance.MudarCena(cenaCreditos);
            creditos.Stop();
        }
    }

    private void OnFinished()
    {
        interagirLabel.Visible = true;
        isFinished = true;
    }
   
}
