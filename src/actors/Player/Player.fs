namespace GodotZombieShooter

open Godot

type public Player () as this =
    inherit KinematicBody2D ()

    let stateChangedEvent = new Event<_>()

    let target = lazy(this.GetNode<Sprite>(NodePath("TargetRay/Target")))
    let targetRay = lazy(this.GetNode<RayCast2D>(NodePath("TargetRay")))
    let step = lazy(this.GetNode<AudioStreamPlayer2D>(NodePath("Audio/Step")))
    let gunShot = lazy(this.GetNode<AudioStreamPlayer2D>(NodePath("Audio/GunShot")))
    let noAmmo = lazy(this.GetNode<AudioStreamPlayer2D>(NodePath("Audio/NoAmmo")))

    let standState = lazy(this.GetNode<Node2D>(new NodePath("State/Stand")))
    let simpleGunState = lazy(this.GetNode<Node2D>(new NodePath("State/SimpleGun")))
    let goodGunState = lazy(this.GetNode<Node2D>(new NodePath("State/GoodGun")))
    let machineGunState = lazy(this.GetNode<Node2D>(new NodePath("State/MachineGun")))
    let mutable state = Stand

    let mutable audio = false
    let mutable maxSpeed = 100.0f
    let mutable velocity = Vector2.Zero
    let mutable interactable: IInteractable option = None

    let mutable simpleGun = -1
    let mutable goodGun = -1
    let mutable machineGun = -1

    let isShootState () = 
        match state with  
        | SimpleGun
        | GoodGun
        | MachineGun -> true
        | _ -> false

    let hasArmo () =
        match state with 
        | SimpleGun when simpleGun > 0 -> true
        | GoodGun when goodGun > 0 -> true
        | MachineGun when machineGun > 0 -> true
        | _ -> false

    let mutable hitFactory : PackedScene = null

    [<Export>]
    member _.MaxSpeed
        with get () = maxSpeed
        and set (value) = maxSpeed <- value

    [<CLIEvent>]
    member _.StateChanged = stateChangedEvent.Publish

    member private _.DisableState(state: PlayerState) = 
        
        match state with
        | Stand -> 
            standState.Value.SetProcess(false)
            standState.Value.Visible <- false
        | SimpleGun -> 
            simpleGunState.Value.SetProcess(false)
            simpleGunState.Value.Visible <- false
        | GoodGun -> 
            goodGunState.Value.SetProcess(false)
            goodGunState.Value.Visible <- false
        | MachineGun -> 
            machineGunState.Value.SetProcess(false)
            machineGunState.Value.Visible <- false

    member private _.EnableState(state: PlayerState) = 
                
        match state with
        | Stand -> 
            standState.Value.SetProcess(true)
            standState.Value.Visible <- true
        | SimpleGun -> 
            simpleGunState.Value.SetProcess(true)
            simpleGunState.Value.Visible <- true
        | GoodGun -> 
            goodGunState.Value.SetProcess(true)
            goodGunState.Value.Visible <- true
        | MachineGun -> 
            machineGunState.Value.SetProcess(true)
            machineGunState.Value.Visible <- true

    member private _.Switch(state': PlayerState) =
        match state' with 
        | Stand -> ()
        | SimpleGun when simpleGun > -1 -> 
            this.DisableState(state)
            state <- state'
            this.EnableState(state)
        | GoodGun when goodGun > -1 -> 
            this.DisableState(state)
            state <- state'
            this.EnableState(state)
        | MachineGun when machineGun > -1 -> 
            this.DisableState(state)
            state <- state'
            this.EnableState(state)

    member private _.DecreaseAmmo() = 
        
        match state with
        | Stand -> ()
        | SimpleGun -> 
            simpleGun <- simpleGun - 1
            stateChangedEvent.Trigger(state, simpleGun)
        | GoodGun ->
            goodGun <- goodGun - 1
            stateChangedEvent.Trigger(state, goodGun)
        | MachineGun ->
            machineGun <- machineGun - 1
            stateChangedEvent.Trigger(state, machineGun)

    override _._Ready() = 
        hitFactory <- ResourceLoader.Load("res://src/Effects/HitEffect/Hit.tscn") :?> PackedScene
        Input.SetMouseMode(Input.MouseMode.Hidden)

    override _._Process(_: float32) = 

        match velocity <> Vector2.Zero with
        | true when not audio ->
            audio <- true
            step.Value.Play()
        | _ -> ()

        target.Value.GlobalPosition <- this.GetGlobalMousePosition()
        targetRay.Value.CastTo <- target.Value.Position

        match Input.IsActionJustPressed("shoot") with 
        | true when isShootState() && hasArmo() -> 
            let hit = hitFactory.Instance() :?> Hit;
            hit.GlobalPosition <- if targetRay.Value.IsColliding() then targetRay.Value.GetCollisionPoint() else target.Value.GlobalPosition
            base.GetTree().CurrentScene.AddChild(hit)
            hit.Hit()
            gunShot.Value.Play()
            this.DecreaseAmmo()
        | true when isShootState() ->
            noAmmo.Value.Play()
        | _ -> ()

    override this._PhysicsProcess (_: float32) =
        
        let direction = Vector2(Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"), Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up")) 
        velocity <- direction * maxSpeed
        this.LookAt(this.GetGlobalMousePosition())
        velocity <- this.MoveAndSlide(velocity)

    override _._UnhandledInput(e: InputEvent) =
        match e, interactable with 
        | :? InputEventKey as key, Some(i) when key.IsActionPressed("interact") -> 
            i.Interact()

            match i with 
            | :? Gun as gun -> 
                match gun.Gun with 
                | GunType.Simple ->
                    simpleGun <- simpleGun + 100
                    this.Switch(SimpleGun)
                    stateChangedEvent.Trigger(SimpleGun, simpleGun)
                | GunType.Good -> 
                    goodGun <- goodGun + 100
                    this.Switch(GoodGun)
                    this.EmitSignal("StateChange", GoodGun)
                    stateChangedEvent.Trigger(GoodGun, goodGun)
                | GunType.Machine -> 
                    machineGun <- machineGun + 100
                    this.Switch(MachineGun)
                    this.EmitSignal("StateChange", MachineGun)
                    stateChangedEvent.Trigger(MachineGun, machineGun)
            | _ -> ()
                
        | _ -> ()

    member _.OnStepAudioFinished() = audio <- false
    
    member _.OnInteractionBegin(body: obj) = 
        match body with 
        | :? IInteractable as o -> interactable <- Some o
        | _ -> ()

    member _.OnInteractionFinish(body: obj) = interactable <- None