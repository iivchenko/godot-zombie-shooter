namespace GodotZombieShooter

open Godot

type GunType =
    | Simple = 0
    | Good = 1
    | Machine = 2

type GunState = 
    | Idle
    | Interact

type public Gun () as this =
    inherit KinematicBody2D ()

    let mutable state = Idle

    let interact = lazy(this.GetNode<Node2D>(new NodePath("States/Interact")))

    let mutable gun = GunType.Simple

    [<Export>]
    member _.Gun with get() = gun and set(value) = gun <- value

    member private _.UpdateState (newState: GunState) =

        match state with 
        | Idle -> ()       
        | Interact ->
            interact.Value.SetProcess(false)
            interact.Value.Visible <- false

        state <- newState

        match state with 
        | Idle -> ()
        | Interact ->
            interact.Value.SetProcess(true)
            interact.Value.Visible <- true

     override this._Ready() = 
         this.UpdateState(state)
         
     member this.OnPlayerCome(_: obj) =
     
         match state with
         | Idle -> this.UpdateState(Interact)
         | _ -> ()

     member this.OnPlayerLeave(_: obj) =
         
         match state with
         | Interact -> this.UpdateState(Idle)
         | _ -> ()

     interface IInteractable with 
         member this.Interact () = this.QueueFree()