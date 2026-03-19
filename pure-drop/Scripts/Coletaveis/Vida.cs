using Godot;
using System;

public partial class Vida : Area3D , IColetaveis
{
	public void Execute(Player player)
	{
		player.vida = player.vidaMax;
		QueueFree();
	}
}
