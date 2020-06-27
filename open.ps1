$url = "https://downloads.tuxfamily.org/godotengine/3.2.1/Godot_v3.2.1-stable_win64.exe.zip"
$out = ".\.tools"

if (Test-Path -Path "$out\godot.exe")
{
    & "$out\godot.exe" --editor
}
else
{
    New-Item -ItemType Directory -Force -Path $out
    wget -Uri $url -OutFile "$out\godot.zip"
    expand-archive -path "$out\godot.zip" -DestinationPath $out 
    Rename-Item -Path "$out\Godot_v3.2.1-stable_win64.exe"  -NewName "godot.exe"
    Remove-Item "$out\godot.zip"
}