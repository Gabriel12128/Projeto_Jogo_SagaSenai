using Godot;
using System;
using System.Threading.Tasks;

public partial class BossArea : Node3D
{
    [ExportGroup("Configurações")]
    [Export] private PackedScene bossScene;
    [Export] private Marker3D spawnBossPoint;
    [Export] private PackedScene lifeScene;
    [Export] private Sprite2D goSprite;
    [Export] private float tempoParaTravar = 2.0f;

    [ExportGroup("Nós da Arena")]
    [Export] private StaticBody3D leftWall;
    [Export] private StaticBody3D rightWall;
    [Export] private Marker3D spawnLife;
    [Export] private Node3D cameraNode;

    private CameraPlayer cameraScript;
    private Node3D bossInstancia;
    private bool arenaActive = false;
    private Random random = new Random();

    public override void _Ready()
    {
        
        SetWallsEnabled(false);

        if (goSprite != null)
            goSprite.Visible = false;

        if (IsInstanceValid(cameraNode))
            cameraScript = cameraNode as CameraPlayer;

        var trigger = GetNodeOrNull<Area3D>("TriggerArea");
        if (IsInstanceValid(trigger))
        {
            trigger.BodyEntered += OnBodyEntered;
        }
    }

    private void OnBodyEntered(Node3D body)
    {
        
        if (body.IsInGroup("player") && !arenaActive)
        {
            _ = RotinaDeInicio();
        }
    }

    private async Task RotinaDeInicio()
    {
        arenaActive = true;
        
        SpawnBoss();
        _ = HelpPlayer(); 

    
        await ToSignal(GetTree().CreateTimer(tempoParaTravar), "timeout");
        if (!IsInstanceValid(this)) return;

        if (IsInstanceValid(leftWall) && IsInstanceValid(rightWall))
        {
            float centroX = (leftWall.GlobalPosition.X + rightWall.GlobalPosition.X) / 2.0f;
            SetWallsEnabled(true);

            if (IsInstanceValid(cameraScript))
                cameraScript.SetLock(true, centroX);
        }
    }

    private void SpawnBoss()
    {
        if (bossScene == null || spawnBossPoint == null) return;

        bossInstancia = bossScene.Instantiate<Node3D>();
        
       
        GetParent().AddChild(bossInstancia); 
        
        bossInstancia.Scale = new Vector3(2f, 2f, 2f);
        bossInstancia.GlobalPosition = spawnBossPoint.GlobalPosition;

        
        bossInstancia.TreeExited += OnBossDefeated;
    }

    private void OnBossDefeated()
    {
        
        if (!IsInstanceValid(this)) return;
        EndArena();
    }

    private async Task HelpPlayer()
    {
        
        while(arenaActive && IsInstanceValid(this))
        {
            int time = random.Next(15, 25);
            await ToSignal(GetTree().CreateTimer(time), "timeout");

            if (arenaActive && IsInstanceValid(this) && IsInstanceValid(spawnLife))
            {
                SpawnLife(spawnLife);
            }
        }
    }

    private void SpawnLife(Marker3D point)
    {
        if (lifeScene == null || !IsInstanceValid(this)) return;
        
        var life = lifeScene.Instantiate<Node3D>();
        GetParent().AddChild(life);
        life.GlobalPosition = point.GlobalPosition;
    }

    private void EndArena()
    {
        arenaActive = false;
        SetWallsEnabled(false);
        
        if (IsInstanceValid(cameraScript)) 
            cameraScript.SetLock(false);
            
        if (IsInstanceValid(goSprite))
            goSprite.Visible = true;

        _ = HideGoSprite();
    }

    private async Task HideGoSprite()
    {
        
        if (!IsInstanceValid(this) || !IsInsideTree()) return;

        
        SceneTree tree = GetTree();

        
        await ToSignal(tree.CreateTimer(5.0f), "timeout");
        
       
        if (IsInstanceValid(this) && IsInsideTree() && IsInstanceValid(goSprite))
        {
            goSprite.Visible = false;
        }
    }
    private void SetWallsEnabled(bool enabled)
    {

        if (IsInstanceValid(leftWall))
        {
            var shapeL = leftWall.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
            if (IsInstanceValid(shapeL)) shapeL.SetDeferred("disabled", !enabled);
        }

        if (IsInstanceValid(rightWall))
        {
            var shapeR = rightWall.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
            if (IsInstanceValid(shapeR)) shapeR.SetDeferred("disabled", !enabled);
        }
    }
}