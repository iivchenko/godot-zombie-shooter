namespace GodotZombieShooter

open Godot

type PlayerStateStructure =
    | EmptyState
    | StandState of node: Node2D
    | SimpleGunState of node: Node2D * damage: int * ammo: int * delay: float32
    | GoodGunState of node: Node2D * damage: int * ammo: int * delay: float32
    | MachineGunState of node: Node2D * damage: int * ammo: int * delay: float32

type public Player () as this =
    inherit KinematicBody2D ()

    let stateChangedEvent = new Event<_>()
    let lifeChangedEvent = new Event<_>()
    let playerKilledEvent = new Event<_>()
    let playerInteractedEvent = new Event<_>()

    let target = lazy(this.GetNode<Sprite>(NodePath("TargetRay/Target")))
    let targetRay = lazy(this.GetNode<RayCast2D>(NodePath("TargetRay")))
    let step = lazy(this.GetNode<AudioStreamPlayer2D>(NodePath("Audio/Step")))
    let gunShot = lazy(this.GetNode<AudioStreamPlayer2D>(NodePath("Audio/GunShot")))
    let noAmmo = lazy(this.GetNode<AudioStreamPlayer2D>(NodePath("Audio/NoAmmo")))
    let hitSound = lazy(this.GetNode<AudioStreamPlayer2D>(NodePath("Audio/Hit")))
    let bloodEffect = lazy(this.GetNode<Particles2D>(NodePath("Blood")))

    let mutable state = Stand
    let mutable states: Map<PlayerState, PlayerStateStructure> = Map.empty

    let mutable life = 100
    let mutable audio = false
    let mutable maxSpeed = 100.0f
    let mutable velocity = Vector2.Zero
    let mutable shootDelay = 0.0f

    let (| Alive | Dead |) life = if life > 0 then Alive else Dead 

    let isShootState () = 
        match state with  
        | SimpleGun
        | GoodGun
        | MachineGun -> true
        | _ -> false

    let hasArmo (state) =
        match state with 
        | SimpleGunState (_, _, ammo, _)
        | GoodGunState (_, _, ammo, _)
        | MachineGunState (_, _, ammo, _) -> ammo > 0
        | _ -> false

    let noShootDelay () = shootDelay <= 0.0f

    let mutable hitFactory : PackedScene = null
    
    let getDamage state = 
        match states.[state] with 
        | SimpleGunState (_, damage, _, _)
        | GoodGunState (_, damage, _, _)
        | MachineGunState (_, damage, _, _) -> damage
        | _ -> 0
    
    let getShootDelay state =
        match state with 
        | SimpleGunState (_, _, _, delay)
        | GoodGunState (_, _, _, delay)
        | MachineGunState (_, _, _, delay) -> delay
        | _ -> 0.0f

    [<Export>]
    member _.MaxSpeed
        with get () = maxSpeed
        and set (value) = maxSpeed <- value

    [<Export>]
    member _.Life
        with get () = life
        and set (value) =
            life <- value
            lifeChangedEvent.Trigger(life)

    [<CLIEvent>]
    member _.StateChanged = stateChangedEvent.Publish

    [<CLIEvent>]
    member _.PlayerKilled = playerKilledEvent.Publish

    [<CLIEvent>]
    member _.LifeChaged = lifeChangedEvent.Publish

    [<CLIEvent>]
    member _.PlayerInteracted = playerInteractedEvent.Publish

    member _.AddSimpleGun(ammo: int) =
        this.Switch(SimpleGun)
        this.UpdateAmmo(ammo)

    member _.AddGoodGun(ammo: int) =
        this.Switch(GoodGun)
        this.UpdateAmmo(ammo)

    member _.AddMachineGun(ammo: int) =
        this.Switch(MachineGun)
        this.UpdateAmmo(ammo)

    member private _.DisableState(state: PlayerState) = 
        
        match states.[state] with
        | StandState (node)
        | SimpleGunState (node, _, _, _)
        | GoodGunState (node, _, _, _)
        | MachineGunState (node, _, _, _) -> 
            node.SetProcess(false)
            node.Visible <- false
        | _ -> ()

    member private _.EnableState(state: PlayerState) = 
        
        match states.[state] with
        | StandState (node)
        | SimpleGunState (node, _, _, _)
        | GoodGunState (node, _, _, _)
        | MachineGunState (node, _, _, _) -> 
            node.SetProcess(true)
            node.Visible <- true
        | _ -> ()

    member private _.Switch(state': PlayerState) =
        match state' with 
        | SimpleGun
        | GoodGun
        | MachineGun ->
            this.DisableState(state)
            state <- state'
            this.EnableState(state)

            stateChangedEvent.Trigger(states.[state])
        | _ -> ()

    member private _.UpdateAmmo(ammo': int) = 
        
        let state' = match states.[state] with
            | SimpleGunState (node, damage, ammo, delay) -> 
               SimpleGunState(node, damage, ammo + ammo', delay)                
            | GoodGunState (node, damage, ammo, delay) ->
                GoodGunState(node, damage, ammo + ammo', delay)
            | MachineGunState (node, damage, ammo, delay) ->
                MachineGunState(node, damage, ammo + ammo', delay)

        states <- states |> Map.filter (fun key _ -> key <> state)
        states <- states.Add(state, state')
        stateChangedEvent.Trigger(states.[state])

    override _._Ready() = 
        hitFactory <- ResourceLoader.Load("res://src/Effects/HitEffect/Hit.tscn") :?> PackedScene
        Input.SetMouseMode(Input.MouseMode.Hidden)

        states <- 
            states
                .Add(Stand, StandState(this.GetNode<Node2D>(new NodePath("State/Stand"))))
                .Add(SimpleGun, SimpleGunState(this.GetNode<Node2D>(new NodePath("State/SimpleGun")), 25, -1, 0.6f))
                .Add(GoodGun, GoodGunState(this.GetNode<Node2D>(new NodePath("State/GoodGun")), 40, -1, 0.2f))
                .Add(MachineGun, MachineGunState(this.GetNode<Node2D>(new NodePath("State/MachineGun")), 75, -1, 0.01f))

    override _._Process(delta: float32) = 

        shootDelay <- max 0.0f (shootDelay - delta)
        match velocity <> Vector2.Zero with
        | true when not audio ->
            audio <- true
            step.Value.Play()
        | _ -> ()

        target.Value.GlobalPosition <- this.GetGlobalMousePosition()
        targetRay.Value.CastTo <- target.Value.Position

        match Input.IsActionJustPressed("player_shoot") with 
        | true when isShootState() && hasArmo(states.[state]) && noShootDelay() ->
            let hit = hitFactory.Instance() :?> Hit;
            hit.Damage <- getDamage state
            hit.GlobalPosition <- if targetRay.Value.IsColliding() then targetRay.Value.GetCollisionPoint() else target.Value.GlobalPosition
            base.GetTree().CurrentScene.AddChild(hit)
            hit.Hit()
            gunShot.Value.Play()
            this.UpdateAmmo(-1)
            shootDelay <- getShootDelay states.[state]
        | true when isShootState() && hasArmo(states.[state]) |> not ->
            noAmmo.Value.Play()
        | _ -> ()
         
        match Input.IsActionJustPressed("player_select_simple_gun") with
        | true when hasArmo(states.[SimpleGun])-> this.Switch(SimpleGun)
        | _ -> ()

        match Input.IsActionJustPressed("player_select_good_gun") with
        | true when hasArmo(states.[GoodGun]) -> this.Switch(GoodGun)
        | _ -> ()

        match Input.IsActionJustPressed("player_select_machine_gun") with
        | true when hasArmo(states.[MachineGun]) -> this.Switch(MachineGun)
        | _ -> ()

    override this._PhysicsProcess (_: float32) =
        
        let direction = Vector2(Input.GetActionStrength("player_move_right") - Input.GetActionStrength("player_move_left"), Input.GetActionStrength("player_move_down") - Input.GetActionStrength("player_move_up")) 
        velocity <- direction * maxSpeed
        this.LookAt(this.GetGlobalMousePosition())
        velocity <- this.MoveAndSlide(velocity)

    override _._UnhandledInput(e: InputEvent) =
        match e with 
        | :? InputEventKey as key when key.IsActionPressed("interact") -> playerInteractedEvent.Trigger()
        | _ -> ()

    member _.OnStepAudioFinished() = audio <- false

    member this.HitInternal(damage: int) =
        life <- max (life - damage) 0

        lifeChangedEvent.Trigger(life)
        hitSound.Value.Play()
        bloodEffect.Value.Emitting <- true

        match life with 
        | Alive -> ()
        | Dead -> 
            playerKilledEvent.Trigger()