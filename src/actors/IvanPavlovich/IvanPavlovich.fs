namespace GodotZombieShooter

open Godot

type IvanPavlovichState = 
    | IdleState
    | VictoryState
    | CanInteractState

type public IvanPavlovich () as this =
    inherit KinematicBody2D ()

    let victoryState = lazy(this.GetNode<Node2D>(new NodePath("States/VictoryState")))
    let canInteractState = lazy(this.GetNode<Node2D>(new NodePath("States/CanInteractState")))
    let mutable state = IdleState

    member private _.UpdateState (newState: IvanPavlovichState) =

        match state with 
        | IdleState -> ()
        | VictoryState -> 
            victoryState.Value.SetProcess(false)
            victoryState.Value.Visible <- false
        | CanInteractState -> 
            canInteractState.Value.SetProcess(false)
            canInteractState.Value.Visible <- false

        state <- newState

        match state with 
        | IdleState -> ()
        | VictoryState -> 
            victoryState.Value.SetProcess(true)
            victoryState.Value.Visible <- true
        | CanInteractState -> 
            canInteractState.Value.SetProcess(true)
            canInteractState.Value.Visible <- true

    override this._Ready() = 
        this.UpdateState(state)
        
    member this.OnPlayerCome(_: obj) =
    
        match state with
        | IdleState -> this.UpdateState(CanInteractState)
        | _ -> ()

    member this.OnPlayerLeave(_: obj) =
        
        match state with
        | _ ->this.UpdateState(IdleState)

    member _.OnExitButtonPressed() = base.GetTree().ChangeScene("res://src/MainMenu/MainMenu.tscn")

    interface IInteractable with 
        member this.Interact () = this.UpdateState(VictoryState)
