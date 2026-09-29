# Add this function to your PowerShell profile after bookpath.exe is on PATH.
# It makes `bp my-bookmark` change the current PowerShell directory.
function bp {
    param(
        [Parameter(ValueFromRemainingArguments = $true)]
        [string[]] $Arguments
    )

    $reservedCommands = @('list', 'update', 'delete', 'help')
    $isLookup = $Arguments.Count -eq 1 -and
                $Arguments[0] -notmatch '^-' -and
                $reservedCommands -notcontains $Arguments[0]

    if ($isLookup) {
        $path = & bookpath @Arguments
        if ($LASTEXITCODE -eq 0 -and $path) {
            Set-Location -LiteralPath $path
        }
        return
    }

    & bookpath @Arguments
}
