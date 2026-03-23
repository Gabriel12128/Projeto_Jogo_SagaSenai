using Godot;
using System;
using System.Threading.Tasks;

public partial class CombateArea : Node3D
{
 [Export] private PackedScene enemyScene;
    [Export] private int totalEnemies = 6;
    [Export] private float tempoParaTravar = 2.0f; // Tempo para o player entrar na arena

    [Export] private StaticBody3D leftWall;
    [Export] private StaticBody3D rightWall;
    [Export] private Marker3D spawnLeft;
    [Export] private Marker3D spawnRight;
    [Export] private Node3D cameraNode;

    private CameraPlayer cameraScript;
    private int currentEnemiesAlive = 0;
    private int remainingToSpawn;
    private bool arenaActive = false;

    public override void _Ready()
    {
        remainingToSpawn = totalEnemies;
        SetWallsEnabled(false);

        if (cameraNode != null)
            cameraScript = cameraNode as CameraPlayer;

        var trigger = GetNode<Area3D>("TriggerArea");
        trigger.BodyEntered += (body) => {
            if (body.IsInGroup("player") && !arenaActive && remainingToSpawn > 0) 
                RotinaDeInicio();
        };
    }

    private async void RotinaDeInicio()
    {
        arenaActive = true;

        // Espera o tempo para o player caminhar até o meio
        await ToSignal(GetTree().CreateTimer(tempoParaTravar), "timeout");

        // Calcula o centro exato entre as paredes
        float centroX = (leftWall.GlobalPosition.X + rightWall.GlobalPosition.X) / 2.0f;

        // Ativa paredes (que só bloqueiam o player) e trava a câmera
        SetWallsEnabled(true);
        if (cameraScript != null) 
            cameraScript.SetLock(true, centroX);

        CheckNextWave();
    }

    private async void CheckNextWave()
    {
        if (currentEnemiesAlive == 0 && remainingToSpawn >= 2)
        {
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
            Spawn(spawnLeft);
            Spawn(spawnRight);
            remainingToSpawn -= 2;
        }
        else if (currentEnemiesAlive == 0 && remainingToSpawn <= 0)
        {
            EndArena();
        }
    }

    private void Spawn(Marker3D point)
    {
        if (enemyScene == null) return;
        var enemy = enemyScene.Instantiate<Enemy1>();
        GetParent().AddChild(enemy);
        enemy.GlobalPosition = point.GlobalPosition;
        
        currentEnemiesAlive++;
        enemy.TreeExited += () => {
            currentEnemiesAlive--;
            CheckNextWave();
        };
    }

    // ... (resto do código anterior)

    private void EndArena()
    {
        arenaActive = false;
        SetWallsEnabled(false);
        
        // Quando chamamos SetLock(false), a câmera no script acima 
        // começará a fazer o Lerp de volta para a posição do player automaticamente.
        if (cameraScript != null) 
            cameraScript.SetLock(false);
            
        GD.Print("GO! GO! GO!");
        
        // Opcional: Você pode instanciar uma setinha piscando aqui
    }

// ... (resto do código anterior)

    private void SetWallsEnabled(bool enabled)
    {
        // Certifique-se que o nome do nó filho é "CollisionShape3D"
        var shapeL = leftWall.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
        var shapeR = rightWall.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
        
        if (shapeL != null) shapeL.Disabled = !enabled;
        if (shapeR != null) shapeR.Disabled = !enabled;
    }
}