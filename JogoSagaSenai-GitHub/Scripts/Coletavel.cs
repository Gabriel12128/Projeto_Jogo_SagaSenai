using Godot;
using System;
using PlayerC;

public partial class Coletavel : Area2D
{
    public static Coletavel instance;

    [Export] private Sprite2D spr;

    [Export] private float floatHeight = 6f; 
    [Export] private float floatSpeed = 2f;  

    private float time = 0f;
    private Vector2 startPosition;

    public override void _Ready()
    {
        startPosition = Position;
        instance = this;
    }

    public override void _Process(double delta)
    {
        time += (float)delta;
        FloatAnimation();
    }

    private void FloatAnimation()
    {
        
        float offsetY = Mathf.Sin(time * floatSpeed) * floatHeight;
        Position = startPosition + new Vector2(0, offsetY);
    }

    private void _on_body_entered(Node2D body)
    {
        if (body.IsInGroup("Player"))
        {

            Player player = body as Player;

            if (player != null)
            {
                player.UpdateGota(0.5f);
            }

            QueueFree();

        }

    }
}