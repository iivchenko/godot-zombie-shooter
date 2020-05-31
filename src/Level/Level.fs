namespace GodotZombieShooter

open Godot

type public Level () =
    inherit Node2D ()


    override this._Ready() =
        let position = this.GetNode<Node2D>(new NodePath("PlayerSpawner")).GlobalPosition

        let path = match Global.Hero with 
        | 1 -> "res://src/Actors/Player/Tarchavka.tscn"
        | 2 -> "res://src/Actors/Player/Emilio.tscn"

        let playerScene = ResourceLoader.Load(path) :?> PackedScene
        let player = playerScene.Instance() :?> Node2D
        player.GlobalPosition <- position
        this.AddChild(player)

