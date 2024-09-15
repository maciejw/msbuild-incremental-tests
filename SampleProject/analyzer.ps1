[CmdletBinding()]
param(
    [Parameter()]
    $SourceFiles,
    [Parameter()]
    $IncludeFiles
)
Write-Verbose "SourceFiles"
$SourceFiles | Out-String | Write-Verbose
Write-Verbose "IncludeFiles"
$IncludeFiles | Out-String | Write-Verbose

$SourceFiles 
| ForEach-Object -ThrottleLimit 5 -Parallel { 
    $source = $_
    $result = Get-Item $source | Select-String "^#include=(.+)$" -AllMatches
    | ForEach-Object { [pscustomobject]@{Source = [string]$source; Include = [string]$_.Matches.Groups[1].Value } } 

    if ($result) {
        $result
    } else {
        [pscustomobject]@{Source = [string]$source; }
    }

}
| ForEach-Object { 
    $source = [string]$_.Source
    $include = [string]$_.Include

    if ($include) {
        $inludePath = $IncludeFiles | Where-Object { $_ -match "$include.include$" } | Select-Object -First 1
        [pscustomobject]@{Source = [string]$source; SourceDependency = [string]$inludePath }
    } else {
        [pscustomobject]@{Source = [string]$source }
    }

    
}

