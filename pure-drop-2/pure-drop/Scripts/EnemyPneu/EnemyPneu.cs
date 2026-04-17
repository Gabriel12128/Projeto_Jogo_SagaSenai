using Godot;
using System.Threading.Tasks;

public partial class EnemyPneu : CharacterBody3D
{
    private enum State { Idle, Chase, Prepare, Rolling, Stunned, Hit, Death }
    private State currentState = State.Idle;

    [ExportGroup("Referências")]
    [Export] private AnimationPlayer animationPlayer;
    [Export] private Sprite3D sprite;
    [Export] private AudioStreamPlayer2D somRolagem;
    [Export] private AudioStreamPlayer2D somDano; 

    [ExportGroup("Configurações de Drop")]
    [Export] private PackedScene itemColetavel; 

    [ExportGroup("Configurações de Movimento")]
    [Export] private float speedChase = 2.5f;
    [Export] private float speedRoll = 10.0f;
    [Export] private float detectionRange = 10.0f;
    [Export] private float distanceToPrepare = 5.0f;
    [Export] private float rollDuration = 0.6f;

    [ExportGroup("Combate & Timing")]
    [Export] private int vida = 4;
    [Export] private int dano = 1;
    [Export] private float tempoDeCarga = 2f;    
    [Export] private float tempoStun = 2.0f;       
    [Export] private float knockbackForce = 6f; 
    [Export] private float knockbackDecay = 10f;

    private Node3D player;
    private Vector3 currentVelocity = Vector3.Zero;
    private Vector3 rollDirection = Vector3.Zero;
    private Vector3 knockbackVelocity = Vector3.Zero;
    private bool morto = false;

    public override void _Ready()
    {
        AddToGroup("enemy");
        player = GetTree().GetFirstNodeInGroup("player") as Node3D;
    }

    public override void _PhysicsProcess(double delta)
    {
        currentVelocity = Velocity;
    	ApplyGravity(ref currentVelocity, delta); 

		if (morto) 
		{
			currentVelocity.X = Mathf.MoveToward(currentVelocity.X, 0, 0.5f);
			currentVelocity.Z = Mathf.MoveToward(currentVelocity.Z, 0, 0.5f);
			Velocity = currentVelocity;
			MoveAndSlide();
			return; 
		}

        if (knockbackVelocity.Length() > 0.1f)
        {
            currentVelocity.X = knockbackVelocity.X;
            currentVelocity.Z = knockbackVelocity.Z;
            knockbackVelocity = knockbackVelocity.MoveToward(Vector3.Zero, knockbackDecay * (float)delta);
        }
        else
        {
            switch (currentState)
            {
                case State.Idle:
                    UpdateIdle();
                    break;
                case State.Chase:
                    UpdateChase(ref currentVelocity);
                    break;
                case State.Prepare:
                    StopMovement(ref currentVelocity);
                    FlipSprite(GetDirectionToPlayer());
                    break;
                case State.Rolling:
                    currentVelocity.X = rollDirection.X * speedRoll;
                    currentVelocity.Z = rollDirection.Z * speedRoll;
                    CheckCollisionWithPlayer();
                    break;
                case State.Stunned:
                case State.Hit:
                    StopMovement(ref currentVelocity);
                    break;
				
            }
        }

        Velocity = currentVelocity;
        MoveAndSlide();
    }

    private void UpdateIdle()
    {
        PlayAnimation("Idle");
        if (GetDistanceToPlayer() < detectionRange)
            ChangeState(State.Chase);
    }

    private void UpdateChase(ref Vector3 vel)
    {
        Vector3 dir = GetDirectionToPlayer();
        float dist = GetDistanceToPlayer();

        if (dist <= distanceToPrepare)
        {
            _ = IniciarAtaqueRolante();
            return;
        }

        vel.X = dir.X * speedChase;
        vel.Z = dir.Z * speedChase;
        PlayAnimation("Walk");
        FlipSprite(dir);
    }

