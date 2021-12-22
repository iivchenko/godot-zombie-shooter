$url = "https://downloads.tuxfamily.org/godotengine/3.2.1/Godot_v3.2.1-stable_win64.exe.zip"
$serverurl="https://downloads.tuxfamily.org/godotengine/3.2.1/Godot_v3.2.1-stable_linux_headless.64.zip"
$out = ".\.tools"

function Download-Godot {

    New-Item -ItemType Directory -Force -Path $out
    wget -Uri $url -OutFile "$out\godot.zip"
    expand-archive -path "$out\godot.zip" -DestinationPath $out 
    Rename-Item -Path "$out\Godot_v3.2.1-stable_win64.exe"  -NewName "godot.exe"
    Remove-Item "$out\godot.zip"
}

function Download-SeverGodot {
	New-Item -ItemType Directory -Force -Path $out
    wget -Uri $serverurl -OutFile "$out\godot.zip"
    expand-archive -path "$out\godot.zip" -DestinationPath $out 
    Rename-Item -Path "$out\Godot_v3.2.1-stable_linux_headless.64"  -NewName "godot"
    Remove-Item "$out\godot.zip"
}

function Run-Editor {
    
    param()

    if (-not (Test-Path -Path "$out\godot.exe"))
    {
        Download-Godot        
    }

    & "$out\godot.exe" --editor
}

function Run-Game {

    param()

    if (-not (Test-Path -Path "$out\godot.exe"))
    {
        Download-Godot
        
    }

    & "$out\godot.exe"
}

function Run-Export {
    param()

    if (-not (Test-Path -Path "$out\godot.exe"))
    {
        Download-Godot
        
    }

    New-Item -ItemType Directory -Force -Path .\publish\winx64\

    & "$out\godot.exe" --path . --export "win-x64" .\publish\winx64\zobmies.exe
}

function Run-ServerExport {
    param()

    if (-not (Test-Path -Path "$out\godot.exe"))
    {
        Download-SeverGodot        
    }

    New-Item -ItemType Directory -Force -Path .\publish\winx64\

    & "$out\godot" --path . --export "win-x64" .\publish\winx64\zobmies.exe
}

function Run-Tests {
    param()

    if (-not (Test-Path -Path "$out\godot.exe"))
    {
        Download-Godot
        
    }

    & "$out\godot.exe" -d --path . test\gut.tscn
}

Export-ModuleMember -Function * -Alias *