using Godot;
using System;

public partial class Lixo : Area3D, IColetaveis
{
    private BarraDePureza barraPureza;

	private Vector3 _velocity;
    private float _gravity = 20.0f;
    private float _bounceFriction = 0.6f; // Quanto de força ele mantém ao rebater (0.6 = 60%)
    private bool _noChao = false;

    // Distância para checar colisão (ajuste conforme o tamanho do seu sprite/item)
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

        // 1. Aplicar Gravidade
        _velocity.Y -= _gravity * fDelta;

        // 2. DETECÇÃO DE PAREDE (Eixo X e Z)
        // Criamos um espaço de consulta para checar colisões simples
        var spaceState = GetWorld3D().DirectSpaceState;

        // Checar horizontalmente (X)
        Vector3 destX = pos + new Vector3(_velocity.X * fDelta + (Mathf.Sign(_velocity.X) * _checkDist), 0, 0);
        var queryX = PhysicsRayQueryParameters3D.Create(pos, destX);
        var resultX = spaceState.IntersectRay(queryX);

        if (resultX.Count > 0)
        {
            _velocity.X *= -_bounceFriction; // Rebate e perde força
        }

        // Checar profundidade (Z)
        Vector3 destZ = pos + new Vector3(0, 0, _velocity.Z * fDelta + (Mathf.Sign(_velocity.Z) * _checkDist));
        var queryZ = PhysicsRayQueryParameters3D.Create(pos, destZ);
        var resultZ = spaceState.IntersectRay(queryZ);

        if (resultZ.Count > 0)
        {
            _velocity.Z *= -_bounceFriction; // Rebate e perde força
        }

        // 3. DETECÇÃO DE CHÃO (Eixo Y)
        Vector3 destY = pos + new Vector3(0, _velocity.Y * fDelta, 0);
        var queryY = PhysicsRayQueryParameters3D.Create(pos, destY - new Vector3(0, _checkDist, 0));
        var resultY = spaceState.IntersectRay(queryY);

        if (resultY.Count > 0)
        {
            // Se bater no chão com força, ele quica
            if (Mathf.Abs(_velocity.Y) > 2.0f)
            {
                _velocity.Y *= -0.5f; 
            }
            else
            {
                // Para no chão
                _noChao = true;
                _velocity = Vector3.Zero;
                // Ajusta a posição exata para o ponto de colisão
                GlobalPosition = (Vector3)resultY["position"] + new Vector3(0, _checkDist, 0);
                return;
            }
        }

        // 4. Aplicar movimento final
        GlobalPosition += _velocity * fDelta;
	}
	 public void Execute(Player player)
    {
        if (barraPureza != null)
        {
            barraPureza.quantidade++;
        }

        QueueFree();
    }
}