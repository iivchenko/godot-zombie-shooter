namespace GodotZombieShooter

open Godot

type public Level () as this =
    inherit Node2D ()

    let simpleGun = lazy(this.GetNode<TextureRect>(NodePath("HeadUpDisplay/HeadUpDisplay/VBoxContainer/Guns/SimpleGun")))
    let goodGuneGun = lazy(this.GetNode<TextureRect>(NodePath("HeadUpDisplay/HeadUpDisplay/VBoxContainer/Guns/GoodGun")))
    let machineGun = lazy(this.GetNode<TextureRect>(NodePath("HeadUpDisplay/HeadUpDisplay/VBoxContainer/Guns/MachineGun")))
    let ammoLabel = lazy(this.GetNode<Label>(NodePath("HeadUpDisplay/HeadUpDisplay/VBoxContainer/Ammo")))

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
            
    override this._Ready() =
        simpleGun.Value.Visible <- false
        goodGuneGun.Value.Visible <- false
        machineGun.Value.Visible <- false

        let position = this.GetNode<Node2D>(new NodePath("PlayerSpawner")).GlobalPosition

        let path = match Global.Hero with 
        | 1 -> "res://src/Actors/Player/Tarchavka.tscn"
        | 2 -> "res://src/Actors/Player/Emilio.tscn"

        let playerScene = ResourceLoader.Load(path) :?> PackedScene
        let player = playerScene.Instance() :?> Player
        player.GlobalPosition <- position
        player.StateChanged.Add(fun (state) -> this.PlayerStateChanged(state))
        this.AddChild(player)

