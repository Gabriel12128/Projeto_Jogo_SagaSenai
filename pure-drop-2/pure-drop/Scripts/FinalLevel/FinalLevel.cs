using Godot;
using System;
using Intefaces;
public partial class FinalLevel : Area3D, ILevel
{
    [Export] private string nextScenePath;
    public void Executar(Player player)
    {
        _ = Fade.Instance.MudarCena(nextScenePath);
    }
}
