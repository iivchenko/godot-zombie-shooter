namespace GodotZombieShooter

open Godot

type public Hit () as this =
    inherit Area2D ()

    let mutable damage = 25

    let effect = lazy(this.GetNode<Particles2D>(NodePath("Effect")))

    [<Export>]
    member _.Damage
        with get () = damage
        and set (value) = damage <- value

    member _.Hit() = effect.Value.Emitting <- true

    member _.OnHitBodyEntered(body: Node2D) =
        match body with 
        | (:? IHittable as target) -> target.Hit(damage)
        | _ -> ()