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

$modDll = "D:\Slay The Spire 2\_mod_work\build\AnimeSurvivalMod.dll"
$managedRefs += "-r:$modDll"

$testSource = "D:\Slay The Spire 2\_mod_work\tools\ModHeadlessTests.cs"
$testExe = "D:\Slay The Spire 2\_mod_work\tools\ModHeadlessTests.exe"

Write-Host "Compiling Headless Test Runner..."
& dotnet exec $csc /noconfig /nostdlib /target:exe "/out:$testExe" $testSource $managedRefs

if ($LASTEXITCODE -eq 0) {
    Write-Host "Compilation SUCCESS! Running Headless Tests in background..."
    $runtimeConfig = "D:\Slay The Spire 2\_mod_work\tools\ModHeadlessTests.runtimeconfig.json"
    & dotnet exec --runtimeconfig $runtimeConfig $testExe
    exit $LASTEXITCODE
} else {
    Write-Host "Failed to compile test runner with code $LASTEXITCODE"
    exit 1
}
