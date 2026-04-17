using Godot;
using System;

public partial class PagandoGota : Node3D
{
    [Export] private VideoStreamPlayer creditos;
    [Export] private PackedScene MissaoCumprida;

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
            _ = Fade.Instance.MudarCena(MissaoCumprida);
            creditos.Stop();
        }
    }

    private void OnFinished()
    {
        interagirLabel.Visible = true;
        isFinished = true;
    }
}
