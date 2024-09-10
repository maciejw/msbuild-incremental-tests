param(
    [Parameter()]    
    [string]
    $outDir,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]
    $files
)
Write-Host "Outdir: $outDir"

$files | ForEach-Object {
    Write-Host "File: $_"
}
$files | ForEach-Object {
    $file = Get-Item $_
    $content = Get-Content $file
    Write-Host "Compiling $file"
    if ($content -match "error") {
        [Console]::Error.WriteLine("ERROR;File=$file")
    } else {
        Write-Output "SUCCESS:File=$file"
        Copy-Item "$file" "$outDir\$($file.BaseName).dll"
    }
}
