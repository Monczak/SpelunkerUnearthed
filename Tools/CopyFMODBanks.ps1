# echo "Source: $($args[0])"
# echo "Dest: $($args[1])"

$source = $args[0]
$dest = $args[1]
if (!(Test-Path "$dest")) {
    mkdir "$dest"
}

Get-ChildItem "$source" -Filter *.bank | Foreach-Object {
    # echo "$_ => $dest"
    cp "$_" "$dest/"
}
