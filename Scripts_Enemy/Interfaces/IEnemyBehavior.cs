using Godot;
using System;
using PlayerC;
using EnemyC;

namespace Interfaces
{
	public interface IEnemyBehavior
	{
		void Execute(Player player, Enemy enemy, double delta);
	}
}

