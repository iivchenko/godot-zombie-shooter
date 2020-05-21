namespace GodotZombieShooter

open Godot

type public Spawner () as this =
    inherit Node2D ()

    let timer = lazy(this.GetNode<Timer>(NodePath("Timer")))
    let tree = lazy(this.GetTree())

    let mutable interval = 60.0f
    let mutable object = Unchecked.defaultof<PackedScene>

    [<Export>]
    member _.Interval with get() = interval and set(value) = interval <- value

    [<Export>]
    member _.Object with get() = object and set(value) = object <- value

    override _._Ready() =
        timer.Value.OneShot <- false
        timer.Value.WaitTime <- interval
        timer.Value.Start()

    member _.OnTimer() =
        let o = object.Instance() :?> Node2D;
        o.GlobalPosition <- this.GlobalPosition

        tree.Value.Root.AddChild(o)