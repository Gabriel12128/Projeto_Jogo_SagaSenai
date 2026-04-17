using Godot;
using System;

public partial class HotBar : Control
{
    [Export] private TextureButton slot1;
    [Export] private TextureButton slot2;
    [Export] private Texture2D iconeNormal;
    [Export] private Texture2D iconeSelecionado;
    
    // Cores para indicar qual está selecionado
    [Export] private Color selectedModulate = new Color(1.5f, 1.5f, 1.5f); // Brilho extra
    [Export] private Color normalModulate = new Color(1, 1, 1);

    private int selectedIndex = 0;

    public override void _Ready()
    {
        
        slot1 ??= GetNode<TextureButton>("slot");
        slot2 ??= GetNode<TextureButton>("slot2");
        GetTree().NodeAdded += OnNodeAdded;
        UpdateVisuals();
    }

    public override void _Input(InputEvent @event)
    {
        
        if (@event.IsActionPressed("tecla_1")) 
            SetSelection(0);
        else if (@event.IsActionPressed("tecla_2"))
            SetSelection(1);
            
       
        /* if (@event.IsActionPressed("ui_left") || @event.IsActionPressed("ui_right"))
        {
            SetSelection(selectedIndex == 0 ? 1 : 0);
        } */
    }

    private void SetSelection(int index)
    {
        selectedIndex = index;
        UpdateVisuals();
        GD.Print($"Habilidade {selectedIndex + 1} pronta para uso!");
    }

    private void UpdateVisuals()
    {
        
        slot1.Modulate = (selectedIndex == 0) ? selectedModulate : normalModulate;
        slot2.Modulate = (selectedIndex == 1) ? selectedModulate : normalModulate;

        
        slot1.Scale = (selectedIndex == 0) ? new Vector2(1.02f, 1.02f) : Vector2.One;
        slot2.Scale = (selectedIndex == 1) ? new Vector2(1.02f, 1.02f) : Vector2.One;

        slot1.TextureNormal = (selectedIndex == 0) ? iconeSelecionado : iconeNormal;
        slot2.TextureNormal = (selectedIndex == 1) ? iconeSelecionado : iconeNormal;
    }

    public int GetSelectedSkill() => selectedIndex;

    
    public void SetCooldown(int slotIndex, bool active)
    {
        TextureButton target = (slotIndex == 0) ? slot1 : slot2;
        target.Disabled = active; 
        target.SelfModulate = active ? new Color(0.3f, 0.3f, 0.3f) : Colors.White;
    }

    private void OnNodeAdded(Node node)
    {
       
        if (node.GetParent() == GetTree().Root && node != this)
        {
            // Se a nova cena principal estiver no grupo "no_global"
            if (node.IsInGroup("no_global"))
            {
                this.Visible = false;
                GD.Print("Global escondido na cena: " + node.Name);
            }
            else
            {
                this.Visible = true;
                GD.Print("Global visível na cena: " + node.Name);
            }
    	}
	}
}