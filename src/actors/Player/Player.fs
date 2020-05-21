namespace GodotZombieShooter

open Godot

type public Player () as this =
    inherit KinematicBody2D ()

    let target = lazy(this.GetNode<Sprite>(NodePath("TargetRay/Target")))
    let targetRay = lazy(this.GetNode<RayCast2D>(NodePath("TargetRay")))
    let step = lazy(this.GetNode<AudioStreamPlayer2D>(NodePath("Audio/Step")))
    let gunShot = lazy(this.GetNode<AudioStreamPlayer2D>(NodePath("Audio/GunShot")))

    let mutable audio = false
    let mutable maxSpeed = 100.0f
    let mutable velocity = Vector2.Zero

    [<Export>] 
    let mutable hitFactory : PackedScene = null

    [<Export>]
    member _.MaxSpeed
        with get () = maxSpeed
        and set (value) = maxSpeed <- value

    override _._Ready() = Input.SetMouseMode(Input.MouseMode.Hidden)

    override _._Process(_: float32) = 

        match velocity <> Vector2.Zero with
        | true when not audio ->
            audio <- true
            step.Value.Play()
        | _ -> ()

        target.Value.GlobalPosition <- this.GetGlobalMousePosition()
        targetRay.Value.CastTo <- target.Value.Position

        match Input.IsActionJustPressed("shoot") with 
        | true -> 
            let hit = hitFactory.Instance() :?> Hit;
            hit.GlobalPosition <- if targetRay.Value.IsColliding() then targetRay.Value.GetCollisionPoint() else target.Value.GlobalPosition
            base.GetTree().CurrentScene.AddChild(hit)
            hit.Hit()
            gunShot.Value.Play()
        | _ -> ()

    override this._PhysicsProcess (_: float32) =
        
        let direction = Vector2(Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"), Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up")) 
        velocity <- direction * maxSpeed
        this.LookAt(this.GetGlobalMousePosition())
        velocity <- this.MoveAndSlide(velocity)

    member _.OnStepAudioFinished() = audio <- false