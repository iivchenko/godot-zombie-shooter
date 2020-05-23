namespace GodotZombieShooter

open Godot

type public MainMenu () as this =
    inherit Control ()

    let buttonHover = lazy(this.GetNode<AudioStreamPlayer>(new NodePath("Sounds/ButtonHover")))
    let buttonClick = lazy(this.GetNode<AudioStreamPlayer>(new NodePath("Sounds/ButtonClick")))

    [<Export>]
    let mutable NextScene = ""

    member this.OnStartPressed() = 
        buttonClick.Value.Play()
        this.GetTree().ChangeScene(NextScene)

    member this.OnExitPressed() = 
        buttonClick.Value.Play()
        this.GetTree().Quit()

    member _.OnButtonHover() = buttonHover.Value.Play()