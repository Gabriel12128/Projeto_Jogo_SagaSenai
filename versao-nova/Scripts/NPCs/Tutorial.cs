using Godot;
using System;

public partial class Tutorial : Node3D
{
    
    [Export] private TextureRect[] diagramas;
    
    private int _indiceAtual = 0;

    public override void _Ready()
    {
       
        EsconderTodos();
        
        if (diagramas.Length > 0)
        {
            diagramas[0].Visible = true;
        }
        else
        {
            GD.PrintErr("Erro: Nenhum diagrama foi atribuído ao array no Inspetor!");
        }
    }

    public override void _Input(InputEvent @event)
    {
     
        if (@event.IsActionPressed("sairCutscene")) 
        {
            AvancarTutorial();
        }
    }

    private void AvancarTutorial()
    {
     
        if (_indiceAtual < diagramas.Length)
        {
            diagramas[_indiceAtual].Visible = false;
        }

       
        _indiceAtual++;

      
        if (_indiceAtual < diagramas.Length)
        {
            diagramas[_indiceAtual].Visible = true;
        }
        else
        {
           
            FinalizarTutorial();
        }
    }

    private void EsconderTodos()
    {
        foreach (var diagrama in diagramas)
        {
            if (diagrama != null)
                diagrama.Visible = false;
        }
    }

    private void FinalizarTutorial()
    {
        
        foreach (var diagrama in diagramas)
		{
			if (diagrama != null)
				diagrama.Visible = false;
		}
    }
}