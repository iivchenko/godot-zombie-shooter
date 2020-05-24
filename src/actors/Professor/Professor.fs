namespace GodotZombieShooter

open Godot

type ProfessorState = 
    | IdleState
    | HasTaskState
    | CanInteractState
    | ShowTaskState

type public Professor () as this =
    inherit KinematicBody2D ()

    let hasTaskState = lazy(this.GetNode<Node2D>(new NodePath("States/HasTaskState")))
    let canInteractState = lazy(this.GetNode<Node2D>(new NodePath("States/CanInteractState")))
    let showTaskState = lazy(this.GetNode<Node2D>(new NodePath("States/ShowTaskState")))
    let mutable state = HasTaskState

    member private _.UpdateState (newState: ProfessorState) =

        match state with 
        | IdleState -> ()
        | HasTaskState -> 
            hasTaskState.Value.SetProcess(false)
            hasTaskState.Value.Visible <- false
        | CanInteractState ->
            canInteractState.Value.SetProcess(false)
            canInteractState.Value.Visible <- false
        | ShowTaskState ->
            showTaskState.Value.SetProcess(false)
            showTaskState.Value.Visible <- false

        state <- newState

        match state with 
        | IdleState -> ()
        | HasTaskState -> 
            hasTaskState.Value.SetProcess(true)
            hasTaskState.Value.Visible <- true
        | CanInteractState -> 
            canInteractState.Value.SetProcess(true)
            canInteractState.Value.Visible <- true
        | ShowTaskState ->
            showTaskState.Value.SetProcess(true)
            showTaskState.Value.Visible <- true

    override this._Ready() = 
        this.UpdateState(state)
        
    member this.OnPlayerCome(_: obj) =
    
        match state with
        | HasTaskState -> this.UpdateState(CanInteractState)
        | IdleState -> this.UpdateState(CanInteractState)
        | _ -> ()

    member this.OnPlayerLeave(_: obj) =
        
        match state with
        | CanInteractState 
        | ShowTaskState 
            -> this.UpdateState(IdleState)
        | _ -> ()

    interface IInteractable with 
        member this.Interact () = this.UpdateState(ShowTaskState)