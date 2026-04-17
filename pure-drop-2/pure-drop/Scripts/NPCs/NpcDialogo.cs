using Godot;
using System;

public partial class NpcDialogo : Node3D
{
    [Export] private Label falasLabel;
    [Export] private Label nomeLabel;
    [Export] private Sprite3D icone;
    [Export] private CanvasLayer painelDialogo; 

    [Export] private string nomeNpc = "Anancônomo";
    [Export] private string[] falas = new string[] {
        "Olá, viajante!",
        "O dia está bonito, não acha?",
        "Tome cuidado com os inimigos explosivos lá fora!"
    };

    [Export] private Area3D area;

    private int index = 0;
    private bool jogadorPerto = false;

    public override void _Ready()
    {
       
        if (painelDialogo != null) 
            painelDialogo.Visible = false;

        if (painelDialogo != null) 
            painelDialogo.Visible = false;


        if (nomeLabel != null) 
            nomeLabel.Text = nomeNpc;

        if (icone != null) 
            icone.Visible = false;
            
        area.AreaEntered += OnAreaEntered;
        area.AreaExited += OnAreaExited;
    }

    public override void _Input(InputEvent @event)
    {
        if (jogadorPerto && @event.IsActionPressed("interacao"))
        {
            GerenciarConversa();
        }
    }

    private void GerenciarConversa()
    {
        if (falas == null || falas.Length == 0) return;

      
        if (painelDialogo != null && !painelDialogo.Visible)
        {
            painelDialogo.Visible = true;
            index = 0; 
        }

        if (index < falas.Length)
        {
            falasLabel.Text = falas[index];
            index++;
        }
        else
        {
            FinalizarConversa();
        }
    }

    private void FinalizarConversa()
    {
        index = 0; 
        if (painelDialogo != null) 
            painelDialogo.Visible = false;
        
        if (falasLabel != null)
            falasLabel.Text = "";
    }

    private void OnAreaEntered(Area3D area)
    {
    
        if (area.IsInGroup("player") || area.GetParent().IsInGroup("player"))
        {
            jogadorPerto = true;

            if (icone != null) 
                icone.Visible = true;
        }
    }

    private void OnAreaExited(Area3D area)
    {
        if (area.IsInGroup("player") || area.GetParent().IsInGroup("player"))
        {
            jogadorPerto = false;

            if (icone != null) 
                icone.Visible = false;

            FinalizarConversa(); 
        }
    }
}