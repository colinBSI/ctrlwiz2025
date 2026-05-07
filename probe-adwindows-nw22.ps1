$dll = 'C:\Program Files\Autodesk\Navisworks Manage 2022\AdWindows.dll'
if (-not (Test-Path $dll)) {
    Write-Host "AdWindows.dll not found at NW2022 path"
    exit 1
}
$asm = [System.Reflection.Assembly]::LoadFile($dll)
Write-Host "Loaded: $dll"
$types = $asm.GetTypes()
$btn = $types | Where-Object { $_.Name -eq 'RibbonButton' }
if ($btn) {
    Write-Host "RibbonButton FOUND: $($btn.FullName)"
    $hasId = $btn.GetProperties() | Where-Object { $_.Name -eq 'Id' }
    $hasSize = $btn.GetProperties() | Where-Object { $_.Name -eq 'Size' }
    $hasShowText = $btn.GetProperties() | Where-Object { $_.Name -eq 'ShowText' }
    $hasOrientation = $btn.GetProperties() | Where-Object { $_.Name -eq 'Orientation' }
    Write-Host "Has Id: $($hasId -ne $null)"
    Write-Host "Has Size: $($hasSize -ne $null)"
    Write-Host "Has ShowText: $($hasShowText -ne $null)"
    Write-Host "Has Orientation: $($hasOrientation -ne $null)"
} else {
    Write-Host "RibbonButton NOT found in NW2022 AdWindows"
}
