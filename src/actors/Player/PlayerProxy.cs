using Godot;

namespace GodotZombieShooter
{
	public sealed class PlayerProxy : Player
	{
		[Signal]
		public delegate void InteractedSignal();

		public PlayerProxy()
		{
			PlayerInteracted += (_, __) => EmitSignal(nameof(InteractedSignal));
		}
	}
}
