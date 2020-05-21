namespace GodotZombieShooter

open Godot

type public Hit () as this =
    inherit Area2D ()

    let mutable damage = 25
    let mutable enabled = true

    let effect = lazy(this.GetNode<Particles2D>(NodePath("Effect")))

    [<Export>]
    member _.Damage
        with get () = damage
        and set (value) = damage <- value

    member this.Hit() =
        effect.Value.Emitting <- true

        async {
            while effect.Value.Emitting do ()
            this.QueueFree()
        } |> Async.StartAsTask |> ignore

    member this.OnHitBodyEntered(body: obj) =
        match body with 
        | (:? IHittable as target) when enabled -> 
            target.Hit(damage)
            enabled <- false
        | _ -> ()