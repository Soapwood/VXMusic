$projectpath  = "C:\Users\Tam\git\VXMusic\VXMusicOverlay"
$buildPath    = "C:\Users\Tam\git\VXMusic\VXMusicDesktop\Overlay" 
$logPath      = "C:\Users\Tam\git\VXMusic\BuildLog.txt"

cd "C:\Program Files\Unity\Hub\Editor\2020.3.30f1\Editor"

if (Test-Path -Path $buildPath) {
    # Get all the files in the directory
    $files = Get-ChildItem -Path $buildPath -File -Recurse
    
    # Check if there are any files
    if ($files.Count -gt 0) {
        # Delete all files in the directory
        Remove-Item -Path "$buildPath\*" -Force -Recurse
        Write-Host "All files in the build output directory have been deleted."
    } else {
        Write-Host "The build output directory does not contain any files."
    }
} else {
    Write-Host "The build output directory does not exist."
}

.\Unity.exe -quit -batchmode -projectpath $projectpath -buildWindowsPlayer $buildPath\bin\VXMOverlay.exe -logFile $logPath

Get-Content -Path $logPath