using Godot;
using System;

public partial class Sombra : Sprite3D
{
	  [Export] private Node3D player;

    private float groundY;

    public override void _Ready()
    {
        groundY = GlobalPosition.Y;
    }

    public override void _Process(double delta)
    {
        Vector3 pos = GlobalPosition;

       
        pos.X = player.GlobalPosition.X;
        pos.Z = player.GlobalPosition.Z;
        pos.Y = groundY;

        GlobalPosition = pos;

       
        float altura = player.GlobalPosition.Y - groundY;

     
        float escalaBase = 1.5f;
        float escala = Mathf.Clamp(escalaBase - altura * 0.1f, 0.4f, escalaBase);
        Scale = new Vector3(escala, escala, 1);

      
        float alpha = Mathf.Clamp(1 - altura * 0.2f, 0.3f, 1f);

        Modulate = new Color(1, 1, 1, alpha);
    }
}
