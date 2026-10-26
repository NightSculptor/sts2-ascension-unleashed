$csc = "C:\Program Files\dotnet\sdk\8.0.408\Roslyn\bincore\csc.dll"

$managedRefs = @()
Get-ChildItem "D:\Slay The Spire 2\data_sts2_windows_x86_64\*.dll" | ForEach-Object {
    try {
        [void][System.Reflection.AssemblyName]::GetAssemblyName($_.FullName)
        $managedRefs += "-r:$($_.FullName)"
    } catch {
        # Native DLL, skip
    }
}

$sourceFiles = @(Get-ChildItem "D:\Slay The Spire 2\_mod_work\src\*.cs" | ForEach-Object { $_.FullName })
$buildFile = "D:\Slay The Spire 2\_mod_work\build\AnimeSurvivalMod.dll"
$outFile = "D:\Slay The Spire 2\mods\AnimeSurvivalMod\AnimeSurvivalMod.dll"
$distFile = "D:\Slay The Spire 2\DIST_FOR_FRIEND\mods\AnimeSurvivalMod\AnimeSurvivalMod.dll"

Write-Host "Compiling $($sourceFiles.Count) source files to $buildFile with $($managedRefs.Count) managed refs..."
& dotnet exec $csc /noconfig /nostdlib /target:library "/out:$buildFile" $sourceFiles $managedRefs

if ($LASTEXITCODE -eq 0) {
    Write-Host "Compilation SUCCESS!"
    
    # Sync to DIST_FOR_FRIEND
    if (Test-Path "D:\Slay The Spire 2\DIST_FOR_FRIEND\mods\AnimeSurvivalMod") {
        Copy-Item $buildFile $distFile -Force
        Write-Host "Synced DLL to DIST_FOR_FRIEND!"
    }

    # Try copying to game mods folder
    try {
        Copy-Item $buildFile $outFile -Force -ErrorAction Stop
        Write-Host "Synced DLL to game mods directory!"
    } catch {
        Write-Host "WARNING: Game is currently running ($outFile is locked). Close the game and run this script to update the game DLL!"
    }
} else {
    Write-Host "Compilation FAILED with code $LASTEXITCODE"
}