    private async Task IniciarAtaqueRolante()
    {
        if (currentState != State.Chase) return;

        ChangeState(State.Prepare);
        PlayAnimation("Idle"); 
        
        await ToSignal(GetTree().CreateTimer(tempoDeCarga), "timeout");
        
        if (morto || currentState != State.Prepare) return;

        rollDirection = GetDirectionToPlayer();
        ChangeState(State.Rolling);
        PlayAnimation("Attack"); 
        if (somRolagem != null) somRolagem.Play();

        await ToSignal(GetTree().CreateTimer(rollDuration), "timeout");
        
        if (!morto && currentState == State.Rolling)
        {
            ChangeState(State.Stunned);
            PlayAnimation("Hit");
            await ToSignal(GetTree().CreateTimer(tempoStun), "timeout");
            if (!morto) ChangeState(State.Chase);
        }
    }


	private bool jaDropou = false;

	public void LevarDano(int danoRecebido, Vector3 direcao)
	{
		
		if (morto) return;

		vida -= danoRecebido;
		if (somDano != null) somDano.Play();
		knockbackVelocity = direcao * knockbackForce;

		if (vida <= 0)
		{
			
			morto = true; 
			MoverParaMorte();
			return;
		}

		
		ProcessarHitVisual();
	}

	private async void ProcessarHitVisual()
	{
		ChangeState(State.Hit);
		PlayAnimation("Hit");
		await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
		
		
		if (!morto) ChangeState(State.Chase);
	}

	private async void MoverParaMorte()
	{
		
		if (jaDropou) return;
		jaDropou = true;

		ChangeState(State.Death);
		PlayAnimation("Death");
		
		
		DropItem();

	
		CollisionLayer = 0; 
		CollisionMask = 1;  

	
		await ToSignal(GetTree().CreateTimer(1.2f), "timeout");

		QueueFree();
	}

    private void DropItem()
    {
        if (itemColetavel == null) return;

        Node3D item = itemColetavel.Instantiate<Node3D>();
        GetParent().AddChild(item);

        
        float tamanhoDesejado = 1.5f; 
        Vector3 escalaFinal = new Vector3(tamanhoDesejado, tamanhoDesejado, tamanhoDesejado);

        Transform3D t = item.GlobalTransform;
        t.Basis = Basis.Identity.Scaled(escalaFinal); 
        t.Origin = GlobalPosition + Vector3.Up * 0.5f;
        item.GlobalTransform = t;

        
        if (item.HasMethod("IniciarSalto"))
        {
            float forcaSalto = 8.0f;
            float espalhar = 3.0f;
            
            Vector3 impulsoInicial = new Vector3(
                (float)GD.RandRange(-espalhar, espalhar),
                forcaSalto,
                (float)GD.RandRange(-espalhar, espalhar)
            );

            item.Call("IniciarSalto", impulsoInicial);
        }
    }

   private void CheckCollisionWithPlayer()
	{
		if (player is Player p && !p.morto)
		{
			
			Vector3 posInimigo = GlobalPosition;
			Vector3 posPlayer = p.GlobalPosition;
			
			float distanciaHorizontal = new Vector2(posInimigo.X - posPlayer.X, posInimigo.Z - posPlayer.Z).Length();
			
		
			float diferencaAltura = Mathf.Abs(posInimigo.Y - posPlayer.Y);

		
			if (distanciaHorizontal < 1.3f && diferencaAltura < 0.8f)
			{
				p.LevarDano(dano, GetDirectionToPlayer());
				PlayAnimation("Hit");
			}
			
		}
	}

    
    private void ApplyGravity(ref Vector3 vel, double delta)
    {
        if (!IsOnFloor()) vel.Y += (float)GetGravity().Y * (float)delta;
    }

    private void StopMovement(ref Vector3 vel)
    {
        vel.X = Mathf.MoveToward(vel.X, 0, 1.0f);
        vel.Z = Mathf.MoveToward(vel.Z, 0, 1.0f);
    }

    private void ChangeState(State newState) => currentState = newState;
    private float GetDistanceToPlayer() => GlobalPosition.DistanceTo(player.GlobalPosition);
    private Vector3 GetDirectionToPlayer() => (player.GlobalPosition - GlobalPosition).Normalized();

    private void PlayAnimation(string name)
    {
        if (animationPlayer.HasAnimation(name) && animationPlayer.CurrentAnimation != name)
            animationPlayer.Play(name);
    }

    private void FlipSprite(Vector3 dir)
    {
        
        if (dir.X < 0) sprite.FlipH = false;
        else if (dir.X > 0) sprite.FlipH = true;
    }
}