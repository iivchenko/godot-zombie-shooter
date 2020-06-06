namespace GodotZombieShooter

open Godot

type public Zombie () as this =
    inherit KinematicBody2D ()

    let hitedEvent = new Event<_>()
    let mutable maxLife = 100
    let mutable maxSpeed = 100.0f

    let mutable life = maxLife
    let mutable attackDelay = 0.0f
    let mutable target: Player option = None
    let mutable attackTarger: IHittable option = None

    let visual = lazy(this.GetNode<CollisionShape2D>(new NodePath("VisualArea/VisualRadius")))

    let (| Alive | Dead |) life = if life > 0 then Alive else Dead 

    [<Export>]
    let mutable damage = 10

    let mutable viewRadius = 100.0f

    [<Export>]
    member _.ViewRadius 
        with get() = viewRadius 
        and set(value) =
            viewRadius <- value
            let shape = new CircleShape2D()
            shape.Radius <- viewRadius
            visual.Value.Shape <- shape

    [<Export>]
    member _.MaxSpeed with get() = maxSpeed and set(value) = maxSpeed <- value

    [<Export>]
    member _.MaxLife with get() = maxLife and set(value) = maxLife <- value

    [<CLIEvent>]
    member _.OnHited = hitedEvent.Publish

    override this._Ready() = 
        life <- maxLife

        let shape = new CircleShape2D()
        shape.Radius <- viewRadius
        visual.Value.Shape <- shape


    override _._PhysicsProcess(_: float32) = 
    
        match target with 
        | Some t -> 
            base.LookAt(t.Position)
            let direction = (t.Position - base.Position).Normalized()
            let velocity = direction * maxSpeed
            base.MoveAndSlide(velocity) |> ignore
        | None -> ()

    override _._Process (delta: float32) = 
        attackDelay <- max 0.0f (attackDelay - delta)

        match attackTarger with 
        | Some player when attackDelay = 0.0f -> 
            player.Hit(damage);
            attackDelay <- 2.0f
        | _ -> ()
    
    member _.OnPlayerDetected (player: Player) = 
        target <- Some player

    member _.OnPlayerLost (_: Player) = target <- None

    member _.OnPlayerEnteredAttackZone(player: obj) = 
        attackTarger <- player :?> IHittable |> Some
    
    member _.OnPlayerExitAttackZone(player: obj) = attackTarger <- None

    interface IHittable with
        member this.Hit(damage: int) =
            life <- max (life - damage) 0

            hitedEvent.Trigger()

            match life with 
            | Alive -> ()
            | Dead -> this.QueueFree()