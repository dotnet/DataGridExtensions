
function Replace-Version
{
    [cmdletbinding()]
    param (
        [Parameter(Position=0, Mandatory=1, ValueFromPipeline=$true)]
        [string]$text
    )

    return $text -replace ' Version: \d\.\d\.\d\+[a-z0-9]+', ''

}

Get-ChildItem '..\..\docs\html\*.htm' | ForEach {
(Get-Content $_ -Raw | Replace-Version) | Set-Content -Path $_ -Encoding UTF8
}

