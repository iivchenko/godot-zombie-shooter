namespace GodotZombieShooter

open Godot

type public Level () as this =
    inherit Node2D ()

    let lifeLabel = lazy(this.GetNode<Label>(NodePath("GUI/HeadUpDisplay/VBoxContainer/Life")))
    let ammoLabel = lazy(this.GetNode<Label>(NodePath("GUI/HeadUpDisplay/VBoxContainer/Ammo")))
    let simpleGun = lazy(this.GetNode<TextureRect>(NodePath("GUI/HeadUpDisplay/VBoxContainer/Guns/SimpleGun")))
    let goodGuneGun = lazy(this.GetNode<TextureRect>(NodePath("GUI/HeadUpDisplay/VBoxContainer/Guns/GoodGun")))
    let machineGun = lazy(this.GetNode<TextureRect>(NodePath("GUI/HeadUpDisplay/VBoxContainer/Guns/MachineGun")))
    let defeatUi = lazy(this.GetNode<Control>(NodePath("GUI/DefeatScreen")))
    let fade = lazy(this.GetNode<AnimationPlayer>(NodePath("GUI/DefeatScreen/Animation")))
    
    let mutable player: Player = Unchecked.defaultof<Player> 

    member _.PlayerStateChanged(state: PlayerStateStructure) =
        match state with
        | SimpleGunState (_, _, ammo, _) ->
            simpleGun.Value.Visible <- true
            simpleGun.Value.Modulate <- Color(1.0f, 1.0f, 1.0f, 1.0f)
            goodGuneGun.Value.Modulate <- Color(1.0f, 1.0f, 1.0f, 0.25f)
            machineGun.Value.Modulate <- Color(1.0f, 1.0f, 1.0f, 0.25f)

            ammoLabel.Value.Text <- sprintf "Ammo: %i" ammo
        | GoodGunState (_, _, ammo, _) ->
            goodGuneGun.Value.Visible <- true
            simpleGun.Value.Modulate <- Color(1.0f, 1.0f, 1.0f, 0.25f)
            goodGuneGun.Value.Modulate <- Color(1.0f, 1.0f, 1.0f, 1.0f)
            machineGun.Value.Modulate <- Color(1.0f, 1.0f, 1.0f, 0.25f)

            ammoLabel.Value.Text <- sprintf "Ammo: %i" ammo
        | MachineGunState (_, _, ammo, _) ->
            machineGun.Value.Visible <- true
            simpleGun.Value.Modulate <- Color(1.0f, 1.0f, 1.0f, 0.25f)
            goodGuneGun.Value.Modulate <- Color(1.0f, 1.0f, 1.0f, 0.25f)
            machineGun.Value.Modulate <- Color(1.0f, 1.0f, 1.0f, 1.0f)

            ammoLabel.Value.Text <- sprintf "Ammo: %i" ammo

    member _.OnGameOver() =      
        this.GetTree().Paused <- true
        defeatUi.Value.Visible <- true
        fade.Value.Play("Fade")
        Input.SetMouseMode(Input.MouseMode.Visible)
        player.QueueFree()
            
    override this._Ready() =
        this.GetNode<Node2D>(NodePath("Tools")).QueueFree()
        simpleGun.Value.Visible <- false
        goodGuneGun.Value.Visible <- false
        machineGun.Value.Visible <- false

        let position = this.GetNode<Node2D>(new NodePath("Game/PlayerSpawner")).GlobalPosition

        let path = match Global.Hero with 
        | 1 -> "res://src/Actors/Player/Tarchavka.tscn"
        | 2 -> "res://src/Actors/Player/Emilio.tscn"

        let playerScene = ResourceLoader.Load(path) :?> PackedScene
        player <- playerScene.Instance() :?> Player
        player.GlobalPosition <- position
        player.StateChanged.Add(fun (state) -> this.PlayerStateChanged(state))
        player.LifeChaged.Add(fun life -> lifeLabel.Value.Text <- sprintf "Life: %i" life)
        player.PlayerKilled.Add(this.OnGameOver)
        this.AddChild(player)

        this.GetTree().Paused <- false

    member _.OnRestartButtonPressed() = this.GetTree().ReloadCurrentScene()
    
    member _.OnExitButtonPressed() = this.GetTree().ChangeScene("res://src/MainMenu/MainMenu.tscn")