namespace GodotZombieShooter

open Godot

type public Player () =
    inherit KinematicBody2D ()

    let mutable speed = 100.0f

    [<Export>]
    member this.Speed
        with get () = speed
        and set (value) = speed <- value


    // move:
    // get input
    // calculate direction
    // apply move