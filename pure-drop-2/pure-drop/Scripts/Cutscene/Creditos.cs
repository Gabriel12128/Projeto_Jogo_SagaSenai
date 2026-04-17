using Godot;
using System;

public partial class Creditos : Node3D
{
	
	[Export] private VideoStreamPlayer creditos;
    [Export] private string cenaMenu;
	[Export] private Label sairLabel;
	private bool terminou = false;

    public override void _Ready()
    {
        creditos.Play();
		creditos.Finished += OnCreditosFinished;
		sairLabel.Visible = false;
    }


    public override void _Input(InputEvent @event)
    {
        
        if (@event.IsActionPressed("sairCutscene") && terminou) 
        {
            _ = Fade.Instance.MudarCena(cenaMenu);
            creditos.Stop();
        }
    }

	public void OnCreditosFinished()
	{
		sairLabel.Visible = true;
		terminou = true;
	}
}
