using Godot;
using PlayerC;

public partial class Coletavel : Area2D
{
    [Export] private float floatHeight = 6f;
    [Export] private float floatSpeed = 2f;

    private float time = 0f;
    private Vector2 startPosition;
    private bool initialized = false;

    public override void _Ready()
    {
        CallDeferred(nameof(InitPosition));
    }

    private void InitPosition()
    {
        startPosition = GlobalPosition;
        initialized = true;
    }

    public override void _Process(double delta)
    {
        if (!initialized)
            return;

        time += (float)delta;

        float offsetY = Mathf.Sin(time * floatSpeed) * floatHeight;
        GlobalPosition = startPosition + new Vector2(0, offsetY);
    }

    private void _on_body_entered(Node2D body)
    {
        if (!body.IsInGroup("Player"))
            return;

        Player player = body as Player;

        if (player != null)
            player.UpdateGota(0.5f);

        QueueFree();
    }
}