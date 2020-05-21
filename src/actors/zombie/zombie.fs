namespace GodotZombieShooter

open Godot

type public Zombie () =
    inherit KinematicBody2D ()
    let mutable maxLife = 100
    let mutable maxSpeed = 100.0f

    let mutable life = maxLife

    let mutable target: Player option = None

    let (| Alive | Dead |) life = if life > 0 then Alive else Dead 

    [<Export>]
    member _.MaxSpeed with get() = maxSpeed and set(value) = maxSpeed <- value

    [<Export>]
    member _.MaxLife with get() = maxLife and set(value) = maxLife <- value

    override _._Ready() = life <- maxLife

    override _._PhysicsProcess(_: float32) = 
    
        match target with 
        | Some t -> 
            base.LookAt(t.Position)
            let direction = (t.Position - base.Position).Normalized()
            let velocity = direction * maxSpeed
            base.MoveAndSlide(velocity) |> ignore
        | None -> ()
    
    member _.OnPlayerDetected (player: Player) = 
        target <- Some player

    member _.OnPlayerLost (_: Player) = target <- None

    interface IHittable with
        member this.Hit(damage: int) =
            life <- max (life - damage) 0

            match life with 
            | Alive -> ()
            | Dead -> this.QueueFree()