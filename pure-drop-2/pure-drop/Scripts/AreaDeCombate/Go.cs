using Godot;
using System;

public partial class Go : Sprite2D
{
	[Export] public float Distancia = 200.0f; 
    [Export] public float Velocidade = 2.0f;  

    private float _tempoAcumulado = 0.0f;
    private float _posicaoInicialX;

    public override void _Ready()
    {
       
        _posicaoInicialX = Position.X;
    }

    public override void _Process(double delta)
    {
        _tempoAcumulado += (float)delta * Velocidade;

        
        float deslocamento = (float)Math.Sin(_tempoAcumulado) * Distancia;
        
       
        Position = new Vector2(_posicaoInicialX + deslocamento, Position.Y);

    }
}
