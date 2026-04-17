using Godot;
using System;
using System.Threading.Tasks;

public partial class CombateArea : Node3D
{
    [Export] private PackedScene[] enemyScenes;
    [Export] private PackedScene lifeScene;
    [Export] private Sprite2D goSprite;
    [Export] private int totalEnemies = 6;
    [Export] private float tempoParaTravar = 2.0f;

    [Export] private StaticBody3D leftWall;
    [Export] private StaticBody3D rightWall;
    [Export] private Marker3D spawnLeft;
    [Export] private Marker3D spawnRight;
    [Export] private Marker3D spawnLife;
    [Export] private Node3D cameraNode;

    private CameraPlayer cameraScript;
    private int currentEnemiesAlive = 0;
    private int remainingToSpawn;
    private bool arenaActive = false;
    private Random random = new Random();

    public override void _Ready()
    {
        remainingToSpawn = totalEnemies;
        SetWallsEnabled(false);

        if (goSprite != null)
            goSprite.Visible = false;

        if (cameraNode != null)
            cameraScript = cameraNode as CameraPlayer;

        var trigger = GetNodeOrNull<Area3D>("TriggerArea");
        if (trigger != null)
        {
            trigger.BodyEntered += (body) => {
                if (body.IsInGroup("player") && !arenaActive && remainingToSpawn > 0) 
                    RotinaDeInicio();
            };
        }
    }

    private async void RotinaDeInicio()
    {
        arenaActive = true;
        HelpPlayer();

        await ToSignal(GetTree().CreateTimer(tempoParaTravar), "timeout");
        
        if (!IsInstanceValid(this) || leftWall == null || rightWall == null) return;

        float centroX = (leftWall.GlobalPosition.X + rightWall.GlobalPosition.X) / 2.0f;
        SetWallsEnabled(true);

        if (cameraScript != null && IsInstanceValid(cameraScript)) 
            cameraScript.SetLock(true, centroX);

        CheckNextWave();
    }

    private async void CheckNextWave()
    {
        if (!IsInstanceValid(this) || !IsInsideTree()) return;

        if (currentEnemiesAlive == 0 && remainingToSpawn > 0)
        {
            var tree = GetTree();
            if (tree == null) return;

            await ToSignal(tree.CreateTimer(0.5f), "timeout");

            if (!IsInstanceValid(this) || !IsInsideTree()) return;

            Spawn(spawnLeft);
            remainingToSpawn--;

            if (remainingToSpawn > 0)
            {
                Spawn(spawnRight);
                remainingToSpawn--;
            }
        }
        else if (currentEnemiesAlive <= 0 && remainingToSpawn <= 0)
        {
            EndArena();
        }
    }

    private void Spawn(Marker3D point)
    {
        if (point == null || enemyScenes == null || enemyScenes.Length == 0 || !IsInstanceValid(this)) return;

        int index = random.Next(enemyScenes.Length);
        if (enemyScenes[index] == null) return;

        var enemy = enemyScenes[index].Instantiate<Node3D>();
        GetParent().AddChild(enemy);
        enemy.GlobalPosition = point.GlobalPosition;
        
        currentEnemiesAlive++;
        enemy.TreeExited += () => {
            if (IsInstanceValid(this))
            {
                currentEnemiesAlive--;
                CheckNextWave();
            }
        };
    }

    private void SpawnLife(Marker3D point)
    {
        if (point == null || lifeScene == null || !IsInstanceValid(this)) return;
        var life = lifeScene.Instantiate<Node3D>();
        GetParent().AddChild(life);
        life.GlobalPosition = point.GlobalPosition;
    }

    private async void HelpPlayer()
    {
        while(arenaActive && IsInstanceValid(this))
        {
            int time = random.Next(10, 20);
            await ToSignal(GetTree().CreateTimer(time), "timeout");

            if (arenaActive && IsInstanceValid(this))
            {
                SpawnLife(spawnLife);
            }
        }
    }

    private void EndArena()
    {
        if (!IsInstanceValid(this)) return;
        
        arenaActive = false;
        SetWallsEnabled(false);
        
        if (cameraScript != null && IsInstanceValid(cameraScript)) 
            cameraScript.SetLock(false);
            
        if (goSprite != null)
            goSprite.Visible = true;

        HideGoSprite();
    }

    private async void HideGoSprite()
    {
        if (!IsInstanceValid(this) || !IsInsideTree()) return;

        SceneTree tree = GetTree();
        if (tree == null) return;

        await ToSignal(tree.CreateTimer(4.0f), "timeout");

        if (!IsInstanceValid(this) || !IsInsideTree() || goSprite == null) return;

        goSprite.Visible = false;
    }

    private void SetWallsEnabled(bool enabled)
    {
        if (leftWall == null || rightWall == null) return;

        var shapeL = leftWall.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
        var shapeR = rightWall.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
        
        if (shapeL != null) shapeL.Disabled = !enabled;
        if (shapeR != null) shapeR.Disabled = !enabled;
    }
}