using Godot;
using System;

public partial class MenuPrincipal : Control
{
    [Export] private TextureButton play;
    [Export] private TextureButton creditos;
    [Export] private TextureButton sair;

    [Export] private AudioStreamPlayer2D somDeclick;
    [Export] private PackedScene intro;
    [Export] private PackedScene cenaCreditos;
    
    private Node _globalUi;
    public override void _Ready()
    {
        play.Pressed += OnPlayPressed;
        creditos.Pressed += OnCreditosPressed;
        sair.Pressed += OnSairPressed;
        
    }

    private void OnPlayPressed()
    {
        somDeclick.Play();
        _ = Fade.Instance.MudarCena(intro);
        
    }

    private void OnCreditosPressed()
    {
        somDeclick.Play();
         _ = Fade.Instance.MudarCena(cenaCreditos);
    }

    private void OnSairPressed()
    {
        somDeclick.Play();
        GetTree().Quit();
    }

    




}
