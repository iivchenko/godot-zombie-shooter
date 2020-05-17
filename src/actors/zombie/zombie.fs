namespace GodotZombieShooter

open Godot

type public Zombie () =
    inherit KinematicBody2D ()

    let mutable target: Player option = None
    let mutable speed = 100.0f

    [<Export>]
    member _.Speed with get() = speed and set(value) = speed <- value

    override _._PhysicsProcess(_: float32) = 
    
        match target with 
        | Some t -> 
            base.LookAt(t.Position)
            let direction = (t.Position - base.Position).Normalized()
            let velocity = direction * speed
            base.MoveAndSlide(velocity) |> ignore
        | None -> ()
        
    
    member _.OnPlayerDetected (player: Player) = target <- Some player

    member _.OnPlayerLost (_: Player) = target <- None