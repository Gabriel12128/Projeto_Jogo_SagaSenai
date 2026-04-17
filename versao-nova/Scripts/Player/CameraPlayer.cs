using Godot;
using System;

public partial class CameraPlayer : Camera3D
{
    [Export] private Node3D player;
    [Export] private float minX = -7.5f;
    [Export] private float maxX = 7.5f;
    
   
    [Export] private float suavidadeTransicao = 4.0f; 

    private bool isLocked = false;
    private float lockedX = 0f;

    public void SetLock(bool shouldLock, float targetX = 0)
    {
        isLocked = shouldLock;
        if (shouldLock)
        {
            lockedX = targetX;
        }
    }

    public override void _Process(double delta)
    {
        if (player == null) return;

        Vector3 pos = GlobalPosition;
        float targetX;

        if (isLocked)
        {
          
            targetX = lockedX;
        }
        else
        {
           
            targetX = Mathf.Clamp(player.GlobalPosition.X, minX, maxX);
        }

       
        pos.X = Mathf.Lerp(pos.X, targetX, (float)delta * suavidadeTransicao);
        
        GlobalPosition = pos;
    }
}