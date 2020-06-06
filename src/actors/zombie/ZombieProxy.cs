using Godot;

namespace GodotZombieShooter
{
	public sealed class ZombieProxy : Zombie
	{
		[Signal]
		public delegate void HitedSignal();

		public ZombieProxy()
		{
			OnHited += (_, __) => EmitSignal(nameof(HitedSignal));
		}
	}
}
