using Godot;

namespace GodotZombieShooter
{
	public sealed class PlayerProxy : Player
	{
		[Signal]
		public delegate void InteractedSignal();

		[Signal]
		public delegate void StateChangedSignal(int state, int ammo);

		[Signal]
		public delegate void LifeChangedSignal(int life);

		[Signal]
		public delegate void PlayerKilledSignal();

		public PlayerProxy()
		{
			PlayerInteracted += (_, __) => EmitSignal(nameof(InteractedSignal));
			StateChanged += (_, e) => EmitSignal(nameof(StateChangedSignal), e.Item1, e.Item2);
			LifeChaged += (sender, life) => EmitSignal(nameof(LifeChangedSignal), life);
			PlayerKilled += (_, __) => EmitSignal(nameof(PlayerKilledSignal));
		}

		public void Hit(int damage)
		{
			base.HitInternal(damage);
		}
	}
}
