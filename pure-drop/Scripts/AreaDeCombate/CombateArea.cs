using Godot;
using System;
using System.Threading.Tasks;

public partial class CombateArea : Node3D
{
    [Export] private PackedScene enemyScene;

    [Export] private PackedScene lifeScene;

    [Export] private int totalEnemies = 6;
    [Export] private float tempoParaTravar = 2.0f; // Tempo para o player entrar na arena

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

        if(arenaActive)
        {
            HelpPlayer();
        }
        

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
        // Se não há inimigos e ainda há gente para spawnar
        if (currentEnemiesAlive == 0 && remainingToSpawn > 0)
        {
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

            // Spawna no ponto esquerdo
            Spawn(spawnLeft);
            remainingToSpawn--;

            // Só spawna no direito se ainda houver inimigos restantes
            if (remainingToSpawn > 0)
            {
                Spawn(spawnRight);
                remainingToSpawn--;
            }
        }
        // Se não há inimigos e ninguém mais para spawnar
        else if (currentEnemiesAlive <= 0 && remainingToSpawn <= 0)
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

    private void SpawnLife(Marker3D point)
    {
        if (lifeScene == null) return;
        var life = lifeScene.Instantiate<Vida>();
        GetParent().AddChild(life);
        life.GlobalPosition = point.GlobalPosition;
        
    }

    private int RandomTime()
    {
        Random aleatorio = new Random();
        int time = aleatorio.Next(10, 20);
        return time;
    }

    private async void HelpPlayer()
    {
        

        while(arenaActive)
        {
            
            int time = RandomTime();
            await ToSignal(GetTree().CreateTimer(time), "timeout");

            if (arenaActive)
            {
                SpawnLife(spawnLife);
            }
        }

        
    }

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