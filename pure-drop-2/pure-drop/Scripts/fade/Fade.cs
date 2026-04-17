using Godot;
using System.Threading.Tasks;

public partial class Fade : CanvasLayer
{
    [Export] private AnimationPlayer _animPlayer;

    public static Fade Instance { get; private set; }

    public override void _Ready()
    {
        
        Instance = this;
        
      
        Visible = false;
        
      
        var fundo = GetNodeOrNull<Control>("ColorRect");
        if (fundo != null) fundo.MouseFilter = Control.MouseFilterEnum.Ignore;
    }

    public async Task MudarCena(PackedScene cena)
    {
        if (cena == null) return;

        Visible = true; 
        _animPlayer.Play("fade_in");
        await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);

        GetTree().ChangeSceneToPacked(cena);

     
        await ToSignal(GetTree().CreateTimer(0.1f, true), "timeout");

        _animPlayer.Play("fade_out");
        await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);
        
        Visible = false; 
    }

    public async Task MudarCena(string cena)
    {
        if (cena == null) return;

        Visible = true; 
        _animPlayer.Play("fade_in");
        await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);

        GetTree().ChangeSceneToFile(cena);

     
        await ToSignal(GetTree().CreateTimer(0.1f, true), "timeout");

        _animPlayer.Play("fade_out");
        await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);
        
        Visible = false; 
    }

     public async Task ResetarCena()
    {

        Visible = true; 
        _animPlayer.Play("fade_in");
        await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);

        GetTree().ReloadCurrentScene();

     
        await ToSignal(GetTree().CreateTimer(0.1f, true), "timeout");

        _animPlayer.Play("fade_out");
        await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);
        
        Visible = false; 
    }
}