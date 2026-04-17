using Godot;
using System;
using Intefaces;
public partial class Lixo : Area3D, IColetaveis
{
    private BarraDePureza barraPureza;

    [Export] private AudioStreamPlayer2D somDecoleta;
	private Vector3 _velocity;
    private float _gravity = 20.0f;
    private float _bounceFriction = 0.6f;
    private bool _noChao = false;

    
    private float _checkDist = 0.3f; 

    public void IniciarSalto(Vector3 impulso)
    {
        _velocity = impulso;
        _noChao = false;
		barraPureza = GetTree().GetFirstNodeInGroup("barra") as BarraDePureza;
    }

    public override void _Process(double delta)
    {
        if (_noChao) return;

        float fDelta = (float)delta;
        Vector3 pos = GlobalPosition;

        
        _velocity.Y -= _gravity * fDelta;

   
        var spaceState = GetWorld3D().DirectSpaceState;

      
        Vector3 destX = pos + new Vector3(_velocity.X * fDelta + (Mathf.Sign(_velocity.X) * _checkDist), 0, 0);
        var queryX = PhysicsRayQueryParameters3D.Create(pos, destX);
        var resultX = spaceState.IntersectRay(queryX);

        if (resultX.Count > 0)
        {
            _velocity.X *= -_bounceFriction; 
        }

        
        Vector3 destZ = pos + new Vector3(0, 0, _velocity.Z * fDelta + (Mathf.Sign(_velocity.Z) * _checkDist));
        var queryZ = PhysicsRayQueryParameters3D.Create(pos, destZ);
        var resultZ = spaceState.IntersectRay(queryZ);

        if (resultZ.Count > 0)
        {
            _velocity.Z *= -_bounceFriction;
        }

       
        Vector3 destY = pos + new Vector3(0, _velocity.Y * fDelta, 0);
        var queryY = PhysicsRayQueryParameters3D.Create(pos, destY - new Vector3(0, _checkDist, 0));
        var resultY = spaceState.IntersectRay(queryY);

        if (resultY.Count > 0)
        {
            
            if (Mathf.Abs(_velocity.Y) > 2.0f)
            {
                _velocity.Y *= -0.5f; 
            }
            else
            {
                
                _noChao = true;
                _velocity = Vector3.Zero;
           
                GlobalPosition = (Vector3)resultY["position"] + new Vector3(0, _checkDist, 0);
                return;
            }
        }

      
        GlobalPosition += _velocity * fDelta;
	}
	public async void Executar(Player player)
    {
        if (barraPureza != null)
        {
            somDecoleta.Play();
            barraPureza.quantidade++;
        }

        await ToSignal(GetTree().CreateTimer(0.3f, true), "timeout");

        QueueFree();
    }
}