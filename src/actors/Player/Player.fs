namespace GodotZombieShooter

open Godot

type public Player () as this =
    inherit KinematicBody2D ()

    [<Export>] 
    let mutable hitFactory : PackedScene = null

    let target = lazy(this.GetNode<Sprite>(NodePath("TargetRay/Target")))
    let targetRay = lazy(this.GetNode<RayCast2D>(NodePath("TargetRay")))

    let mutable maxSpeed = 100.0f

    [<Export>]
    member _.MaxSpeed
        with get () = maxSpeed
        and set (value) = maxSpeed <- value

    override _._Ready() = Input.SetMouseMode(Input.MouseMode.Hidden)

    override _._Process(_: float32) = 

        target.Value.GlobalPosition <- this.GetGlobalMousePosition()
        targetRay.Value.CastTo <- target.Value.Position

        match Input.IsActionJustPressed("shoot") with 
        | true -> 
            let hit = hitFactory.Instance() :?> Hit;
            hit.GlobalPosition <- if targetRay.Value.IsColliding() then targetRay.Value.GetCollisionPoint() else target.Value.GlobalPosition
            base.GetTree().CurrentScene.AddChild(hit)
            hit.Hit()
        | _ -> ()

    override this._PhysicsProcess (_: float32) =
        
        let direction = Vector2(Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"), Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up")) 
        let velocity = direction * maxSpeed
        this.LookAt(this.GetGlobalMousePosition())
        this.MoveAndSlide(velocity) |> ignore