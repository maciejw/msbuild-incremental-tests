param(
    [Parameter()]
    [string]
    $OutputDirectory,
    [Parameter()]
    $SourceFiles,
    [Parameter()]
    $IncludeFiles
)
Write-Verbose "OutputDirectory: $OutputDirectory"

Write-Verbose "SourceFiles"
$SourceFiles | Out-String | Write-Verbose

Write-Verbose "IncludeFiles"
$IncludeFiles | Out-String | Write-Verbose

$SourceFiles | ForEach-Object {
    $file = Get-Item $_
    $content = Get-Content $file
    Write-Verbose "Compiling $file"
    if ($content -match "error") {
        Write-Error "ERROR;File=$file"
    } else {
        Write-Output "SUCCESS:File=$file"
        Copy-Item "$file" "$OutputDirectory\$($file.BaseName).dll"
    }
}