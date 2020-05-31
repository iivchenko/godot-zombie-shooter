namespace GodotZombieShooter

open Godot

[<Tool>]
type public SelectHero () as this =
    inherit Control ()

    let buttonHover = lazy(this.GetNode<AudioStreamPlayer>(new NodePath("Sounds/ButtonHover")))
    let buttonClick = lazy(this.GetNode<AudioStreamPlayer>(new NodePath("Sounds/ButtonClick")))

    [<Export>]
    let mutable NextScene = ""

    override _._Ready() = 
        Input.SetMouseMode(Input.MouseMode.Visible)

    member this.OnStartPressed() = 
        buttonClick.Value.Play()
        this.GetTree().ChangeScene(NextScene)

    member _.OnButtonHover() = buttonHover.Value.Play()

    member _.OnTarchavkaInput(event: InputEvent) = 
        match event with
        | :? InputEventMouse as mouse when mouse.ButtonMask = 1 -> 
            Global.Hero <- 1
            this.GetNode<ColorRect>(new NodePath("VBoxContainer/HBoxContainer/Tarchavka/Selected")).Visible <- true
            this.GetNode<ColorRect>(new NodePath("VBoxContainer/HBoxContainer/Emilio/Selected")).Visible <- false
        | _ -> ()           

    member _.OnEmilioInput(event: InputEvent) = 
        match event with
        | :? InputEventMouse as mouse when mouse.ButtonMask = 1 -> 
            Global.Hero <- 2
            this.GetNode<ColorRect>(new NodePath("VBoxContainer/HBoxContainer/Tarchavka/Selected")).Visible <- false
            this.GetNode<ColorRect>(new NodePath("VBoxContainer/HBoxContainer/Emilio/Selected")).Visible <- true
        | _ -> ()
        